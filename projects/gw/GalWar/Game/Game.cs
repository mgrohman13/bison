using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using MattUtil;

namespace GalWar
{
    [Serializable]
    public class Game
    {
        #region static

        public static readonly MTRandom Random = new MTRandom();

        #endregion //static

        #region fields and constructors

        [NonSerialized]
        public string AutoSavePath = "../../../auto";

        public readonly Graphs Graphs;

        public readonly StoreProd StoreProd;
        public readonly Soldiering Soldiering;

        public readonly int Diameter;

        internal readonly ShipNames ShipNames;

        private readonly Tile[,] map;
        private readonly List<Planet> planets;
        private Player[] players;

        private readonly List<Result> deadPlayers;
        private readonly List<Result> winningPlayers;

        private byte _currentPlayer;
        private ushort _turn;
        private readonly double _planetPct;

        public Game(Player[] players, int radius, double planetPct)
        {
            int numPlayers = players.Length;
            this.Diameter = radius * 2 - 1;

            AssertException.Assert(players != null);
            AssertException.Assert(numPlayers > 1);
            AssertException.Assert(numPlayers * 78 < MapSize);
            AssertException.Assert(radius < 70);
            AssertException.Assert(planetPct >= 0);
            AssertException.Assert(planetPct < 0.13);

            this.StoreProd = new StoreProd();
            this.Soldiering = new Soldiering();

            //set up map hexagon
            this.map = new Tile[Diameter, Diameter];
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

            planetPct *= MapSize;
            int planets = Random.GaussianCappedInt(planetPct, .21);
            this._planetPct = Random.GaussianCapped(planetPct * Consts.PlanetCreationRate, .169);
            this.planets = new List<Planet>(planets + numPlayers);
            //first create enough planets for homeworlds
            while (this.planets.Count < numPlayers)
                NewPlanet();
            //each additional starting planet is just a chance of creating one
            for (int i = 0 ; i < planets ; ++i)
                NewPlanet();

            int startPop = GetStartInt(Consts.StartPopulation);
            double soldiers = GetStartDouble(startPop);
            double startGold = GetStartDouble(Consts.StartGold * numPlayers / (double)this.planets.Count);
            //set later based on colony ship costs
            double startProd = -1;

            List<int> research = new List<int>(4);
            research.Add(GetStartInt(Consts.StartResearch * 1 / 4.0));
            research.Add(GetStartInt(Consts.StartResearch * 2 / 4.0));
            research.Add(GetStartInt(Consts.StartResearch * 3 / 4.0));
            research.Add(GetStartInt(Consts.StartResearch * 4 / 4.0));
            //ensure the List is in order despite randomness
            research.Sort();

            this.players = new Player[numPlayers];
            int index = 0;
            this.ShipNames = new ShipNames(numPlayers);
            foreach (Player player in Random.Iterate<Player>(players))
            {
                Planet homeworld = GetHomeworld(startPop);
                double gold = startGold / homeworld.Quality + index * Consts.GetMoveOrderGold(numPlayers);
                int needProd;
                this.players[index] = new Player(index, this, player, homeworld, startPop, soldiers, gold, research, out needProd);
                //all players start with the highest colony ship design production
                startProd = Math.Max(startProd, needProd);
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

            this.currentPlayer = byte.MaxValue;
            this.turn = 0;

            this.deadPlayers = new List<Result>(numPlayers - 1);
            this.winningPlayers = new List<Result>(numPlayers - 1);

            this.Graphs = new Graphs(this);
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

        private static int GetStartInt(double avg)
        {
            return Random.GaussianCappedInt(avg, Consts.StartRndm, Random.Round(avg * Consts.StartMinMult));
        }

        private static double GetStartDouble(double avg)
        {
            return Random.GaussianCapped(avg, Consts.StartRndm, avg * Consts.StartMinMult);
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

            Planet homeworld;
            if (homeworldCount == 1)
                homeworld = homeworlds[0];
            else
                homeworld = homeworlds[Random.Next(homeworldCount)];

            return homeworld;
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

        #endregion //fields and constructors

        #region internal

        internal double AvgResearch
        {
            get
            {
                double avgResearch = 0;
                foreach (Player player in this.players)
                    avgResearch += player.LastResearched;
                return avgResearch / this.players.Length;
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
            if (this.players.Length == 1)
                this.winningPlayers.Add(new Result(this.players[0], true));
        }

        private void RemovePlayer(Player player)
        {
            Player[] newPlayers = new Player[this.players.Length - 1];
            bool found = false;
            for (int i = 0 ; i < this.players.Length ; ++i)
            {
                if (this.players[i] == player)
                {
                    found = true;
                    if (this.currentPlayer > i)
                        --this.currentPlayer;
                    else if (this.currentPlayer == i)
                        throw new Exception();
                }
                else
                {
                    //players farther back move up in the order
                    newPlayers[i - ( found ? 1 : 0 )] = this.players[i];
                }
            }
            this.players = newPlayers;
        }

        #endregion //internal

        #region    public

        public int MapSize
        {
            get
            {
                int radius = ( Diameter + 1 ) / 2;
                return 3 * radius * radius - 3 * radius + 1;
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
            handler = new HandlerWrapper(handler);

            foreach (Player p in players)
                this.Graphs.StartTurn(p);
            this.Graphs.Increment(this);

            this.currentPlayer = 0;
            this.turn = 1;

            foreach (Player p in players)
                p.SetGame(this);

            StartPlayerTurn(handler);
        }

        public Tile[,] GetMap()
        {
            return (Tile[,])this.map.Clone();
        }

        //research is given only as a percentage of the amount needed to win the game
        public Dictionary<Player, double> GetResearch()
        {
            Player[] players = GetResearchOrder();
            double[] rawValues = new double[players.Length];
            for (int a = 0 ; a < players.Length ; ++a)
            {
                Player player = players[a];
                double research;
                if (players.Length > 1)
                    research = player.Research / (double)players[1].Research / Consts.ResearchVictoryMult;
                else
                    research = 1 / Consts.ResearchVictoryMult;
                rawValues[a] = research;
            }
            Dictionary<Player, double> retVal = new Dictionary<Player, double>(players.Length);
            for (int b = 0 ; b < players.Length ; ++b)
            {
                double value = rawValues[b];
                if (b != 1)
                {
                    double skew = players[b].ResearchDisplaySkew;
                    double low, high;
                    if (b == 0)
                    {
                        low = 1 / Consts.ResearchVictoryMult;
                        high = 1 - Consts.FLOAT_ERROR;
                        if (value < high)
                            skew *= ( 1 - value ) / ( 2 - value ) * ( 2 - low ) / ( 1 - low );
                        else
                            skew = 0;
                    }
                    else
                    {
                        if (b + 1 < rawValues.Length)
                            low = rawValues[b + 1];
                        else
                            low = 0;
                        high = rawValues[b - 1];
                    }
                    double s2 = Math.Min(high - value, value - low);
                    if (skew < 0)
                        value = value + -s2 * ( skew / ( skew - 1 ) );
                    else
                        value = value + s2 * ( skew / ( skew + 1 ) );
                }
                retVal.Add(players[b], value * 100);
                rawValues[b] = value;
            }
            return retVal;
        }

        private Player[] GetResearchOrder()
        {
            Player[] players = (Player[])this.players.Clone();
            Array.Sort<Player>(players, delegate(Player p1, Player p2)
            {
                //descending sort
                return p2.Research - p1.Research;
            });
            return players;
        }

        public Player[] GetPlayers()
        {
            return (Player[])this.players.Clone();
        }

        public ReadOnlyCollection<Planet> GetPlanets()
        {
            return this.planets.AsReadOnly();
        }

        //blerg
        public void EndTurn(IEventHandler handler)
        {
            handler = new HandlerWrapper(handler);

            CurrentPlayer.EndTurn(handler);
            Graphs.EndTurn(CurrentPlayer);

            if (++this.currentPlayer >= this.players.Length)
                NewRound();

            StartPlayerTurn(handler);

            AutoSave();

            CurrentPlayer.PlayTurn(handler);
        }

        public void AutoSave()
        {
            //TBSUtil.SaveGame(this, AutoSavePath, turn + ".gws");
        }

        private void NewRound()
        {
            //just so an exception is thrown if current player is mistakenly used
            this.currentPlayer = byte.MaxValue;

            CheckResearchVictory();
            RandMoveOrder();
            CreatePlanets();

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
            if (researchOrder.Length > 1 && researchOrder[0].Research > researchOrder[1].Research * Consts.ResearchVictoryMult)
            {
                researchOrder[0].Destroy();
                RemovePlayer(researchOrder[0]);
                this.winningPlayers.Add(new Result(researchOrder[0], true));
            }
        }

        private void RandMoveOrder()
        {
            Dictionary<Player, int> playerGold = MattUtil.TBSUtil.RandMoveOrder<Player>(Random, this.players, Consts.MoveOrderShuffle);
            if (playerGold.Count > 0)
            {
                double moveOrderGold = Consts.GetMoveOrderGold(this.players.Length);
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

        private void CreatePlanets()
        {
            int planets = Game.Random.OEInt(this._planetPct);
            for (int i = 0 ; i < planets ; ++i)
                NewPlanet();
        }

        private Planet NewPlanet()
        {
            Tile tile;
            do
            {
                int x = Random.Next(Diameter), y = Random.Next(Diameter);
                tile = this.map[x, y];
                //planets cannot be right on the map edge so tile must have 6 neighbors
            } while (tile == null || Tile.GetNeighbors(tile).Count < 6);

            if (tile.SpaceObject == null)
            {
                int distance = int.MaxValue;
                foreach (Planet planet in this.planets)
                    distance = Math.Min(distance, Tile.GetDistance(tile.X, tile.Y, planet.Tile.X, planet.Tile.Y));

                if (distance > Consts.PlanetDistance)
                {
                    Planet planet = new Planet(tile);
                    this.planets.Add(planet);
                    return planet;
                }
            }
            //dont retry if it cant be placed because of occupied space or existing planet proximity
            return null;
        }

        private void StartPlayerTurn(IEventHandler handler)
        {
            Graphs.StartTurn(CurrentPlayer);
            CurrentPlayer.StartTurn(handler);
        }

        public void SaveGame(string filePath)
        {
            TBSUtil.SaveGame(this, filePath);
        }

        public static Game LoadGame(string filePath)
        {
            Game game = TBSUtil.LoadGame<Game>(filePath);
            foreach (Player player in game.GetPlayers())
                player.SetGame(game);
            return game;
        }

        public List<Result> GetGameResult()
        {
            AssertException.Assert(this.players.Length == 1);

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

        [Serializable]
        public class Result
        {
            public readonly Player Player;

            private sbyte _points;

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

            internal Result(Player player, bool won)
            {
                this.Player = player;
                //extra points are awarded for gaining a research victory as quickly as possible
                double mult = Math.Pow(player.Game.MapSize, Consts.PointsTilesPower) / (double)player.Game.Turn;
                this.Points = Random.Round(mult * ( won ? Consts.WinPointsMult : Consts.LosePointsMult ));
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
    }
}
