using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using MattUtil;
using Rectangle = System.Drawing.Rectangle;

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

        public static string NumberToRoman(int number)
        {
            string retVal = string.Empty;
            if (number < 0)
            {
                retVal = "-";
                number = -number;
            }

            if (number > 3999 || number == 0)
            {
                retVal += number.ToString();
            }
            else
            {
                int[] values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
                string[] numerals = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

                for (int i = 0 ; i < values.Length ; ++i)
                    while (number >= values[i])
                    {
                        number -= values[i];
                        retVal += numerals[i];
                    }
            }
            return retVal;
        }

        #endregion //static

        #region fields and constructors

        private Graphs _graphs;
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

        private PointS _center;

        private readonly uint _id;

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
                AssertException.Assert(numPlayers * 39 < MapSize);
                AssertException.Assert(mapSize < 39);
                AssertException.Assert(planetPct > .13);
                AssertException.Assert(planetPct < .65);

                this.StoreProd = new StoreProd();
                this.Attack = new Attack();
                this.Defense = new Defense();

                this.ShipNames = new ShipNames(numPlayers);

                this._spaceObjects = new Dictionary<PointS, SpaceObject>();
                this.players = new List<Player>(numPlayers);
                this._teleporters = new List<Tuple<PointS, PointS>>();
                this.deadPlayers = new List<Result>(numPlayers - 1);
                this.winningPlayers = new List<Result>(numPlayers - 1);

                this._id = Random.NextUInt();

                this._currentPlayer = byte.MaxValue / 2;
                this._turn = 0;

                planetPct *= Math.Sqrt(MapSize);
                this._planetPct = (float)Random.GaussianCapped(planetPct / 210.0, .078, planetPct / 260.0);
                double min = this.PlanetPct + 0.13;
                this._anomalyPct = (float)Random.GaussianCapped(min + MapSize * .000169 + ( numPlayers + 6.5 ) * .013, .169, min);

                CreateSpaceObjects(numPlayers, planetPct);
                InitPlayers(players);

                AdjustCenter(13);

                this._graphs = new Graphs(this);
            }
        }

        public Graphs Graphs
        {
            get
            {
                return this._graphs;
            }
            private set
            {
                this._graphs = value;
            }
        }

        public IEnumerable<SpaceObject> GetSpaceObjects()
        {
            return this._spaceObjects.Select(pair => pair.Value);
        }
        private void AddSpaceObject(int x, int y, SpaceObject spaceObject)
        {
            checked
            {
                this._spaceObjects.Add(GetPointS(x, y), spaceObject);
            }
        }
        private bool RemoveSpaceObject(int x, int y)
        {
            checked
            {
                return this._spaceObjects.Remove(GetPointS(x, y));
            }
        }
        private bool TryGetSpaceObject(int x, int y, out SpaceObject spaceObject)
        {
            checked
            {
                return this._spaceObjects.TryGetValue(GetPointS(x, y), out spaceObject);
            }
        }
        public List<Tuple<Tile, Tile>> GetTeleporters()
        {
            checked
            {
                return _teleporters.ConvertAll(teleporter => new Tuple<Tile, Tile>(
                        GetTile(teleporter.Item1.X, teleporter.Item1.Y), GetTile(teleporter.Item2.X, teleporter.Item2.Y)));
            }
        }
        private void AddTeleporter(Tuple<Tile, Tile> teleporter)
        {
            checked
            {
                this._teleporters.Add(GetPointSTeleporter(teleporter));
            }
        }
        private bool RemoveTeleporter(Tuple<Tile, Tile> teleporter)
        {
            checked
            {
                return this._teleporters.Remove(GetPointSTeleporter(teleporter));
            }
        }
        private static Tuple<PointS, PointS> GetPointSTeleporter(Tuple<Tile, Tile> teleporter)
        {
            checked
            {
                return new Tuple<PointS, PointS>(GetPointS(teleporter.Item1), GetPointS(teleporter.Item2));
            }
        }
        public Tile Center
        {
            get
            {
                checked
                {
                    return GetTile(this._center.X, this._center.Y);
                }
            }
            private set
            {
                checked
                {
                    this._center = GetPointS(value);
                }
            }
        }
        internal static PointS GetPointS(Tile tile)
        {
            checked
            {
                return GetPointS(tile.X, tile.Y);
            }
        }
        private static PointS GetPointS(int x, int y)
        {
            checked
            {
                return new PointS((short)x, (short)y);
            }
        }

        private Stack<IUndoCommand> undoStack
        {
            get
            {
                return ( this._undoStack ?? ( this._undoStack = new Stack<IUndoCommand>() ) );
            }
        }

        private uint ID
        {
            get
            {
                return this._id;
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

        private void CreateSpaceObjects(int numPlayers, double planetPct)
        {
            //temporary starting teleporters to space out initial map generation somewhat
            double avgTeleporters = Math.Pow(MapSize, .26) / Math.PI;
            int teleporters = Random.GaussianOEInt(avgTeleporters, .065, .065, ( ( avgTeleporters > 1 ) ? 1 : 0 ));
            for (int a = 0 ; a < teleporters ; ++a)
            {
                Tile t1 = GetRandomTile(), t2 = GetRandomTile();
                if (CanCreateTeleporter(t1, t2))
                    CreateTeleporter(t1, t2);
            }

            //first create homeworlds
            while (GetPlanets().Count < numPlayers)
                NewPlanet();
            //starting planets are actually just chances to create one after homeworlds
            int startPlanets = Random.GaussianOEInt(planetPct, .13, .091, ( ( planetPct > 1 ) ? 1 : 0 ));
            for (int a = 0 ; a < startPlanets ; ++a)
                NewPlanet();

            //starting anomalies
            double anomalyPct = this.AnomalyPct * Consts.StartAnomalies;
            int startAnomalies = Random.GaussianOEInt(anomalyPct, .39, .13, ( ( anomalyPct > 1 ) ? 1 : 0 ));
            for (int a = 0 ; a < startAnomalies ; ++a)
                CreateAnomaly();
        }

        private Planet NewPlanet()
        {
            Tile tile = GetRandomTile();
            if (tile.SpaceObject == null && Random.Bool(GetPlanetChance(tile, false)))
                return CreatePlanet(tile);
            return null;
        }

        private void InitPlayers(Player.StartingPlayer[] players)
        {
            int numPlayers = players.Length;

            int startPop = GetStartInt(Consts.StartPopulation);
            List<int> startResearch = GetStartResearch();

            //randomize turn order and init players
            foreach (int id in Random.Iterate(numPlayers))
                this.players.Add(new Player(id, this, players[id], GetHomeworld(startPop), startPop, startResearch));
            ShipNames.EndSetup();

            double startDefense = GetStartDouble(startPop);

            //starting gold is based on the number and value of initial planets, must happen after GetHomeworld planet shuffling
            double startGold = Consts.StartGold;
            //remove temporary starting teleporters to allow actual distance calculations
            this._teleporters.Clear();
            //homeworlds count as single planets regardless of quality, anomalies count as their chance of being a planet,
            //and actual current uncolonized planets count as their planet value
            double numPlanets = numPlayers + CountAnomPlanets() + GetPlanets().Where(planet => planet.Colony == null)
                    .Sum(planet => planet.PlanetValue / ( Consts.AverageQuality + Consts.PlanetConstValue ));
            //divide starting gold by the number of planets per player, and randomize 
            startGold = GetStartDouble(startGold * numPlayers / numPlanets);

            //starting production is based initial ship design costs
            double startProd = 0, count = 0, max = double.MinValue;
            foreach (Player player in this.players)
                foreach (ShipDesign design in player.GetDesigns())
                {
                    //colony ships are weighted more than other ships
                    double weight = design.Colony ? 1.69 : 1;
                    startProd += design.Cost * weight;
                    count += weight;
                    //non-conoly ships only have a half-chance of increasing the maximum
                    if (design.Cost > max && ( design.Colony || Random.Bool() ))
                        max = design.Cost;
                }
            //average the maximum and average design cost, reduce by StartGoldProdPct, and randomize 
            startProd = GetStartDouble(( startProd / count + max ) / 2.0 * ( 1 - Consts.StartGoldProdPct ));

            for (this.currentPlayer = 0 ; this.currentPlayer < numPlayers ; ++this.currentPlayer)
            {
                //will always have exactly one colony at this point, the homeworld
                Colony homeworld = CurrentPlayer.GetColonies()[0];

                //starting soldiers and defense
                homeworld.BuildPlanetDefense(startDefense, true);

                //starting gold is divided by each indivual player's homeworld quality
                double addGold = startGold / (double)homeworld.Planet.Quality;

                //some starting gold automatically turns into production
                double addProduction = addGold * Consts.StartGoldProdPct;
                addGold -= addProduction;

                //starting production
                addProduction += startProd;

                //other gold balancing
                addGold += currentPlayer * Consts.GetMoveOrderGold(numPlayers) +
                        startDefense - ( homeworld.PlanetDefenseCostPerHP * homeworld.HP + homeworld.Soldiers * Consts.ProductionForSoldiers );

                //calculations to offset AddProduction when currently building StoreProd
                addProduction /= ( 1 - Consts.StoreProdLossPct );
                addGold -= addProduction * Consts.StoreProdLossPct / Consts.ProductionForGold;

                //actually add in starting gold and production
                CurrentPlayer.AddGold(addGold);
                homeworld.AddProduction(addProduction);

                //calculate current income total
                CurrentPlayer.IncomeTotal += CurrentPlayer.TotalGold + homeworld.production;

                homeworld.ProdGuess = homeworld.production;
            }
        }
        private static List<int> GetStartResearch()
        {
            var research = new List<int> {
                GetStartInt(Consts.StartResearch * 1 / 4.0),
                GetStartInt(Consts.StartResearch * 2 / 4.0),
                GetStartInt(Consts.StartResearch * 3 / 4.0),
                GetStartInt(Consts.StartResearch * 4 / 4.0),
            };
            //ensure the List is in order despite randomness
            research.Sort();
            return research;
        }
        private Planet GetHomeworld(int startPop)
        {
            List<Planet> homeworlds;

            //we may need to add another valid homeworld
            while (( homeworlds = GetAvailableHomeworlds(startPop) ).Count == 0)
            {
                //we dont want to change the number of planets, so take one out first
                this.RemovePlanet(Random.SelectValue(GetPlanets().Where(planet => planet.Colony == null)));
                //try until we add a new one
                while (NewPlanet() == null)
                    ;
            }

            return Random.SelectValue(homeworlds);
        }
        private List<Planet> GetAvailableHomeworlds(int startPop)
        {
            var colonies = this.players.Where(player => player != null).SelectMany(player => player.GetColonies()).ToList();
            //planets can only be used as homeworlds if they have enough quality to support the initial population
            return GetPlanets().Where(planet => planet.Quality > startPop && planet.Colony == null
                    //and are far enough away from other homeworlds
                    && colonies.All(colony => ( Tile.GetDistance(planet.Tile, colony.Tile) > Consts.HomeworldDistance ))).ToList();
        }
        private double CountAnomPlanets()
        {
            var anomalies = new Dictionary<Anomaly, double>();
            //'explore' anomalies in random order
            foreach (Anomaly anomaly in Random.Iterate(GetSpaceObjects().OfType<Anomaly>()))
            {
                int planetDist = GetPlanets().Min(planet => Tile.GetDistance(anomaly.Tile, planet.Tile));
                //must be outside of PlanetDistance to have any chance of being a planet
                if (planetDist > Consts.PlanetDistance)
                {
                    //previously 'explored' anomalies have their chance of providing a shorter distance
                    var distanceChances = new SortedDictionary<int, double>();
                    foreach (var pair in anomalies)
                    {
                        int dist = Tile.GetDistance(anomaly.Tile, pair.Key.Tile);
                        if (dist < planetDist)
                        {
                            double value;
                            distanceChances.TryGetValue(dist, out value);
                            distanceChances[dist] = 1 - ( ( 1 - value ) * ( 1 - pair.Value ) );
                        }
                    }

                    //calculate in order of shortest distances first (longer distance chances don't matter if a closer one happens)
                    double chance = 1, anomPlanetRate = 0;
                    foreach (var pair in distanceChances)
                    {
                        anomPlanetRate += chance * pair.Value * GetPlanetChance(pair.Key, true);
                        chance *= ( 1 - pair.Value );
                    }
                    anomPlanetRate += chance * GetPlanetChance(planetDist, true);

                    //store final chance to become a planet
                    anomalies.Add(anomaly, anomPlanetRate);
                }
            }

            return anomalies.Values.Sum();
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
                return this.players.Average(player => ( 1 * player.ResearchDisplay + 2 * player.ResearchGuess
                        + 5 * player.Research + 13 * player.LastResearched ) / 21.0);
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
            if (this.currentPlayer >= i)
                --this.currentPlayer;

            this.players.Remove(player);
        }

        #endregion //internal

        #region public

        public double MapSize
        {
            get
            {
                //set this constant to Pi*m^2 where m is the target standard deviation confidence interval
                return 10.4 * this.MapDeviation * this.MapDeviation;
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
            this.AutoSave();
            CurrentPlayer.PlayTurn(handler, Enumerable.Empty<Tile>());
        }

        internal void SetSpaceObject(int x, int y, SpaceObject spaceObject)
        {
            if (spaceObject == null)
            {
                if (!RemoveSpaceObject(x, y))
                    throw new Exception();
            }
            else
            {
                AddSpaceObject(x, y, spaceObject);
            }
        }

        public Tile GetTile(Point point)
        {
            return GetTile(point.X, point.Y);
        }
        public Tile GetTile(int x, int y)
        {
            SpaceObject spaceObject;
            if (TryGetSpaceObject(x, y, out spaceObject))
                return spaceObject.Tile;
            return new Tile(this, x, y);
        }

        public SpaceObject GetSpaceObject(Point point)
        {
            return GetSpaceObject(point.X, point.Y);
        }
        public SpaceObject GetSpaceObject(int x, int y)
        {
            SpaceObject spaceObject;
            TryGetSpaceObject(x, y, out spaceObject);
            return spaceObject;
        }

        public Dictionary<Player, double> GetResearch()
        {
            Player[] players = GetResearchDisplayOrder();
            return players.ToDictionary(player => player, player => player.ResearchDisplay / players[0].ResearchDisplay * 100);
        }
        internal Player[] GetResearchDisplayOrder()
        {
            return GetOrder(player => player.ResearchDisplay);
        }
        internal Player[] GetRealResearchOrder()
        {
            return GetOrder(player => player.Research);
        }
        private Player[] GetOrder(Func<Player, double> Func)
        {
            Player[] players = this.players.ToArray();
            Array.Sort<Player>(players, (p1, p2) => ( Math.Sign(Func(p2) - Func(p1)) ));
            return players;
        }

        public ReadOnlyCollection<Player> GetPlayers()
        {
            return this.players.AsReadOnly();
        }

        public HashSet<Planet> GetPlanets()
        {
            return new HashSet<Planet>(GetSpaceObjects().OfType<Planet>());
        }

        public IEnumerable<Tile> EndTurn(IEventHandler handler)
        {
            return EndTurn(handler, false);
        }
        internal IEnumerable<Tile> EndTurn(IEventHandler handler, bool allowAI)
        {
            handler = new HandlerWrapper(handler, this);

            AssertException.Assert(allowAI || CurrentPlayer.AI == null);

            CurrentPlayer.EndTurn(handler);
            Graphs.EndTurn(CurrentPlayer);

            CheckResearchVictory();
            var anomalies = CreateAnomalies().Select(anomaly => anomaly.Tile);
            AdjustCenter(1 / (double)this.players.Count);
            var removed = RemoveTeleporters();
            if (removed != null)
                anomalies = anomalies.Concat(new[] { removed.Item1, removed.Item2 });

            if (++this.currentPlayer >= this.players.Count)
                NewRound();

            StartPlayerTurn(handler);

            if (CurrentPlayer.AI == null)
                AutoSave();

            anomalies = CurrentPlayer.PlayTurn(handler, anomalies);
            return anomalies;
        }

        public void AutoSave()
        {
            if (AutoSavePath != null)
            {
                TBSUtil.SaveGame(this, AutoSavePath, turn + ".gws");

                Graphs temp = this.Graphs;
                this.Graphs = null;
                TBSUtil.SaveGame(this, AutoSavePath + "/../replay/" + this.ID, turn + "_" + ( currentPlayer + 1 ) + ".gws");
                this.Graphs = temp;
            }
        }

        private void NewRound()
        {
            //just so an exception is thrown if current player is mistakenly used
            this.currentPlayer = byte.MaxValue;

            RandMoveOrder();

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
                if (chance > 0 && PlayerTurnChance(chance))
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
            return Consts.GetResearchVictoryChance(first / second);
        }

        private bool PlayerTurnChance(double roundChance)
        {
            return Random.Bool(1 - Math.Pow(1 - roundChance, 1 / (double)this.players.Count));
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
                    double gold = moveOrderGold * pair.Value;
                    player.AddGold(gold);
                    player.IncomeTotal += gold;
                }
            }
        }

        private Tuple<Tile, Tile> RemoveTeleporters()
        {
            List<Tuple<Tile, Tile>> teleporters = GetTeleporters();
            if (teleporters.Count > 0)
            {
                double chance = teleporters.Count / ( 65.0 + teleporters.Count );
                if (PlayerTurnChance(chance))
                {
                    var remove = Random.SelectValue(teleporters);
                    RemoveTeleporter(remove);
                    return remove;
                }
            }
            return null;
        }

        internal bool CreateTeleporter(IEventHandler handler, Tile tile, Tile target)
        {
            double chance = GetTeleporters().Count + 1;
            //check if the tiles are too close to be useful or if either tile already has a teleporter
            if (CanCreateTeleporter(tile, target) && Random.Bool(1.0 / chance))
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
        private static bool CanCreateTeleporter(Tile tile, Tile target)
        {
            return ( Tile.GetDistance(tile, target) > 1 && tile.Teleporter == null && target.Teleporter == null );
        }
        private bool CheckAttInvPlayers(SpaceObject obj, bool inv, Tile t1, Tile t2)
        {
            HashSet<SpaceObject> before = Anomaly.GetAttInv(obj.Tile, inv);
            Tuple<Tile, Tile> teleporter = CreateTeleporter(t1, t2);
            HashSet<SpaceObject> after = Anomaly.GetAttInv(obj.Tile, inv);
            RemoveTeleporter(teleporter);
            foreach (SpaceObject other in after)
                if (other.Player != obj.Player && !before.Contains(other))
                    return false;
            return true;
        }
        private Tuple<Tile, Tile> CreateTeleporter(Tile t1, Tile t2)
        {
            var teleporter = new Tuple<Tile, Tile>(t1, t2);
            AddTeleporter(teleporter);
            return teleporter;
        }

        internal Planet CreateAnomalyPlanet(IEventHandler handler, Tile tile)
        {
            if (Random.Bool(GetPlanetChance(tile, true)))
            {
                handler.Explore(Anomaly.AnomalyType.NewPlanet);
                return CreatePlanet(tile);
            }
            return null;
        }
        public double GetAnomalyPlanetChance(Tile tile)
        {
            return GetPlanetChance(tile, true);
        }
        public double GetPlanetChance(Tile tile, bool anomaly)
        {
            int dist = int.MaxValue;
            foreach (Planet planet in GetPlanets())
                dist = Math.Min(dist, Tile.GetDistance(tile, planet.Tile));
            return GetPlanetChance(dist, anomaly);
        }
        private double GetPlanetChance(int dist, bool anomaly)
        {
            double value = 0;
            if (dist > Consts.PlanetDistance)
            {
                value = .91 + dist - Consts.PlanetDistance - 1;
                value = Math.Sqrt(value / ( 9.1 + Math.Pow(MapSize, .21) + value ));
                if (anomaly)
                    value *= this.PlanetPct / this.AnomalyPct;
            }
            return value;
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
            int amt = Random.OEInt(avg * Math.Sqrt(MapSize) / 130.0);

            if (amt > 0)
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
                if (shipWeight != 0)
                    shipWeight = AnomalyWeight / 1.3 / shipWeight;
                //population is weighted at its total's square root
                if (popWeight > 0)
                    popWeight = 1 / Math.Sqrt(popWeight);

                for (int a = 0 ; a < amt ; ++a)
                    AdjustCenter(shipWeight, popWeight, AnomalyWeight);
            }
        }
        private void AdjustCenter(double shipWeight, double popWeight, double anomalyWeight)
        {
            double max = double.MinValue;
            var directions = new Dictionary<Tile, double>();

            Tile centerTile = this.Center;
            foreach (SpaceObject spaceObject in this.GetSpaceObjects())
            {
                int distance = Tile.GetDistance(centerTile, spaceObject.Tile);
                if (distance > 0)
                {
                    double weight;
                    Ship ship = ( spaceObject as Ship );
                    Planet planet = ( spaceObject as Planet );
                    if (ship != null)
                        weight = shipWeight * GetShipWeight(ship) + popWeight * ship.Population;
                    else if (planet != null)
                        weight = planet.PlanetValue + ( planet.Colony == null ? 0 : popWeight * planet.Colony.Population );
                    else if (spaceObject is Anomaly)
                        weight = anomalyWeight;
                    else
                        throw new Exception();

                    HashSet<Tile> neighbors = Tile.GetNeighbors(centerTile);
                    neighbors.RemoveWhere(neighbor => ( Tile.GetDistance(neighbor, spaceObject.Tile) >= distance ));

                    //sqrt because final values will be squared
                    weight *= distance / Math.Sqrt(neighbors.Count);

                    foreach (Tile neighbor in neighbors)
                    {
                        double value;
                        directions.TryGetValue(neighbor, out value);
                        value += weight;
                        directions[neighbor] = value;
                        max = Math.Max(max, value);
                    }
                }
            }

            //mult just ensures we don't overload 32 bit ints
            double mult = 99 / max / max;

            Dictionary<Tile, int> chances = new Dictionary<Tile, int>();
            foreach (var pair in directions)
                //squared - if changed, must change count divide above
                chances.Add(pair.Key, Random.Round(pair.Value * pair.Value * mult));
            this.Center = Random.SelectValue(chances);
        }
        private static double GetShipWeight(Ship ship)
        {
            return ( 2.6 * ship.GetStrength() + ship.GetValue() ) * Math.Sqrt(ship.HP / (double)ship.MaxHP);
        }

        public Rectangle GetGameBounds(params Tile[] include)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (Tile tile in include)
                FindBounds(ref minX, ref minY, ref maxX, ref maxY, tile);
            foreach (SpaceObject spaceObject in GetSpaceObjects())
                FindBounds(ref minX, ref minY, ref maxX, ref maxY, spaceObject.Tile);
            FindTeleporterBounds(ref minX, ref minY, ref maxX, ref maxY);
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }
        private void FindTeleporterBounds(ref int minX, ref int minY, ref int maxX, ref int maxY)
        {
            foreach (Tuple<Tile, Tile> teleporter in GetTeleporters())
            {
                FindBounds(ref minX, ref minY, ref maxX, ref maxY, teleporter.Item1);
                FindBounds(ref minX, ref minY, ref maxX, ref maxY, teleporter.Item2);
            }
        }
        private static void FindBounds(ref int minX, ref int minY, ref int maxX, ref int maxY, Tile tile)
        {
            minX = Math.Min(minX, tile.X);
            minY = Math.Min(minY, tile.Y);
            maxX = Math.Max(maxX, tile.X);
            maxY = Math.Max(maxY, tile.Y);
        }

        internal Tile GetRandomTile()
        {
            return GetRandomTile(this.Center, this.MapDeviation);
        }
        internal Tile GetRandomTile(Tile center, double stdDev)
        {
            return GetDistanceTiles(center, Math.Abs(Random.GaussianInt(stdDev))).First();
        }
        public IEnumerable<Tile> GetDistanceTiles(Tile center, int dist)
        {
            int minX = center.X, minY = center.Y, maxX = center.X, maxY = center.Y;
            FindTeleporterBounds(ref minX, ref minY, ref maxX, ref maxY);
            minX -= dist;
            minY -= dist;
            maxX += dist;
            maxY += dist;

            //Expected iterations per returned value is i=n/(3*(SQRT(n)-1)) where n=(maxX-minX+1)*(maxY-minY+1), or O(SQRT(n)).
            //Because n is O(dist^2), the operation time to return a single value from this loop, i, is O(dist).
            //Teleporters may increase the value of n with respect to dist, but reduce i with respect to values of n.
            foreach (Point p in Random.Iterate(minX, maxX, minY, maxY))
            {
                Tile test = GetTile(p);
                if (Tile.GetDistance(center, test) == dist)
                    yield return test;
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
            checked
            {
                //objects will be re-added as each tile is set
                var temp = this._spaceObjects.ToList();
                this._spaceObjects.Clear();

                //Tiles are not serialized so we go through and create a new one for each SpaceObject
                foreach (var pair in temp)
                    pair.Value.OnDeserialization(new Tile(this, pair.Key.X, pair.Key.Y));
            }
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

            for (int i = this.deadPlayers.Count ; --i > -1 ;)
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
                : base(delegate (Tuple<T1, T2> args)
                    {
                        return UndoMethod(args.Item1, args.Item2);
                    }, new Tuple<T1, T2>(arg1, arg2))
            {
            }
        }
        internal class UndoCommand<T1, T2, T3> : UndoCommand<Tuple<T1, T2, T3>>
        {
            public UndoCommand(UndoMethod<T1, T2, T3> UndoMethod, T1 arg1, T2 arg2, T3 arg3)
                : base(delegate (Tuple<T1, T2, T3> args)
                    {
                        return UndoMethod(args.Item1, args.Item2, args.Item3);
                    }, new Tuple<T1, T2, T3>(arg1, arg2, arg3))
            {
            }
        }
        internal class UndoCommand<T1, T2, T3, T4> : UndoCommand<Tuple<T1, T2, T3, T4>>
        {
            public UndoCommand(UndoMethod<T1, T2, T3, T4> UndoMethod, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
                : base(delegate (Tuple<T1, T2, T3, T4> args)
                    {
                        return UndoMethod(args.Item1, args.Item2, args.Item3, args.Item4);
                    }, new Tuple<T1, T2, T3, T4>(arg1, arg2, arg3, arg4))
            {
            }
        }
        internal class UndoCommand<T1, T2, T3, T4, T5, T6> : UndoCommand<Tuple<T1, T2, T3, T4, T5, T6>>
        {
            public UndoCommand(UndoMethod<T1, T2, T3, T4, T5, T6> UndoMethod, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
                : base(delegate (Tuple<T1, T2, T3, T4, T5, T6> args)
                {
                    return UndoMethod(args.Item1, args.Item2, args.Item3, args.Item4, args.Item5, args.Item6);
                }, new Tuple<T1, T2, T3, T4, T5, T6>(arg1, arg2, arg3, arg4, arg5, arg6))
            {
            }
        }
        internal class UndoCommand<T1, T2, T3, T4, T5, T6, T7, T8> : UndoCommand<Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>>>
        {
            public UndoCommand(UndoMethod<T1, T2, T3, T4, T5, T6, T7, T8> UndoMethod, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
                : base(delegate (Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>> args)
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
                        this._points = (sbyte)value;
                    }
                }
            }

            internal static void Finalize(List<Result> results)
            {
                int points = 0;
                int add = -1;
                int min = int.MaxValue;
                //adds in (x^2+x)/2 points, where x is the inverse index
                for (int i = results.Count ; --i > -1 ;)
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
