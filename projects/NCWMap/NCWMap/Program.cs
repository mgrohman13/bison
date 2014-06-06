using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MattUtil;

namespace NCWMap
{
    public static class Program
    {
        public const int Width = 18, Height = 18;

        public static MTRandom Random;

        public static Tile[,] Map;
        public static Player[] Players;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Random = new MTRandom();
            Random.StartTick();


            //SortedDictionary<int, int> algDist = new SortedDictionary<int, int>();
            //int algTot = 1000000;
            //for (int a = 0 ; a < algTot ; ++a)
            //{
            //    int amt = GetResourceAmt(), num;
            //    algDist.TryGetValue(amt, out num);
            //    algDist[amt] = num + 1;
            //}


            //SortedDictionary<int, int>[] dist = new SortedDictionary<int, int>[] { new SortedDictionary<int, int>(), new SortedDictionary<int, int>(), new SortedDictionary<int, int>() };
            //int[] r2 = new int[3];
            //InitMap();
            //int total = 1000000;
            //for (int a = 0 ; a < total ; ++a)
            //{
            //    int[] res = new int[3];
            //    Player p = new Player("", Map[0, 0], 0);
            //    for (int b = 0 ; b < 3 ; ++b)
            //    {
            //        res[b] += p.Resources[b, 0];
            //        r2[b] += p.Resources[b, 0];
            //    }
            //    int tier = int.Parse(p.Unit[0].ToString());
            //    res[tier - 1] += tier * 2;
            //    r2[tier - 1] += tier * 2;
            //    for (int b = 0 ; b < 3 ; ++b)
            //    {
            //        int v;
            //        dist[b].TryGetValue(res[b], out v);
            //        dist[b][res[b]] = v + 1;
            //    }
            //}
            //total = 0;
            //for (int b = 0 ; b < 3 ; ++b)
            //    total += r2[b];


            CreateMap();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ShowMap());

