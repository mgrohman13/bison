using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using MattUtil;
using System.Runtime.Serialization;

namespace GalWar
{
    [Serializable]
    public class Game
    {
        #region static

        public static readonly MTRandom Random = new MTRandom();

        public static string AutoSavePath = "../../../auto";

        public static string CamelToSpaces(string str)
        {
            return new Regex("(?<=[a-z])(?<x>[A-Z])|(?<=.)(?<x>[A-Z])(?=[a-z])").Replace(str, " ${x}");
        }

        public static string NumberToRoman(int mark)
        {
            if (mark > 99)
                return mark.ToString();

            int[] values = new int[] { 90, 50, 40, 10, 9, 5, 4, 1 };
            string[] numerals = new string[] { "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

            string result = string.Empty;
            for (int i = 0 ; i < values.Length ; ++i)
                while (mark >= values[i])
                {
                    mark -= values[i];
                    result += numerals[i];
                }
            return result;
        }

        #endregion //static

        #region fields and constructors

        public readonly Graphs Graphs;
        public readonly StoreProd StoreProd;
        public readonly Attack Attack;
        public readonly Defense Defense;

        internal readonly ShipNames ShipNames;

        private readonly List<Player> players;
        private readonly List<Result> deadPlayers, winningPlayers;

        private readonly Dictionary<PointS, SpaceObject> _spaceObjects;
        private readonly List<Tuple<PointS, PointS>> _teleporters;

        [NonSerialized]
        private Stack<IUndoCommand> _undoStack;
        [NonSerialized]
        private Dictionary<Point, Tile> tileCache;

        private PointS _center;

        private byte _currentPlayer;
        private ushort _turn;
        private readonly float _mapDeviation, _planetPct, _anomalyPct;

        public Game(Player.StartingPlayer[] players, double mapSize, double planetPct)
        {
            checked
            {
                int numPlayers = players.Length;

                this._center = new PointS(0, 0);
                this._mapDeviation = (float)mapSize;

                AssertException.Assert(players != null);
                AssertException.Assert(numPlayers > 1);
                AssertException.Assert(numPlayers * 78 < MapSize);
                AssertException.Assert(mapSize < 39);
                AssertException.Assert(planetPct > 0.00013);
                AssertException.Assert(planetPct < 0.013);

                this.StoreProd = new StoreProd();
                this.Attack = new Attack();
                this.Defense = new Defense();

                this.ShipNames = new ShipNames(numPlayers);

                this._spaceObjects = new Dictionary<PointS, SpaceObject>();
                this.players = new List<Player>();
                this._teleporters = new List<Tuple<PointS, PointS>>();
                this.deadPlayers = new List<Result>(numPlayers - 1);
                this.winningPlayers = new List<Result>(numPlayers - 1);

                this._currentPlayer = byte.MaxValue;
                this._turn = 0;

                planetPct *= MapSize;
                this._planetPct = (float)Random.GaussianCapped(planetPct / 104.0, .091, planetPct / 169.0);
                this._anomalyPct = (float)Random.GaussianCapped(this.PlanetPct + MapSize * .00013 + ( numPlayers + 6.5 ) * .013,
                        .21, this.PlanetPct + 0.13);

                double numPlanets = CreateSpaceObjects(numPlayers, planetPct);
                InitPlayers(players, numPlanets);

                AdjustCenter(this.MapDeviation);

                this.Graphs = new Graphs(this);
            }
        }

        public HashSet<SpaceObject> GetSpaceObjects()
        {
            return new HashSet<SpaceObject>(this._spaceObjects.Values);
        }
        private void AddSpaceObject(Point point, SpaceObject spaceObject)
        {
            this._spaceObjects.Add(GetPointS(point), spaceObject);
        }
        private bool RemoveSpaceObject(Point point)
        {
            return this._spaceObjects.Remove(GetPointS(point));
        }
        private bool TryGetSpaceObject(Point point, out SpaceObject spaceObject)
        {
            return this._spaceObjects.TryGetValue(GetPointS(point), out spaceObject);
        }
        public List<Tuple<Point, Point>> GetTeleporters()
        {
            List<Tuple<Point, Point>> teleporters = new List<Tuple<Point, Point>>();
            foreach (Tuple<PointS, PointS> teleporter in this._teleporters)
                teleporters.Add(new Tuple<Point, Point>(new Point(teleporter.Item1.X, teleporter.Item1.Y),
                        new Point(teleporter.Item2.X, teleporter.Item2.Y)));
            return teleporters;
        }
        private void AddTeleporter(Tuple<Point, Point> teleporter)
        {
            this._teleporters.Add(GetPointSTeleporter(teleporter));
        }
        private bool RemoveTeleporter(Tuple<Point, Point> teleporter)
        {
            return this._teleporters.Remove(GetPointSTeleporter(teleporter));
        }
        private static Tuple<PointS, PointS> GetPointSTeleporter(Tuple<Point, Point> teleporter)
        {
            return new Tuple<PointS, PointS>(GetPointS(teleporter.Item1), GetPointS(teleporter.Item2));
        }
        private static PointS GetPointS(Point point)
        {
            checked
            {
                return new PointS((short)point.X, (short)point.Y);
            }
        }

        public Point Center
        {
            get
            {
                return new Point(this._center.X, this._center.Y);
            }
            private set
            {
                checked
                {
                    this._center = new PointS((short)value.X, (short)value.Y);
                }
            }
        }

        private Stack<IUndoCommand> undoStack
        {
            get
            {
                if (this._undoStack == null)
                    this._undoStack = new Stack<IUndoCommand>();
                return this._undoStack;
            }
        }

        private int currentPlayer
        {
            get
            {
                return this._currentPlayer;
            }
            set
            {
                checked
                {
                    this._currentPlayer = (byte)value;
                }
            }
        }
        private int turn
        {
            get
            {
                return this._turn;
            }
            set
            {
                checked
                {
                    this._turn = (ushort)value;
                }
            }
        }
        public double MapDeviation
        {
            get
            {
                return this._mapDeviation;
            }
        }
        public double PlanetPct
        {
            get
            {
                return this._planetPct;
            }
        }
        public double AnomalyPct
        {
            get
            {
                return this._anomalyPct;
            }
        }

        private double CreateSpaceObjects(int numPlayers, double planetPct)
        {
            //first create enough planets for homeworlds
            while (GetPlanets().Count < numPlayers)
                NewPlanet();

            //then create anomalies
            double anomPlanets = 0;
            var anomalies = new Dictionary<Anomaly, double>();
            int startAnomalies = Random.GaussianOEInt(this.AnomalyPct * Consts.StartAnomalies, .39, .13,
                    ( this.AnomalyPct * Consts.StartAnomalies > 1 ) ? 1 : 0);
            for (int a = 0 ; a < startAnomalies ; ++a)
            {
                Anomaly anomaly = CreateAnomaly();

                //keep track of the chances that starting anomlies will become planets
                if (anomaly != null && CheckPlanetDistance(anomaly.Tile))
                {
                    double anomPlanetRate = this.PlanetPct / this.AnomalyPct;
                    foreach (var pair in anomalies)
                        if (Tile.GetDistance(anomaly.Tile, pair.Key.Tile) <= Consts.PlanetDistance)
                            anomPlanetRate *= ( 1 - pair.Value );
                    anomPlanets += anomPlanetRate;
                    if (anomPlanets > planetPct)
                    {
                        anomaly.Tile.SpaceObject = null;
                        anomPlanets -= anomPlanetRate;
                    }
                    else
                    {
                        anomalies.Add(anomaly, anomPlanetRate);
                    }
                }
            }

            //reduce starting planets by the total chances anomalies will be planets
            planetPct -= anomPlanets;
            int startPlanets = Random.GaussianOEInt(planetPct, .13, .091, ( planetPct > 1 ) ? 1 : 0);
            for (int a = 0 ; a < startPlanets ; ++a)
                NewPlanet();

            return GetPlanets().Count + anomPlanets;
        }

        private Planet NewPlanet()
        {
            Tile tile = GetRandomTile();
            if (tile.SpaceObject == null && CheckPlanetDistance(tile))
                return CreatePlanet(tile);
            //dont retry if it cant be placed because of occupied space or existing planet proximity
            return null;
        }

        private void InitPlayers(Player.StartingPlayer[] players, double numPlanets)
        {
            int numPlayers = players.Length;

            int startPop = GetStartInt(Consts.StartPopulation);
            double startSoldiers = GetStartDouble(startPop);
            double startGold = GetStartDouble(Consts.StartGold * numPlayers / numPlanets);
            List<int> startResearch = GetStartResearch();
            //set later based on colony ship costs
            double startProd = -1;

            int index = 0;
            foreach (Player.StartingPlayer player in Random.Iterate<Player.StartingPlayer>(players))
            {
                Planet homeworld = GetHomeworld(startPop);
                double gold = startGold / (double)homeworld.Quality + index * Consts.GetMoveOrderGold(numPlayers);
                this.players.Add(new Player(index, this, player, homeworld, startPop, startSoldiers, gold, startResearch));
                //starting production is based on the highest colony ship design cost
                foreach (ShipDesign design in this.players[index].GetDesigns())
                    if (design.Colony)
                    {
                        startProd = Math.Max(startProd, design.Cost);
                        break;
                    }
                ++index;
            }
            this.ShipNames.EndSetup();

            startProd = GetStartDouble(startProd);
            double addProduction = startProd / ( 1 - Consts.StoreProdLossPct );
            double spendGold = addProduction * Consts.StoreProdLossPct / Consts.ProductionForGold;
            for (this.currentPlayer = 0 ; this.currentPlayer < numPlayers ; ++this.currentPlayer)
            {
                Colony colony = CurrentPlayer.GetColonies()[0];
                colony.AddProduction(addProduction);
                CurrentPlayer.SpendGold(spendGold);
                CurrentPlayer.IncomeTotal += startProd;
            }
        }
        private static List<int> GetStartResearch()
        {
            List<int> research = new List<int>(4);
            research.Add(GetStartInt(Consts.StartResearch * 1 / 4.0));
            research.Add(GetStartInt(Consts.StartResearch * 2 / 4.0));
            research.Add(GetStartInt(Consts.StartResearch * 3 / 4.0));
            research.Add(GetStartInt(Consts.StartResearch * 4 / 4.0));
            //ensure the List is in order despite randomness
            research.Sort();
            return research;
        }
        private Planet GetHomeworld(int startPop)
        {
            List<Planet> homeworlds;
            int homeworldCount;

            //we may need to add another valid homeworld
            while (( homeworldCount = ( homeworlds = GetAvailableHomeworlds(startPop) ).Count ) == 0)
            {
                //we dont want to change the number of planets, so take one out first
                Planet removePlanet = null;
                foreach (Planet planet in Random.Iterate(GetPlanets()))
                {
                    removePlanet = planet;
                    if (removePlanet.Colony == null)
                        break;
                }
                if (removePlanet == null)
                    throw new Exception();
                this.RemovePlanet(removePlanet);

                //try until we add a new one
                while (NewPlanet() == null)
                    ;
            }

            return homeworlds[Random.Next(homeworldCount)];
        }
        private List<Planet> GetAvailableHomeworlds(int startPop)
        {
            //planets can only be used as homeworlds if they have enough quality to support the initial population
            List<Planet> retVal = new List<Planet>();
            foreach (Planet planet in GetPlanets())
                if (planet.Quality > startPop && planet.Colony == null)
                {
                    foreach (Player player in this.players)
                        if (player != null)
                            foreach (Colony colony in player.GetColonies())
                                if (Tile.GetDistance(planet.Tile, colony.Tile) <= Consts.HomeworldDistance)
                                    goto next_planet;
                    retVal.Add(planet);
next_planet:
                    ;
                }
            return retVal;
        }

        private static int GetStartInt(double avg)
        {
            return Random.Round(GetStartDouble(avg));
        }
        private static double GetStartDouble(double avg)
        {
            return Random.GaussianCapped(avg, Consts.StartRndm, avg * Consts.StartMinMult);
        }

        #endregion //fields and constructors

        #region internal

        internal double AvgResearch
        {
            get
            {
                double avgResearch = 0;
                foreach (Player player in this.players)
                    avgResearch += 1 * player.ResearchDisplay + 2 * player.Research + 4 * player.LastResearched;
                return avgResearch / (double)this.players.Count / 7.0;
            }
        }

        internal void RemovePlanet(Planet planet)
        {
            planet.Tile.SpaceObject = null;
        }

        internal void KillPlayer(Player player)
        {
            RemovePlayer(player);
            this.deadPlayers.Add(new Result(player, false));
            //if one player wipes out the other in a 1 vs 1 situation, they still get a victory bonus
            if (this.players.Count == 1)
                this.winningPlayers.Add(new Result(this.players[0], true));
        }

        private void RemovePlayer(Player player)
        {
            int i = this.players.IndexOf(player);
            if (this.currentPlayer > i)
                --this.currentPlayer;
            else if (this.currentPlayer == i)
                throw new Exception();

            this.players.Remove(player);
        }

        #endregion //internal

        #region public

        public double MapSize
        {
            get
            {
                return 9.1 * this.MapDeviation * this.MapDeviation;
            }
        }

        public Player CurrentPlayer
        {
            get
            {
                if (this.currentPlayer == byte.MaxValue)
                    throw new InvalidOperationException();
                return this.players[this.currentPlayer];
            }
        }

        public int Turn
        {
            get
            {
                return this.turn;
            }
        }

        public void StartGame(IEventHandler handler)
        {
            handler = new HandlerWrapper(handler, this);

            foreach (Player p in players)
                this.Graphs.StartTurn(p);
            this.Graphs.Increment(this);

            this.currentPlayer = 0;
            this.turn = 1;

            foreach (Player p in players)
                p.SetGame(this);

            StartPlayerTurn(handler);
            CurrentPlayer.PlayTurn(handler, new List<Anomaly>());
        }

        internal void SetSpaceObject(int x, int y, SpaceObject spaceObject)
        {
            Point point = new Point(x, y);
            if (spaceObject == null)
            {
                if (!RemoveSpaceObject(point))
                    throw new Exception();
            }
            else
            {
                AddSpaceObject(point, spaceObject);
            }
        }

        public Tile GetTile(int x, int y)
        {
            return GetTile(new Point(x, y));
        }
        public Tile GetTile(Point point)
        {
            if (this.tileCache == null)
                this.tileCache = new Dictionary<Point, Tile>();

            Tile tile;
            SpaceObject spaceObject;
            if (!tileCache.TryGetValue(point, out tile))
            {
                if (TryGetSpaceObject(point, out spaceObject))
                    tile = spaceObject.Tile;
                else
                    tile = new Tile(this, point);
                tileCache.Add(point, tile);
            }
            return tile;
        }

        public SpaceObject GetSpaceObject(int x, int y)
        {
            return GetSpaceObject(new Point(x, y));
        }
        public SpaceObject GetSpaceObject(Point point)
        {
            SpaceObject spaceObject;
            TryGetSpaceObject(point, out spaceObject);
            return spaceObject;
        }

        public Tuple<Point, Point> GetTeleporter(Point point, out int number)
        {
            number = 0;
            foreach (Tuple<Point, Point> teleporter in GetTeleporters())
            {
                ++number;
                if (teleporter.Item1 == point || teleporter.Item2 == point)
                    return teleporter;
            }
            number = -1;
            return null;
        }

        public Dictionary<Player, double> GetResearch()
        {
            Player[] players = GetResearchDisplayOrder();
            Dictionary<Player, double> retVal = new Dictionary<Player, double>(players.Length);
            for (int a = 0 ; a < players.Length ; ++a)
                retVal.Add(players[a], players[a].ResearchDisplay / players[0].ResearchDisplay * 100);
            return retVal;
        }
        internal Player[] GetResearchDisplayOrder()
        {
            return GetOrder(delegate(Player player)
            {
                return player.ResearchDisplay;
            });
        }
        internal Player[] GetRealResearchOrder()
        {
            return GetOrder(delegate(Player player)
            {
                return player.Research;
            });
        }
        private Player[] GetOrder(Func<Player, double> Func)
        {
            Player[] players = this.players.ToArray();
            Array.Sort<Player>(players, delegate(Player p1, Player p2)
            {
                //descending sort
                return Math.Sign(Func(p2) - Func(p1));
            });
            return players;
        }

        public ReadOnlyCollection<Player> GetPlayers()
        {
            return this.players.AsReadOnly();
        }

        public HashSet<Planet> GetPlanets()
        {
            HashSet<Planet> planets = new HashSet<Planet>();
            Planet planet;
            foreach (SpaceObject spaceObject in GetSpaceObjects())
                if (( planet = spaceObject as Planet ) != null)
                    planets.Add(planet);
            return planets;
        }

        public List<Anomaly> EndTurn(IEventHandler handler)
        {
            return EndTurn(handler, false);
        }
        internal List<Anomaly> EndTurn(IEventHandler handler, bool allowAI)
        {
            handler = new HandlerWrapper(handler, this);

            AssertException.Assert(allowAI || CurrentPlayer.AI == null);

            CurrentPlayer.EndTurn(handler);
            Graphs.EndTurn(CurrentPlayer);

            if (++this.currentPlayer >= this.players.Count)
                NewRound();

            List<Anomaly> anomalies = CreateAnomalies();
            RemoveTeleporters();
            AdjustCenter(1 / (double)this.players.Count);

            StartPlayerTurn(handler);

            if (CurrentPlayer.AI == null)
                AutoSave();

            CurrentPlayer.PlayTurn(handler, anomalies);
            return anomalies;
        }

        public void AutoSave()
        {
            if (AutoSavePath != null)
                TBSUtil.SaveGame(this, AutoSavePath, turn + ".gws");
        }

        private void NewRound()
        {
            //just so an exception is thrown if current player is mistakenly used
            this.currentPlayer = byte.MaxValue;

            CheckResearchVictory();
            RandMoveOrder();

            foreach (Player player in players)
                player.NewRound();

            this.Graphs.Increment(this);

            ++this.turn;
            this.currentPlayer = 0;
        }

        private void CheckResearchVictory()
        {
            //use real research
            Player[] researchOrder = GetRealResearchOrder();
            if (researchOrder.Length > 1)
            {
                Player winner = researchOrder[0];
                double chance = GetResearchVictoryChance(winner.Research, researchOrder[1].Research);
                if (chance > 0 && Random.Bool(chance))
                {
                    winner.Destroy();
                    RemovePlayer(winner);
                    this.winningPlayers.Add(new Result(winner, true));
                }
            }
        }
        public double GetResearchVictoryChance(out Player winner)
        {
            //use research display values
            Player[] researchOrder = GetResearchDisplayOrder();
            if (researchOrder.Length > 1)
            {
                winner = researchOrder[0];
                return GetResearchVictoryChance(winner.ResearchDisplay, researchOrder[1].ResearchDisplay);
            }
            winner = null;
            return 0;
        }
        private double GetResearchVictoryChance(double first, double second)
        {
            //research victory can happen when the top player exceeds a certain multiple of the second place player
            double mult = first / second;
            if (mult > Consts.ResearchVictoryMin)
            {
                double chance = ( mult - Consts.ResearchVictoryMin ) / ( Consts.ResearchVictoryMult - Consts.ResearchVictoryMin );
                if (chance > 0.5)
                    chance /= ( chance + 0.5 );
                return Math.Pow(chance, Consts.ResearchVictoryPow);
            }
            return 0;
        }

        private void RandMoveOrder()
        {
            Dictionary<Player, int> playerGold = TBSUtil.RandMoveOrder<Player>(Random, this.players, Consts.MoveOrderShuffle);
            if (playerGold.Count > 0)
            {
                double moveOrderGold = Consts.GetMoveOrderGold(this.players.Count);
                foreach (KeyValuePair<Player, int> pair in playerGold)
                {
                    Player player = pair.Key;
                    //player cant move up any further
                    double gold = moveOrderGold * pair.Value;
                    player.AddGold(gold);
                    player.IncomeTotal += gold;
                }
            }
        }

        private void RemoveTeleporters()
        {
            List<Tuple<Point, Point>> teleporters = GetTeleporters();
            if (teleporters.Count > 0)
            {
                const double avgTurns = 65;
                double chance = 1 - Math.Pow(avgTurns / ( avgTurns + teleporters.Count ), 1 / (double)this.players.Count);
                if (Random.Bool(chance))
                    RemoveTeleporter(teleporters[Random.Next(teleporters.Count)]);
            }
        }

        internal bool CreateTeleporter(IEventHandler handler, Tile tile, Tile target)
        {
            double chance = GetTeleporters().Count + 1;
            //check if the tiles are too close to be useful or if either tile already has a teleporter
            if (Tile.GetDistance(tile, target) > 1 && tile.Teleporter == null && target.Teleporter == null && Random.Bool(1.0 / chance))
            {
                //check this will not make any planets be too close
                HashSet<Planet> planets = new HashSet<Planet>();
                foreach (Planet p1 in planets)
                {
                    int dist = Consts.PlanetDistance - Tile.GetDistance(tile, p1.Tile);
                    if (dist > 0)
                        foreach (Planet p2 in planets)
                            if (p1 != p2 && Tile.GetDistance(target, p2.Tile) < dist)
                                return false;
                }

                //check and make sure enemies cannot be attacked/invaded
                foreach (Player p in this.players)
                {
                    foreach (Colony c in p.GetColonies())
                        if (!CheckAttInvPlayers(c.Planet, true, tile, target))
                            return false;
                    if (!p.IsTurn)
                        foreach (Ship s in p.GetShips())
                            if (!CheckAttInvPlayers(s, false, tile, target))
                                return false;

                    handler.Explore(Anomaly.AnomalyType.Wormhole);

                    CreateTeleporter(tile, target);
                    return true;
                }
            }
            return false;
        }
        private bool CheckAttInvPlayers(SpaceObject obj, bool inv, Tile t1, Tile t2)
        {
            HashSet<SpaceObject> before = Anomaly.GetAttInv(obj.Tile, inv);
            Tuple<Point, Point> teleporter = CreateTeleporter(t1, t2);
            HashSet<SpaceObject> after = Anomaly.GetAttInv(obj.Tile, inv);
            RemoveTeleporter(teleporter);
            foreach (SpaceObject other in after)
                if (other.Player != obj.Player && !before.Contains(other))
                    return false;
            return true;
        }
        private Tuple<Point, Point> CreateTeleporter(Tile t1, Tile t2)
        {
            Tuple<Point, Point> teleporter = new Tuple<Point, Point>(t1.Point, t2.Point);
            AddTeleporter(teleporter);
            return teleporter;
        }

        internal Planet CreateAnomalyPlanet(IEventHandler handler, Tile tile)
        {
            if (Random.Bool(this.PlanetPct / this.AnomalyPct) && CheckPlanetDistance(tile))
            {
                handler.Explore(Anomaly.AnomalyType.NewPlanet);
                return CreatePlanet(tile);
            }
            return null;
        }
        internal bool CheckPlanetDistance(Tile tile)
        {
            foreach (Planet planet in GetPlanets())
                if (Tile.GetDistance(tile, planet.Tile) <= Consts.PlanetDistance)
                    return false;
            return true;
        }
        internal Planet CreatePlanet(Tile tile)
        {
            return new Planet(tile);
        }

        private List<Anomaly> CreateAnomalies()
        {
            List<Anomaly> anomalies = new List<Anomaly>();
            int create = Random.OEInt(this.AnomalyPct / (double)this.players.Count);
            for (int a = 0 ; a < create ; ++a)
            {
                Anomaly anomaly = CreateAnomaly();
                if (anomaly != null)
                    anomalies.Add(anomaly);
            }
            return anomalies;
        }
        private Anomaly CreateAnomaly()
        {
            return CreateAnomaly(GetRandomTile());
        }
        internal Anomaly CreateAnomaly(Tile tile)
        {
            if (tile.SpaceObject == null)
                return new Anomaly(tile);
            return null;
        }

        private void AdjustCenter(double avg)
        {
            int amt = Random.OEInt(avg * MapDeviation / 16.9);
            for (int a = 0 ; a < amt ; ++a)
                AdjustCenter();
        }
        private void AdjustCenter()
        {
            double shipWeight = 0, popWeight = 0;
            foreach (SpaceObject spaceObject in this.GetSpaceObjects())
            {
                Ship ship = ( spaceObject as Ship );
                Planet planet = ( spaceObject as Planet );
                if (ship != null)
                {
                    shipWeight += GetShipWeight(ship);
                    popWeight += ship.Population;
                }
                else if (planet != null && planet.Colony != null)
                {
                    popWeight += planet.Colony.Population;
                }
            }

            //anomalies weigh less than even the smallest possible planet
            const double AnomalyWeight = Consts.PlanetConstValue / 1.3;
            //the total weight of all ships is always less than even a single anomaly
            shipWeight = AnomalyWeight / 1.3 / shipWeight;
            //population is weighted at its square root
            if (popWeight != 0)
                popWeight = 1 / Math.Sqrt(popWeight);

            var directions = new Dictionary<Tile, double>();

            Tile centerTile = GetTile(this.Center);
            foreach (SpaceObject spaceObject in this.GetSpaceObjects())
            {
                int distance = Tile.GetDistance(centerTile, spaceObject.Tile);
                if (distance > 0)
                {
                    double weight;
                    Ship ship = ( spaceObject as Ship );
                    Planet planet = ( spaceObject as Planet );
                    if (ship != null)
                    {
                        weight = shipWeight * GetShipWeight(ship);
                        weight += popWeight * ship.Population;
                    }
                    else if (planet != null)
                    {
                        weight = planet.PlanetValue;
                        if (planet.Colony != null)
                            weight += popWeight * planet.Colony.Population;
                    }
                    else if (spaceObject is Anomaly)
                    {
                        weight = AnomalyWeight;
                    }
                    else
                    {
                        throw new Exception();
                    }

                    HashSet<Tile> neighbors = Tile.GetNeighbors(centerTile);
                    neighbors.RemoveWhere(delegate(Tile neighbor)
                    {
                        return ( Tile.GetDistance(neighbor, spaceObject.Tile) >= distance );
                    });

                    weight *= distance / (double)neighbors.Count;

                    foreach (Tile neighbor in neighbors)
                    {
                        double value;
                        directions.TryGetValue(neighbor, out value);
                        directions[neighbor] = value + weight;
                    }
                }
            }

            double max = double.MinValue;
            Tile newCenter = null;
            foreach (var pair in Random.Iterate(directions))
            {
                double value = Random.GaussianCapped(pair.Value * pair.Value, .26);
                if (value > max)
                {
                    max = value;
                    newCenter = pair.Key;
                }
            }

            this.Center = newCenter.Point;
        }
        private static double GetShipWeight(Ship ship)
        {
            return ship.GetStrength() * Math.Sqrt(ship.HP / (double)ship.MaxHP);
        }

        internal Tile GetRandomTile()
        {
            return GetRandomTile(GetTile(this.Center), this.MapDeviation);
        }
        internal Tile GetRandomTile(Tile center, double stdDev)
        {
            return GetDistanceTile(center, Math.Abs(Random.GaussianInt(stdDev)));
        }
        private Tile GetDistanceTile(Tile center, int dist)
        {
            int minX, minY, maxX, maxY;
            GetTileDistances(center, dist, out minX, out minY, out maxX, out maxY, GetTeleporters());

            //Expected iterations is n/(3*(SQRT(n)-1)) where n=(maxX-minX+1)*(maxY-minY+1), or O(SQRT(n)).
            //Because n is O(dist^2), the operation time of this loop is O(dist).
            //Teleporters may affect the value of n, but not significantly.
            foreach (Point p in Random.Iterate(minX, maxX, minY, maxY))
            {
                Tile test = GetTile(p);
                if (Tile.GetDistance(center, test) == dist)
                    return test;
            }

            throw new Exception();
        }
        private void GetTileDistances(Tile center, int dist, out int minX, out int minY, out int maxX, out int maxY, List<Tuple<Point, Point>> teleporters)
        {
            minX = center.X - dist;
            minY = center.Y - dist;
            maxX = center.X + dist;
            maxY = center.Y + dist;
            foreach (var teleporter in teleporters)
            {
                var subset = new List<Tuple<Point, Point>>(teleporters);
                subset.Remove(teleporter);
                GetTeleporterDistances(center, dist, teleporter.Item1, teleporter.Item2, ref minX, ref minY, ref maxX, ref maxY, subset);
                GetTeleporterDistances(center, dist, teleporter.Item2, teleporter.Item1, ref minX, ref minY, ref maxX, ref maxY, subset);
            }
        }
        private void GetTeleporterDistances(Tile center, int dist, Point p1, Point p2, ref int minX, ref int minY, ref int maxX, ref int maxY, List<Tuple<Point, Point>> teleporters)
        {
            int telDist = Tile.GetDistance(center, GetTile(p1));
            if (telDist < dist)
            {
                int minX2, minY2, maxX2, maxY2;
                GetTileDistances(GetTile(p2), dist - telDist - 1, out minX2, out minY2, out maxX2, out maxY2, teleporters);
                minX = Math.Min(minX, minX2);
                minY = Math.Min(minY, minY2);
                maxX = Math.Max(maxX, maxX2);
                maxY = Math.Max(maxY, maxY2);
            }
        }

        private void StartPlayerTurn(IEventHandler handler)
        {
            Graphs.StartTurn(CurrentPlayer);

            ClearUndoStack();
            CurrentPlayer.StartTurn(handler);
        }

        public void SaveGame(string filePath)
        {
            TBSUtil.SaveGame(this, filePath);
        }

        public static Game LoadGame(string filePath)
        {
            Game game = TBSUtil.LoadGame<Game>(filePath);
            game.OnDeserialization();
            return game;
        }
        private void OnDeserialization()
        {
            //objects will be re-added as each tile is set
            var temp = new List<KeyValuePair<PointS, SpaceObject>>(this._spaceObjects);
            this._spaceObjects.Clear();

            //Tiles are not serialized so we go through and create a new one for each SpaceObject
            foreach (var pair in temp)
                pair.Value.OnDeserialization(new Tile(this, new Point(pair.Key.X, pair.Key.Y)));
        }

        public List<Result> GetGameResult()
        {
            AssertException.Assert(this.players.Count == 1);

            List<Result> result = new List<Result>(this.winningPlayers.Count + 1 + this.deadPlayers.Count);

            bool foundCurrent = false;
            foreach (Result victory in this.winningPlayers)
            {
                result.Add(victory);
                //if one of the last two players gained a research victory, we will need to manually add the loser
                foundCurrent = ( victory.Player == this.players[0] );
            }

            if (!foundCurrent)
                result.Add(new Result(this.players[0], false));

            for (int i = this.deadPlayers.Count ; --i > -1 ; )
                result.Add(this.deadPlayers[i]);

            //add in the final point score
            Result.Finalize(result);

            return result;
        }

        #endregion //   public

        #region Undo

        public bool CanUndo()
        {
            return ( undoStack.Count > 0 );
        }

        public Tile Undo(IEventHandler handler)
        {
            handler = new HandlerWrapper(handler, this, false);
            AssertException.Assert(CanUndo());

            return undoStack.Pop().Undo();
        }

        internal void ClearUndoStack()
        {
            undoStack.Clear();
        }

        internal void PushUndoCommand(IUndoCommand undoCommand)
        {
            undoStack.Push(undoCommand);
        }

        internal delegate Tile UndoMethod<T>(T arg);
        internal delegate Tile UndoMethod<T1, T2>(T1 arg1, T2 arg2);
        internal delegate Tile UndoMethod<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
        internal delegate Tile UndoMethod<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
        internal delegate Tile UndoMethod<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
        internal delegate Tile UndoMethod<T1, T2, T3, T4, T5, T6, T7, T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

        internal interface IUndoCommand
        {
            Tile Undo();
        }

        internal class UndoCommand<T> : IUndoCommand
        {
            private readonly UndoMethod<T> UndoMethod;
            private readonly T arg;

            public UndoCommand(UndoMethod<T> UndoMethod, T arg)
            {
                this.UndoMethod = UndoMethod;
                this.arg = arg;
            }

            public Tile Undo()
            {
                return UndoMethod(arg);
            }
        }

        internal class UndoCommand<T1, T2> : UndoCommand<Tuple<T1, T2>>
        {
            public UndoCommand(UndoMethod<T1, T2> UndoMethod, T1 arg1, T2 arg2)
                : base(delegate(Tuple<T1, T2> args)
                    {
                        return UndoMethod(args.Item1, args.Item2);
                    }, new Tuple<T1, T2>(arg1, arg2))
            {
            }
        }
        internal class UndoCommand<T1, T2, T3> : UndoCommand<Tuple<T1, T2, T3>>
        {
            public UndoCommand(UndoMethod<T1, T2, T3> UndoMethod, T1 arg1, T2 arg2, T3 arg3)
                : base(delegate(Tuple<T1, T2, T3> args)
                    {
                        return UndoMethod(args.Item1, args.Item2, args.Item3);
                    }, new Tuple<T1, T2, T3>(arg1, arg2, arg3))
            {
            }
        }
        internal class UndoCommand<T1, T2, T3, T4> : UndoCommand<Tuple<T1, T2, T3, T4>>
        {
            public UndoCommand(UndoMethod<T1, T2, T3, T4> UndoMethod, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
                : base(delegate(Tuple<T1, T2, T3, T4> args)
                    {
                        return UndoMethod(args.Item1, args.Item2, args.Item3, args.Item4);
                    }, new Tuple<T1, T2, T3, T4>(arg1, arg2, arg3, arg4))
            {
            }
        }
        internal class UndoCommand<T1, T2, T3, T4, T5, T6> : UndoCommand<Tuple<T1, T2, T3, T4, T5, T6>>
        {
            public UndoCommand(UndoMethod<T1, T2, T3, T4, T5, T6> UndoMethod, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
                : base(delegate(Tuple<T1, T2, T3, T4, T5, T6> args)
                {
                    return UndoMethod(args.Item1, args.Item2, args.Item3, args.Item4, args.Item5, args.Item6);
                }, new Tuple<T1, T2, T3, T4, T5, T6>(arg1, arg2, arg3, arg4, arg5, arg6))
            {
            }
        }
        internal class UndoCommand<T1, T2, T3, T4, T5, T6, T7, T8> : UndoCommand<Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>>>
        {
            public UndoCommand(UndoMethod<T1, T2, T3, T4, T5, T6, T7, T8> UndoMethod, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
                : base(delegate(Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>> args)
                    {
                        return UndoMethod(args.Item1, args.Item2, args.Item3, args.Item4, args.Item5, args.Item6, args.Item7, args.Rest.Item1);
                    }, new Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>>(arg1, arg2, arg3, arg4, arg5, arg6, arg7, new Tuple<T8>(arg8)))
            {
            }
        }

        #endregion Undo

        #region Result

        [Serializable]
        public class Result
        {
            public readonly Player Player;

            private sbyte _points;

            internal Result(Player player, bool won)
            {
                checked
                {
                    this.Player = player;
                    //extra points are awarded for gaining a research victory as quickly as possible
                    double mult = Math.Pow(player.Game.MapSize, Consts.PointsTilesPower) / (double)player.Game.Turn;
                    this._points = (sbyte)Random.Round(mult * ( won ? Consts.WinPointsMult : Consts.LosePointsMult ));
                }
            }
            public int Points
            {
                get
                {
                    return _points;
                }
                private set
                {
                    checked
                    {
                        _points = (sbyte)value;
                    }
                }
            }

            internal static void Finalize(List<Result> results)
            {
                int points = 0;
                int add = -1;
                int min = int.MaxValue;
                //adds in (x^2+x)/2 points, where x is the inverse index
                for (int i = results.Count ; --i > -1 ; )
                {
                    int newPoints = results[i].Points + ( points += ( ++add ) );
                    results[i].Points = newPoints;
                    min = Math.Min(min, newPoints);
                }
                foreach (Result result in results)
                    result.Points -= min;
            }
        }

        #endregion //Result
    }
}
