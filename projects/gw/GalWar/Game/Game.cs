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

            if (number > 3999 || number < 1)
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

        internal readonly ShipNames ShipNames;

        private readonly List<Player> players;
        private readonly List<Result> deadPlayers, winningPlayers;

        private readonly Dictionary<PointS, SpaceObject> _spaceObjects;
        private readonly List<Wormhole> _wormholes;

        [NonSerialized]
        private Stack<IUndoCommand> _undoStack;

        private PointS _center;

        private readonly uint _id;

        private byte _currentPlayer;
        private ushort _turn;
        private float _avgResearch;
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

                this.ShipNames = new ShipNames(numPlayers);

                this._spaceObjects = new Dictionary<PointS, SpaceObject>();
                this.players = new List<Player>(numPlayers);
                this._wormholes = new List<Wormhole>();
                this.deadPlayers = new List<Result>(numPlayers - 1);
                this.winningPlayers = new List<Result>(numPlayers - 1);

                this._id = Random.NextUInt();

                this._currentPlayer = byte.MaxValue / 2;
                this._turn = 0;

                planetPct *= Math.Sqrt(MapSize);
                this._planetPct = (float)Random.GaussianCapped(planetPct / 210.0, .078, planetPct / 260.0);
                double min = this.PlanetPct + 0.13;
                this._anomalyPct = (float)Random.GaussianCapped(min + MapSize * .000169 + ( numPlayers + 6.5 ) * .013, .169, min);

                this._avgResearch = (float)GetStartDouble(Consts.StartResearch * .78);

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

        public ReadOnlyCollection<Wormhole> GetWormholes()
        {
            return _wormholes.AsReadOnly();
        }
        private void AddWormhole(Wormhole wormhole)
        {
            this._wormholes.Add(wormhole);
        }
        private bool RemoveWormhole(Wormhole wormhole)
        {
            return this._wormholes.Remove(wormhole);
        }
        private void SetWormholes(IEnumerable<Wormhole> wormholes)
        {
            this._wormholes.Clear();
            this._wormholes.AddRange(wormholes);
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

        public string ID
        {
            get
            {
                return string.Format("{0:X8}", this._id);
            }
        }

        public int currentPlayer
        {
            get
            {
                return this._currentPlayer;
            }
            private set
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
            //temporary starting wormholes to space out initial map generation somewhat
            double avgWormholes = Math.Pow(MapSize, .26) / Math.PI;
            int wormholes = Random.GaussianOEInt(avgWormholes, .065, .065, ( ( avgWormholes > 1 ) ? 1 : 0 ));
            for (int a = 0 ; a < wormholes ; ++a)
                CreateWormhole(GetRandomTile(), GetRandomTile());

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
            if (tile.SpaceObject == null && Random.Bool(GetPlanetChance(tile)))
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
            //remove temporary starting wormholes to allow actual distance calculations
            SetWormholes(Enumerable.Empty<Wormhole>());

            //starting gold is based on the number and value of initial planets, must happen after GetHomeworld planet shuffling
            double startGold = Consts.StartGold;
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
            double moveOrderGold = Consts.GetMoveOrderGold(this.players);

            double startSoldiers = GetStartDouble(startPop / 1.69);
            double startDefense = GetStartDouble(( startSoldiers + startProd ) / 2.6);
            int startDefenseSteps = (int)Math.Floor(( startDefense / Colony.GetMultedProd(startPop * Consts.Income) - 1 ) / Consts.FLOAT_ERROR_ONE);

            for (this.currentPlayer = 0 ; this.currentPlayer < numPlayers ; ++this.currentPlayer)
            {
                //will always have exactly one colony at this point, the homeworld
                Colony homeworld = CurrentPlayer.GetColonies()[0];

                //starting soldiers and defense
                for (int a = 0 ; a < startDefenseSteps ; ++a)
                {
                    homeworld.BuildSoldiers(startSoldiers / startDefenseSteps, false);
                    homeworld.BuildPlanetDefense(startDefense / startDefenseSteps, true);
                }

                //starting gold is divided by each indivual player's homeworld quality
                double addGold = startGold / (double)homeworld.Planet.Quality;

                //some starting gold automatically turns into production
                double addProduction = addGold * Consts.StartGoldProdPct;
                addGold -= addProduction;

                //starting production
                addProduction += startProd;

                //turn order balancing
                addGold += currentPlayer * moveOrderGold;

                //calculations to offset AddProduction when currently building StoreProd
                addProduction /= Consts.StoreProdRatio;
                addGold -= addProduction * ( 1 - Consts.StoreProdRatio ) / Consts.ProductionForGold;

                //actually add in starting gold and production
                CurrentPlayer.AddGold(addGold);
                homeworld.AddProduction(addProduction);

                //calculate current income total
                CurrentPlayer.IncomeTotal += CurrentPlayer.TotalGold + homeworld.production2;

                homeworld.ProdGuess = homeworld.production2;
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
                        anomPlanetRate += chance * pair.Value * GetPlanetChance(pair.Key);
                        chance *= ( 1 - pair.Value );
                    }
                    anomPlanetRate += chance * GetPlanetChance(planetDist);

                    //store final chance to become a planet
                    anomalies.Add(anomaly, Consts.LimitPct(anomPlanetRate * anomaly.planetChance));
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

        public double AvgResearch
        {
            get
            {
                return this._avgResearch;
            }
            private set
            {
                checked
                {
                    this._avgResearch = (float)value;
                }
            }
        }
        public double PDResearch
        {
            get
            {
                double minResearch;
                if (!this.players.Any())
                {
                    minResearch = 0;
                }
                else
                {
                    minResearch = this.players.Min(player => player.ResearchDisplay);
                    double diff = Math.Abs(AvgResearch - minResearch);
                    minResearch = Math.Max(0, Math.Min(AvgResearch, minResearch) - diff);
                }
                return minResearch;
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
            var removed = RemoveWormhole();
            if (removed != null)
                anomalies = anomalies.Concat(removed);
            AdjustAvgResearch();

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
            Dictionary<SpaceObject, HashSet<SpaceObject>> attInv = Anomaly.GetAttInv(this);
            Dictionary<Player, int> playerGold = TBSUtil.RandMoveOrder<Player>(Random, this.players, Consts.MoveOrderShuffle, (index, swap) =>
            {
                Player attacker = this.players[index];
                Player defender = this.players[swap];
                return !attInv.Where(pair => pair.Key.Player == defender).SelectMany(pair => pair.Value).Where(spaceObject => spaceObject.Player == attacker).Any();
            });
            if (playerGold.Count > 0)
            {
                double moveOrderGold = Consts.GetMoveOrderGold(this.players);
                foreach (KeyValuePair<Player, int> pair in playerGold)
                {
                    Player player = pair.Key;
                    double gold = moveOrderGold * pair.Value;
                    player.AddGold(gold);
                    player.IncomeTotal += gold;
                }
            }
        }

        private IEnumerable<Tile> RemoveWormhole()
        {
            IEnumerable<Wormhole> wormholes = GetWormholes();
            int count = wormholes.Sum(w => w.Tiles.Count());
            if (count > 0)
            {
                double chance = count / ( 130.0 + count );
                if (PlayerTurnChance(chance))
                    return RemoveWormholeExit(Random.SelectValue(wormholes));
            }
            return null;
        }
        private List<Tile> RemoveWormholeExit(Wormhole wormhole)
        {
            List<Tile> removed = wormhole.Tiles.ToList();
            if (wormhole.Remove())
                RemoveWormhole(wormhole);
            else
                ;
            return removed;
        }

        internal bool CreateWormhole(IEventHandler handler, Tile tile, Tile target, Ship anomShip)
        {
            double chance = GetWormholes().Sum(w => w.Tiles.Count());
            chance = Math.Pow(2.1 / ( 2.6 + chance ), 1.69);
            if (tile.Wormhole != null || target.Wormhole != null)
                chance = 1;
            if (tile != target && ( tile.Wormhole == null || tile.Wormhole != target.Wormhole ) && Random.Bool(chance))
            {
                //check if any tiles are next to each other
                var all = new Tile[] { tile, target }.SelectMany(t =>
                {
                    IEnumerable<Tile> ret = new Tile[] { t };
                    if (t.Wormhole != null)
                        ret = ret.Concat(t.Wormhole.Tiles);
                    return ret;
                });
                foreach (Tile t1 in all)
                    foreach (Tile t2 in all)
                        if (t1 != t2 && Tile.IsRawNeighbor(t1, t2.X, t2.Y))
                            return false;

                List<Wormhole> old = GetWormholes().ToList();
                Dictionary<SpaceObject, HashSet<SpaceObject>> before = Anomaly.GetAttInv(this);

                CreateWormhole(tile, target);

                bool valid = true;
                //check this will not make any planets be too close
                HashSet<Planet> planets = GetPlanets();
                foreach (Planet p1 in planets)
                    foreach (Planet p2 in planets)
                        if (p1 != p2 && Tile.GetDistance(p1.Tile, p2.Tile) <= Consts.PlanetDistance)
                        {
                            valid = false;
                            break;
                        }
                //check and make sure enemies cannot be attacked/invaded
                if (valid)
                {
                    valid = Anomaly.ValidateChange(before, anomShip);
                    if (valid)
                        ;
                    else
                        ;
                }
                else
                    ;

                if (valid)
                    return true;
                else
                    SetWormholes(old);
            }
            else if (!( tile != target ))
                ;
            else if (!( tile.Wormhole == null || tile.Wormhole != target.Wormhole ))
            {
                if (tile.Wormhole == null)
                    ;
                else
                    ;
                if (target.Wormhole == null)
                    ;
                else
                    ;
            }
            return false;
        }
        private void CreateWormhole(Tile t1, Tile t2)
        {
            IEnumerable<Wormhole> merge = GetWormholes().Where(w => w.Tiles.Contains(t1) || w.Tiles.Contains(t2)).ToList();

            IEnumerable<Tile> tiles = new Tile[] { t1, t2 };
            tiles = tiles.Union(merge.SelectMany(w => w.Tiles)).Distinct().ToList();

            foreach (Wormhole r in merge)
                RemoveWormhole(r);

            int turn = this.Turn;
            if (merge.Any())
                turn = merge.Min(w => w.CreatedTurn);
            AddWormhole(new Wormhole(tiles, turn));
        }

        internal Planet CreateAnomalyPlanet(IEventHandler handler, Anomaly anomaly)
        {
            if (Random.Bool(GetPlanetChance(anomaly)))
            {
                handler.Explore(Anomaly.AnomalyType.NewPlanet);
                return CreatePlanet(anomaly.Tile);
            }
            return null;
        }
        private double GetPlanetChance(Tile tile)
        {
            return GetPlanetChance(tile, null);
        }
        public double GetPlanetChance(Anomaly anomaly)
        {
            return GetPlanetChance(anomaly.Tile, anomaly.planetChance);
        }
        private double GetPlanetChance(int dist)
        {
            return GetPlanetChance(dist, 1);
        }
        private double GetPlanetChance(Tile tile, double? anomChance)
        {
            int dist = int.MaxValue;
            foreach (Planet planet in GetPlanets())
                dist = Math.Min(dist, Tile.GetDistance(tile, planet.Tile));
            return GetPlanetChance(dist, anomChance);
        }
        private double GetPlanetChance(int dist, double? anomChance)
        {
            double value = 0;
            if (dist > Consts.PlanetDistance)
            {
                value = .91 + dist - Consts.PlanetDistance - 1;
                value = Math.Sqrt(value / ( 9.1 + Math.Pow(MapSize, .21) + value ));
                if (anomChance.HasValue)
                    value *= anomChance.Value * this.PlanetPct / this.AnomalyPct;
            }
            return Consts.LimitPct(value);
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
                    Colony colony = ( planet == null ? null : planet.Colony );
                    if (ship != null)
                    {
                        shipWeight += GetShipWeight(ship);
                        popWeight += GetPopWeight(ship);
                    }
                    if (colony != null)
                    {
                        shipWeight += GetProdWeight(colony);
                        popWeight += GetPopWeight(colony);
                    }
                }

                //anomalies weigh less than even the smallest possible planet
                const double AnomalyWeight = Consts.PlanetConstValue / 1.3;
                //the total weight of all ships is always less than even a single anomaly
                if (shipWeight > 0)
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
                    double weight = 0;
                    Ship ship = ( spaceObject as Ship );
                    Planet planet = ( spaceObject as Planet );
                    Colony colony = ( planet == null ? null : planet.Colony );
                    if (ship != null)
                        weight += shipWeight * GetShipWeight(ship) + popWeight * GetPopWeight(ship);
                    if (planet != null)
                        weight += planet.PlanetValue;
                    if (colony != null)
                        weight += shipWeight * GetProdWeight(colony) + popWeight * GetPopWeight(colony);
                    if (spaceObject is Anomaly)
                        weight += anomalyWeight;

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
            double mult = byte.MaxValue / max / max;

            Dictionary<Tile, int> chances = new Dictionary<Tile, int>();
            foreach (var pair in directions)
                //squared - if changed, must change count divide above
                chances.Add(pair.Key, Random.Round(pair.Value * pair.Value * mult));
            this.Center = Random.SelectValue(chances);
        }
        private static double GetShipWeight(Ship ship)
        {
            return GetCenterWeight(ship.GetStrength(), ship.GetValue(), ship.HP / (double)ship.MaxHP);
        }
        private static double GetProdWeight(Colony colony)
        {
            int att = colony.Player.PDAtt, def = colony.Player.PDDef;
            double prodMult = ShipDesign.GetPlanetDefenseStrength(att, def) / ShipDesign.GetPlanetDefenseCost(att, def, colony.Player.Game.AvgResearch);
            double str = colony.PDStrength + colony.production2 * prodMult;
            return GetCenterWeight(str, str, 1);
        }
        private double GetPopWeight(PopCarrier carrier)
        {
            return carrier.Population * Math.Sqrt(1.0 + carrier.GetSoldierPct());
        }
        private static double GetCenterWeight(double str, double value, double pct)
        {
            return ( 2.6 * str + value ) * Math.Sqrt(pct);
        }

        public Rectangle GetGameBounds(params Tile[] include)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (Tile tile in include)
                FindBounds(ref minX, ref minY, ref maxX, ref maxY, tile);
            foreach (SpaceObject spaceObject in GetSpaceObjects())
                FindBounds(ref minX, ref minY, ref maxX, ref maxY, spaceObject.Tile);
            FindWormholeBounds(ref minX, ref minY, ref maxX, ref maxY);
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }
        private void FindWormholeBounds(ref int minX, ref int minY, ref int maxX, ref int maxY)
        {
            foreach (Tile wormhole in GetWormholes().SelectMany(w => w.Tiles))
                FindBounds(ref minX, ref minY, ref maxX, ref maxY, wormhole);
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
            FindWormholeBounds(ref minX, ref minY, ref maxX, ref maxY);
            minX -= dist;
            minY -= dist;
            maxX += dist;
            maxY += dist;

            //Expected iterations per returned value is i=n/(3*(SQRT(n)-1)) where n=(maxX-minX+1)*(maxY-minY+1), or O(SQRT(n)).
            //Because n is O(dist^2), the operation time to return a single value from this loop, i, is O(dist).
            //Wormholes may increase the value of n with respect to dist, but reduce i with respect to values of n.
            foreach (Point p in Random.Iterate(minX, maxX, minY, maxY))
            {
                Tile test = GetTile(p);
                if (Tile.GetDistance(center, test) == dist)
                    yield return test;
            }
        }

        private void AdjustAvgResearch()
        {
            double avgResearch = this.players.Average(player => ( 2 * player.ResearchDisplay + 6 * player.Research + 13 * player.GetLastResearched() ) / 21.0);
            //adjust AvgResearch by the average player income at maximum emphasis every full turn round
            double add = players.Average(player => player.GetTotalIncome()) * Consts.EmphasisValue / ( Consts.EmphasisValue + 2.0 ) / players.Count();
            //Console.WriteLine("this.AvgResearch " + this.AvgResearch.ToString(".000").PadLeft(10));
            //Console.WriteLine("avgResearch      " + avgResearch.ToString(".000").PadLeft(10));
            //Console.WriteLine("add              " + ( Math.Sign(avgResearch - this.AvgResearch) * add ).ToString(".000").PadLeft(10));
            this.AvgResearch += Math.Sign(avgResearch - this.AvgResearch) * Random.Gaussian(add, Consts.ResearchRndm);
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

            //game._wormholes = new List<GalWar.Wormhole>();
            //int v = Random.OEInt(13);
            //for (int a = 0 ; a < v ; ++a)
            //{
            //    Tile t;
            //    if (game.GetWormholes().Any() && Random.Bool())
            //        t = Random.SelectValue(game.GetWormholes().SelectMany(w => w.Tiles));
            //    else
            //        t = game.GetRandomTile();
            //    game.CreateWormhole(null, game.GetRandomTile(), t, Random.SelectValue(game.GetSpaceObjects().OfType<Ship>()));
            //    if (Random.Bool())
            //        game.RemoveWormhole();
            //}

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

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
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
        internal delegate Tile UndoMethod<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
        internal delegate Tile UndoMethod<T1, T2, T3, T4, T5, T6, T7, T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
        internal delegate Tile UndoMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);

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
        internal class UndoCommand<T1, T2, T3, T4, T5, T6, T7> : UndoCommand<Tuple<T1, T2, T3, T4, T5, T6, T7>>
        {
            public UndoCommand(UndoMethod<T1, T2, T3, T4, T5, T6, T7> UndoMethod, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
                : base(delegate (Tuple<T1, T2, T3, T4, T5, T6, T7> args)
                {
                    return UndoMethod(args.Item1, args.Item2, args.Item3, args.Item4, args.Item5, args.Item6, args.Item7);
                }, new Tuple<T1, T2, T3, T4, T5, T6, T7>(arg1, arg2, arg3, arg4, arg5, arg6, arg7))
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
        internal class UndoCommand<T1, T2, T3, T4, T5, T6, T7, T8, T9> : UndoCommand<Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9>>>
        {
            public UndoCommand(UndoMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> UndoMethod, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
                : base(delegate (Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9>> args)
                    {
                        return UndoMethod(args.Item1, args.Item2, args.Item3, args.Item4, args.Item5, args.Item6, args.Item7, args.Rest.Item1, args.Rest.Item2);
                    }, new Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9>>(arg1, arg2, arg3, arg4, arg5, arg6, arg7, new Tuple<T8, T9>(arg8, arg9)))
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