            Random.Dispose();
        }

        public static void DoMore()
        {
        }

        private static void CreateMap()
        {
            CreateTerrain();
            CreateResources();
            CreatePlayers();
            CreateCitySpots();
        }

        private static void CreateTerrain()
        {
            InitMap();
            int countWater = InitWater();
            FillWater(countWater);
        }
        private static void InitMap()
        {
            Map = new Tile[Width, Height];
            for (int x = 0 ; x < Width ; ++x)
                for (int y = 0 ; y < Height ; ++y)
                    Map[x, y] = new Tile(x, y);
        }
        private static int InitWater()
        {
            //all tiles have a chance of initially being water
            foreach (Tile tile in Map)
                tile.Water = Random.Next(3) == 0;

            //select the largest block of contiuous land as the mainland
            KeepLargest(false);

            //select the largest lake
            return KeepLargest(true);
        }
        private static int KeepLargest(bool water)
        {
            //find the largest block
            Tile main = null;
            int count = int.MinValue;
            foreach (Tile tile in Random.Iterate(Map.Cast<Tile>()))
                if (tile.Water == water && ( main == null || !CanReach(tile, main, water) ))
                {
                    int cur = Map.Cast<Tile>().Count(t => t.Water == water && CanReach(tile, t, water));
                    if (count < cur)
                    {
                        main = tile;
                        count = cur;
                    }
                }

            //remove all others
            foreach (Tile tile in Map)
                if (tile.Water == water && !CanReach(tile, main, water))
                    tile.Water = !water;

            return count;
        }
        private static void FillWater(int countWater)
        {
            int targetWater = Random.Round(Width * Height / 3.0);
            bool water = true;
            if (countWater > targetWater)
            {
                //we actually need land to creep in on the excess water instead
                countWater = Width * Height - countWater;
                targetWater = Width * Height - targetWater;
                water = false;
            }
            while (countWater++ < targetWater)
                ChangeTile(water);
        }
        private static void ChangeTile(bool water)
        {
            //select a random water tile that has land neighbors
            foreach (Tile waterTile in Random.Iterate(Map.Cast<Tile>()))
                if (waterTile.Water == water)
                {
                    var neighbors = waterTile.GetNeighbors().Where(n => n.Water != water);
                    if (neighbors.Any())
                        //change a random neighbor, if possible
                        foreach (Tile test in Random.Iterate(neighbors))
                        {
                            neighbors = test.GetNeighbors().Where(n => n.Water != water);
                            Tile land = neighbors.First();
                            //test if the change is valid
                            test.Water = water;
                            if (neighbors.All(n => CanReach(n, land, !water)))
                                return;
                            else
                                test.Water = !water;
                        }
                }
            throw new Exception();
        }
        private static bool CanReach(Tile t1, Tile t2, bool water)
        {
            return ( t1 == t2 || null != TBSUtil.PathFind(Random, t1, t2,
                    t => t.GetNeighbors().Where(n => n.Water == water).Select(n => Tuple.Create(n, 1)), Tile.GetDistance) );
        }

        private static void CreateResources()
        {
            //each map sector gets 2 resources
            for (int a = 0 ; a < 2 ; ++a)
                for (int x = 0 ; x < 3 ; ++x)
                    for (int y = 0 ; y < 3 ; ++y)
                        CreateResource(GetSectorPoint(x, y, true));

            //6 more randomly throughout map
            for (int b = 0 ; b < 6 ; ++b)
                CreateResource(Map[Random.Next(Width), Random.Next(Height)]);
        }
        private static void CreateResource(Tile t)
        {
            int type, amt;
            if (t.Inf == null)
            {
                type = Random.RangeInt(1, 3);
                amt = 0;
            }
            else
            {
                //we haven't placed anything else yet so the only tile info is another resource
                type = int.Parse(t.Inf[1]);
                amt = int.Parse(t.Inf[2]) * 6 + int.Parse(t.Inf[3]);
            }

            amt += GetResourceAmt();

            t.Inf = new string[] { "T", type.ToString(), ( amt / 6 ).ToString(), ( amt % 6 ).ToString() };
        }
        private static int GetResourceAmt()
        {
            //same algorithm as done in-game
            int amt = 0;
            int zeroes = 0;
            while (zeroes != 2)
            {
                ++amt;
                if (Random.Next(6) == 0)
                    ++zeroes;
            }
            if (Random.Bool())
                --amt;
            else
                ++amt;
            return amt;
        }

        private static void CreatePlayers()
        {
            Dictionary<string, Tile> tiles = PlayerStartTiles(new[] { "GRN", "BLK", "PNK", "YLW", "BLE", "RED" });
            PlayerResources(tiles);
        }
        private static Dictionary<string, Tile> PlayerStartTiles(string[] players)
        {
            Dictionary<string, Tile> result = new Dictionary<string, Tile>();

            IEnumerator<Point> sectors = Random.Iterate(3, 3).GetEnumerator();
            foreach (string player in Random.Iterate(players))
            {
                //each player starts in a different sector
                sectors.MoveNext();
                Point sector = sectors.Current;

                Tile tile;
                do
                    tile = GetSectorPoint(sector.X, sector.Y, false);
                //must have at least 4 tiles in between players
                while (tile.Inf != null || tile.GetNeighbors()
                        .SelectMany(t => t.GetNeighbors()).SelectMany(t => t.GetNeighbors()).SelectMany(t => t.GetNeighbors())
                        .Any(t => t.Inf != null && t.Inf.Length == 2));

                tile.Inf = new[] { player, null };

                result.Add(player, tile);
            }
            return result;
        }
        private static void PlayerResources(Dictionary<string, Tile> players)
        {
            int idx = 0;
            Players = new Player[players.Count];
            foreach (var pair in Random.Iterate(players))
            {
                //starting resources and units created and stored in Player object
                Players[idx] = new Player(pair.Key, pair.Value, idx);
                ++idx;
            }
        }

        private static void CreateCitySpots()
        {
            //each sector gets a city spot
            foreach (Point sector in Random.Iterate(3, 3))
            {
                Tile tile;
                do
                {
                    tile = GetSectorPoint(sector.X, sector.Y, true);
                }
                while (tile.Inf != null);
                tile.Inf = new[] { "CTY", null };
            }
        }

        private static Tile GetSectorPoint(int x, int y, bool edge)
        {
            int val = edge ? 0 : 1;
            return Map[Random.RangeInt(x * 6 + val, x * 6 + 5 - val), Random.RangeInt(y * 6 + val, y * 6 + 5 - val)];
        }
    }
}
