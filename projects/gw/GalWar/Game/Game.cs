using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using MattUtil;

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

        private readonly Tile[,] map;
        private readonly List<Player> players;
        private readonly List<Planet> planets;
        private readonly List<Tuple<Tile, Tile>> teleporters;
        private readonly List<Result> deadPlayers, winningPlayers;

        [NonSerialized]
        private Stack<IUndoCommand> _undoStack;


        private readonly byte _diameter;

        private byte _currentPlayer;
        private ushort _turn;
        private float _planetPct, _anomalyPct;

        public Game(Player.StartingPlayer[] players, int radius, double planetPct)
        {
            checked
            {
                int numPlayers = players.Length;

                this._diameter = (byte)( radius * 2 - 1 );

                AssertException.Assert(players != null);
                AssertException.Assert(numPlayers > 1);
                AssertException.Assert(numPlayers * 78 < MapSize);
                AssertException.Assert(radius < 78);
                AssertException.Assert(planetPct > 0.00013);
                AssertException.Assert(planetPct < 0.039);

                this.StoreProd = new StoreProd();
                this.Attack = new Attack();
                this.Defense = new Defense();

                this.ShipNames = new ShipNames(numPlayers);

                this.map = new Tile[Diameter, Diameter];
                this.players = new List<Player>();
                this.planets = new List<Planet>();
                this.teleporters = new List<Tuple<Tile, Tile>>();
                this.deadPlayers = new List<Result>(numPlayers - 1);
                this.winningPlayers = new List<Result>(numPlayers - 1);

                this._currentPlayer = byte.MaxValue;
                this._turn = 0;
                this._planetPct = float.NaN;
                this._anomalyPct = float.NaN;

                InitMap(radius);
                double numPlanets = CreateSpaceObjects(numPlayers, planetPct);
                InitPlayers(players, numPlanets);

                this.Graphs = new Graphs(this);
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

        public int Diameter
        {
            get
            {
                return this._diameter;
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
        private double planetPct
        {
            get
            {
                return this._planetPct;
            }
            set
            {
                checked
                {
                    this._planetPct = (float)value;
                }
            }
        }
        private double anomalyPct
        {
            get
            {
                return this._anomalyPct;
            }
            set
            {
                checked
                {
                    this._anomalyPct = (float)value;
                }
            }
        }

        private void InitMap(int radius)
        {
            //set up map hexagon
            int nullTiles = Diameter - radius;
            for (int y = 0 ; y < Diameter ; ++y)
            {
                for (int x = 0 ; x < Diameter ; ++x)
                {
                    int compX = x - ( radius % 2 == 0 && y % 2 == 0 ? 1 : 0 );
                    if (compX >= nullTiles / 2 && compX < Diameter - ( nullTiles + 1 ) / 2)
                        this.map[x, y] = new Tile(this, x, y);
                }
                if (y < Diameter / 2)
                    --nullTiles;
                else
                    ++nullTiles;
            }
        }

        private double CreateSpaceObjects(int numPlayers, double planetPct)
        {
            planetPct *= MapSize;

            this.planetPct = Random.GaussianCapped(planetPct / 91.0, .091, Consts.FLOAT_ERROR);
            this.anomalyPct = Random.GaussianCapped(this.planetPct + MapSize * .00013 + ( numPlayers + 6.5 ) * .013,
                    .21, this.planetPct + Consts.FLOAT_ERROR);

            //first create enough planets for homeworlds
            while (this.planets.Count < numPlayers)
                NewPlanet();

            double anomPlanets = 0;
            double anomPlanetRate = ( this.planetPct / this.anomalyPct ) / 1.3;
            int startAnomalies = Random.GaussianOEInt(this.anomalyPct * Consts.StartAnomalies, .39, .13,
                    ( this.anomalyPct * Consts.StartAnomalies > 1 ) ? 1 : 0);
            for (int a = 0 ; a < startAnomalies ; ++a)
            {
                Anomaly anomaly = CreateAnomaly();
                if (anomaly != null && CheckPlanetDistance(anomaly.Tile) && ( anomPlanets += anomPlanetRate ) > planetPct)
                {
                    planetPct = anomPlanets;
                    break;
                }
            }

            planetPct -= anomPlanets;
            int startPlanets = Random.GaussianOEInt(planetPct, .13, .091, ( planetPct > 1 ) ? 1 : 0);
            for (int a = 0 ; a < startPlanets ; ++a)
                NewPlanet();

            return this.planets.Count + anomPlanets;
        }
        private Planet NewPlanet()
        {
            Tile tile;
            do
            {
                tile = GetRandomTile();
                //planets cannot be right on the map edge so tile must have 6 neighbors
            } while (Tile.GetNeighbors(tile).Count < 6);

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
                Planet removePlanet;
                do
                {
                    removePlanet = this.planets[Random.Next(this.planets.Count)];
                } while (removePlanet.Colony != null);
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
            List<Planet> retVal = new List<Planet>(this.planets.Count);
            foreach (Planet planet in this.planets)
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
            this.planets.Remove(planet);
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

        public int MapSize
        {
            get
            {
                int radius = ( Diameter + 1 ) / 2;
                return 3 * radius * ( radius - 1 ) + 1;
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
            CurrentPlayer.PlayTurn(handler);
        }

        public Tile[,] GetMap()
        {
            return (Tile[,])this.map.Clone();
        }

        public ReadOnlyCollection<Tuple<Tile, Tile>> GetTeleporters()
        {
            return this.teleporters.AsReadOnly();
        }

        public Dictionary<Player, double> GetResearch()
        {
            Player[] players = GetResearchOrder();
            Dictionary<Player, double> retVal = new Dictionary<Player, double>(players.Length);
            for (int a = 0 ; a < players.Length ; ++a)
                retVal.Add(players[a], players[a].ResearchDisplay / players[0].ResearchDisplay * 100);
            return retVal;
        }

        internal Player[] GetResearchOrder()
        {
            Player[] players = this.players.ToArray();
            Array.Sort<Player>(players, delegate(Player p1, Player p2)
            {
                //descending sort
                return Math.Sign(p2.ResearchDisplay - p1.ResearchDisplay);
            });
            return players;
        }

        public ReadOnlyCollection<Player> GetPlayers()
        {
            return this.players.AsReadOnly();
        }

        public ReadOnlyCollection<Planet> GetPlanets()
        {
            return this.planets.AsReadOnly();
        }

        public void EndTurn(IEventHandler handler)
        {
            EndTurn(handler, false);
        }
        internal void EndTurn(IEventHandler handler, bool allowAI)
        {
            handler = new HandlerWrapper(handler, this);

            AssertException.Assert(allowAI || CurrentPlayer.AI == null);

            CurrentPlayer.EndTurn(handler);
            Graphs.EndTurn(CurrentPlayer);

            if (++this.currentPlayer >= this.players.Count)
                NewRound();

            CreateAnomalies();
            RemoveTeleporters();

            StartPlayerTurn(handler);

            if (CurrentPlayer.AI == null)
                AutoSave();

            CurrentPlayer.PlayTurn(handler);
        }

        public void AutoSave()
        {
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
            Player[] researchOrder = GetResearchOrder();
            //research victory happens when the top player exceeds a certain multiple of the second place player
            if (researchOrder.Length > 1 && researchOrder[0].Research >
                    Game.Random.Gaussian(researchOrder[1].Research * Consts.ResearchVictoryMult, Consts.ResearchVictoryRndm))
            {
                researchOrder[0].Destroy();
                RemovePlayer(researchOrder[0]);
                this.winningPlayers.Add(new Result(researchOrder[0], true));
            }
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
            if (teleporters.Count > 0)
            {
                double chance = Math.Pow(teleporters.Count - 1.0, 1.69) + 1.0;
                if (Game.Random.Bool(chance / ( chance + 52.0 ) / (double)players.Count))
                    RemoveTeleporter(teleporters[Random.Next(teleporters.Count)]);
            }
        }

        internal bool CreateTeleporter(IEventHandler handler, Tile tile, Tile target)
        {
            double chance = Math.Pow(teleporters.Count + 1.3, 1.3);
            //check if the tiles are too close to be useful or if either tile already has a teleporter
            if (Tile.GetDistance(tile, target) > 1 && tile.Teleporter == null && target.Teleporter == null && Game.Random.Bool(1.0 / chance))
            {
                //check this will not make any planets be too close
                int closeThis = int.MaxValue, closTrg = int.MaxValue;
                foreach (Planet planet in this.planets)
                {
                    closeThis = Math.Min(closeThis, Tile.GetDistance(tile, planet.Tile));
                    closTrg = Math.Min(closTrg, Tile.GetDistance(target, planet.Tile));
                }

                //check and make sure enemies cannot be attacked/invaded
                if (closeThis + closTrg + 1 > Consts.PlanetDistance)
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
        private bool CheckAttInvPlayers(ISpaceObject obj, bool inv, Tile t1, Tile t2)
        {
            HashSet<ISpaceObject> before = Anomaly.GetAttInv(obj.Tile, inv);
            Tuple<Tile, Tile> teleporter = CreateTeleporter(t1, t2);
            HashSet<ISpaceObject> after = Anomaly.GetAttInv(obj.Tile, inv);
            RemoveTeleporter(teleporter);
            foreach (ISpaceObject other in after)
                if (other.Player != obj.Player && !before.Contains(other))
                    return false;
            return true;
        }
        private Tuple<Tile, Tile> CreateTeleporter(Tile t1, Tile t2)
        {
            Tuple<Tile, Tile> teleporter = new Tuple<Tile, Tile>(t1, t2);
            this.teleporters.Add(teleporter);
            return teleporter;
        }

        private void RemoveTeleporter(Tuple<Tile, Tile> teleporter)
        {
            this.teleporters.Remove(teleporter);
        }

        internal Planet CreateAnomalyPlanet(IEventHandler handler, Tile tile)
        {
            if (Random.Bool(this.planetPct / this.anomalyPct))
                if (CheckPlanetDistance(tile))
                {
                    handler.Explore(Anomaly.AnomalyType.NewPlanet);
                    return CreatePlanet(tile);
                }
            return null;
        }
        internal bool CheckPlanetDistance(Tile tile)
        {
            if (Tile.GetNeighbors(tile).Count < 6)
                return false;
            foreach (Planet planet in this.planets)
                if (Tile.GetDistance(tile, planet.Tile) <= Consts.PlanetDistance)
                    return false;
            return true;
        }
        internal Planet CreatePlanet(Tile tile)
        {
            Planet planet = new Planet(tile);
            this.planets.Add(planet);
            return planet;
        }

        private void CreateAnomalies()
        {
            double numPlayers = this.players.Count;
            int create = Game.Random.OEInt(this.anomalyPct / numPlayers);
            for (int a = 0 ; a < create ; ++a)
                CreateAnomaly();
            this.planetPct = Random.GaussianCapped(this.planetPct, .0052 / numPlayers,
                     Math.Max(0, 2 * this.planetPct - this.anomalyPct) + Consts.FLOAT_ERROR);
            this.anomalyPct = Random.GaussianOE(this.anomalyPct, .0065 / numPlayers,
                    .0091 / numPlayers, this.planetPct + Consts.FLOAT_ERROR);
        }
        private Anomaly CreateAnomaly()
        {
            Tile tile = GetRandomTile();
            if (tile.SpaceObject == null)
                return new Anomaly(tile);
            return null;
        }

        internal Tile GetRandomTile()
        {
            Tile tile;
            do
                tile = this.map[Random.Next(Diameter), Random.Next(Diameter)];
            while (tile == null);
            return tile;
        }

        private void StartPlayerTurn(IEventHandler handler)
        {
            Graphs.StartTurn(CurrentPlayer);
            CurrentPlayer.StartTurn(handler);

            ClearUndoStack();
        }

        public void SaveGame(string filePath)
        {
            TBSUtil.SaveGame(this, filePath);
        }

        public static Game LoadGame(string filePath)
        {
            return TBSUtil.LoadGame<Game>(filePath);
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
