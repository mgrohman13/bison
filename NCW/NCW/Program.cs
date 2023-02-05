using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MattUtil;

namespace NCWMap
{
    public static class Program
    {
        public static MTRandom Random;

        public static Tile[,] Map;
        public static Player[] Players;

        private static bool log = true;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Random = new MTRandom();
            Random.StartTick();
            log = false;


            //int[] res = new int[7];
            //for (int a = 0 ; a < 2430000 ; a++)
            //{
            //    int amt = Program.Random.SelectValue(new Dictionary<int, int> { { 0, 1 }, { 2, 2 }, { 4, 3 } });
            //    res[amt]++;
            //}
            //;


            //for (int att = 2 ; att <= 6 ; ++att)
            //    for (int def = 1 ; def <= 4 ; ++def)
            //        if (att >= def)
            //            for (int divFlag = 0 ; divFlag < 2 ; ++divFlag)
            //                if (divFlag == 1 || ( att == 4 && def < 3 ))
            //                {
            //                    if (divFlag == 0 && def == 2)
            //                        def = 3;
            //                    bool divDef = ( divFlag == 0 );
            //                     double[] r = new double[26];
            //                    for (int attHP = 1 ; attHP <= (int)Math.Ceiling(def * 2.5) ; ++attHP)
            //                        r[-attHP + 10] = new BattleResult(att, def, divDef, attHP, 16).AttKill;
            //                    for (int defHP = 1 ; defHP <= (int)Math.Ceiling(att * 2.5) ; ++defHP)
            //                        r[defHP + 10] = new BattleResult(att, def, divDef, 16, defHP).DefKill;
            //                    Console.Write("{0}\t{1}\t", att, divDef ? ( def / 2.0 ).ToString("0.0") : def.ToString());
            //                    for (int a = 0 ; a < 26 ; ++a)
            //                        Console.Write("\t" + r[a]);
            //                    Console.WriteLine();
            //                    if (divFlag == 0 && def == 3)
            //                        def = 2;
            //                }


            //foreach (Point p in Random.Iterate(2, 6, 1, 4))
            //{
            //    int att = p.X, def = p.Y;
            //    if (att >= def)
            //        for (int divFlag = 0 ; divFlag < 2 ; ++divFlag)
            //        {
            //            bool divDef = ( att == 4 && divFlag == 0 );
            //            int attHP = 1 + Random.WeightedInt(35, 5.0 / 35), defHP = 1 + Random.WeightedInt(35, 5.0 / 35);
            //            BattleResult br = new BattleResult(att, def, divDef, attHP, defHP);
            //            int pos = 0, kill = 0, neg = 0, die = 0, tot = 100000;
            //            for (int a = 0 ; a < tot ; ++a)
            //            {
            //                int ad = 0, dd = 0;
            //                for (int b = 0 ; b < att ; ++b)
            //                    ad += Random.Next(6);
            //                int ct = ( divDef ? Random.Round(def / 2.0) : def );
            //                for (int c = 0 ; c < ct ; ++c)
            //                    dd += Random.Next(6);
            //                if (ad > dd)
            //                {
            //                    int dmg = Random.Round(( ad - dd ) / 2.0);
            //                    if (dmg >= defHP)
            //                    {
            //                        dmg = defHP;
            //                        kill++;
            //                    }
            //                    pos += dmg;
            //                }
            //                else
            //                {
            //                    int dmg = Random.Round(( dd - ad ) / 2.0);
            //                    if (dmg >= attHP)
            //                    {
            //                        dmg = attHP;
            //                        die++;
            //                    }
            //                    neg += dmg;
            //                }
            //            }
            //            double attKill, defKill, attDmg, defDmg;
            //            attDmg = neg / (double)tot;
            //            defDmg = pos / (double)tot;
            //            attKill = die / (double)tot;
            //            defKill = kill / (double)tot;
            //        }
            //}


            //SortedDictionary<int, int> algDist = new SortedDictionary<int, int>();
            //int algTot = 1000000;
            //for (int a = 0; a < algTot; a++)
            //{
            //    int amt = GetResourceAmt(), num;
            //    algDist.TryGetValue(amt, out num);
            //    algDist[amt] = num + 1;
            //}
            //Console.WriteLine(algDist.Sum(p => p.Key * p.Value) / (double)algTot);
            //Console.WriteLine();
            //for (int b = 0; b <= algDist.Keys.Max(); b++)
            //{
            //    int v;
            //    algDist.TryGetValue(b, out v);
            //    Console.WriteLine(b + "\t" + (v / (double)algTot));
            //}
            //;

            //SortedDictionary<int, int>[] dist = new SortedDictionary<int, int>[] {
            //        new SortedDictionary<int, int>(), new SortedDictionary<int, int>(), new SortedDictionary<int, int>()
            //};
            //int[] r2 = new int[3];
            //InitMap();
            //int total = 100000;
            //for (int a = 0; a < total; ++a)
            //{
            //    int[] res = new int[3];


            //    Dictionary<string, Tile> players = new Dictionary<string, Tile> {
            //        { "1", new Tile(0, 0) },
            //        { "2", new Tile(0, 0) },
            //        { "3", new Tile(0, 0) },
            //        { "4", new Tile(0, 0) },
            //        { "5", new Tile(0, 0) },
            //        { "6", new Tile(0, 0) },
            //    };
            //    PlayerResources(players);
            //    SubtractPlayerExtra();

            //    foreach (Player p in Players)
            //    {
            //        for (int b = 0; b < 3; ++b)
            //        {
            //            int amt = p.Resources[b, 0] * 6 + p.Resources[b, 1];
            //            res[b] += amt;
            //            r2[b] += amt;
            //        }
            //        int tier = int.Parse(p.Unit[0].ToString());
            //        res[tier - 1] += tier * 12;
            //        r2[tier - 1] += tier * 12;
            //        for (int b = 0; b < 3; ++b)
            //        {
            //            int r = res[b] / 6;
            //            int v;
            //            dist[b].TryGetValue(r, out v);
            //            dist[b][r] = v + 1;
            //        }
            //    }
            //}
            //total = 0;
            //for (int b = 0; b < 3; ++b)
            //    total += r2[b];


            log = true;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Calculator());

            Random.Dispose();
        }

        private static int step, countWater;
        public static bool DoStep()
        {
            switch (step)
            {
                case 0:
                    foreach (Tile tile in Map)
                        tile.Water = (Random.Next(3) == 0);
                    Log("InitWater: " + EnumMap().Count(t => t.Water));
                    break;
                case 1:
                    KeepLargest(false);
                    break;
                case 2:
                    countWater = KeepLargest(true);
                    break;
                case 3:
                    FillWater(countWater);
                    break;
                case 4:
                    DoMore();
                    break;
                case 5:
                    CreateCitySpots();
                    break;
                case 6:
                    for (int a = 0; a < 2; ++a)
                        for (int x = 0; x < 3; ++x)
                            for (int y = 0; y < 3; ++y)
                                CreateResource(() => GetSectorTile(x, y));
                    break;
                case 7:
                    for (int b = 0; b < 6; ++b)
                        CreateResource(() => GetRandomTile());
                    break;
                case 8:
                    CreatePlayers();
                    break;
                case 9:
                    SubtractPlayerExtra();
                    break;
                default:
                    return true;
            }

            step++;
            return false;
        }

        public static void DoMore()
        {
            int turn = 1, turns = Random.OEInt(39);
            Log("DoMore total: " + turns);
            while (turn <= turns)
            {
                turn++;

                Tile t1 = GetRandomTile();
                List<Tile> n1 = t1.GetNeighbors().Where(n => n.Water != t1.Water).ToList();
                if (n1.Any())
                {
                    Tile t2 = null;
                    List<Tile> n2 = null;
                    foreach (Tile start in RandMap())
                    {
                        Tile tile = GetCoastal(new HashSet<Tile>(), start, !t1.Water, 3);
                        if (tile != null)
                        {
                            t2 = tile;
                            n2 = tile.GetNeighbors().Where(n => n.Water != tile.Water).ToList();
                            break;
                        }
                    }
                    if (t2 == null || !n2.Any())
                        throw new Exception();

                    foreach (Point point in Random.Iterate(n1.Count, n2.Count))
                        if (TrySwap(n1[point.X], n2[point.Y]))
                        {
                            turn--;
                            Log("DoMore: " + turn);
                            break;
                        }
                }
            }
        }

        private static Tile GetCoastal(HashSet<Tile> visited, Tile tile, bool targetWater, int depth)
        {
            if (visited.Contains(tile))
                return null;
            visited.Add(tile);
            if (--depth < 0)
                return null;
            foreach (Tile a in Random.Iterate(tile.GetNeighbors()))
            {
                if (a.Water != tile.Water)
                {
                    if (a.Water == targetWater)
                        return a;
                    return tile;
                }
            }
            foreach (Tile a in Random.Iterate(tile.GetNeighbors()))
            {
                Tile b = GetCoastal(visited, a, targetWater, depth);
                if (b != null)
                    return b;
            }
            return null;
        }

        private static bool TrySwap(Tile s1, Tile s2)
        {
            s1.Water = !s1.Water;
            s2.Water = !s2.Water;
            var total = new[] { s1, s2 }.Concat(s1.GetNeighbors()).Concat(s2.GetNeighbors());
            foreach (Tile t1 in total)
                foreach (Tile t2 in total)
                    if (t1.Water == t2.Water && !CanReach(t1, t2, t1.Water))
                    {
                        s1.Water = !s1.Water;
                        s2.Water = !s2.Water;
                        return false;
                    }
            return true;
        }

        public static void CreateMap()
        {
            //InitMap();
            ////CreateResources();
            //foreach (Tile t1 in Map.Cast<Tile>())
            //    if (t1.Inf == null || t1.Inf.Length == 2)
            //    {
            //        AddPick(t1, 0, 0.5);
            //        var others = Map.Cast<Tile>().Where(t2 =>
            //                ( t2.Inf == null || t2.Inf.Length == 2 ) && Tile.GetDistance(t1, t2) >= 7).ToList();
            //        double mult = 0.5 / others.Count;
            //        foreach (Tile t2 in others)
            //            AddPick(t2, 0, mult);
            //    }
            //foreach (Tile tile in Map.Cast<Tile>())
            //    if (tile.Inf.Length == 2)
            //        tile.Inf[0] = double.Parse(tile.Inf[0]).ToString("0.000");

            Players = null;
            InitMap();
            step = 0;

            //CreateTerrain();
            //CreateCitySpots();
            //CreateResources();
            //CreatePlayers();
        }
        //private static void AddPick(Tile tile, int idx, double amt)
        //{
        //    if (tile.Inf == null)
        //        tile.Inf = new[] { "0", "" };
        //    tile.Inf[idx] = ( double.Parse(tile.Inf[idx]) + amt ).ToString();
        //}

        private static void CreateTerrain()
        {
            InitMap();
            int countWater = InitWater();
            FillWater(countWater);
            DoMore();
        }
        private static void InitMap()
        {
            Map = new Tile[18, 18];
            for (int x = 0; x < 18; ++x)
                for (int y = 0; y < 18; ++y)
                    Map[x, y] = new Tile(x, y);
        }
        private static int InitWater()
        {
            //all tiles have a chance of initially being water
            foreach (Tile tile in Map)
                tile.Water = (Random.Next(3) == 0);

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
            foreach (Tile tile in RandMap())
                if (tile.Water == water && (main == null || !CanReach(tile, main, water)))
                {
                    int cur = EnumMap().Count(t2 => t2.Water == water && CanReach(tile, t2, water));
                    if (count < cur)
                    {
                        main = tile;
                        count = cur;
                    }
                    else if (count == cur)
                    {
                        Log("KeepLargest tie: " + count);
                    }
                }

            //remove all others
            int removed = 0;
            foreach (Tile tile in Map)
                if (tile.Water == water && !CanReach(tile, main, water))
                {
                    ++removed;
                    tile.Water = !water;
                }
            Log("KeepLargest removed: " + removed);

            return count;
        }
        private static void FillWater(int countWater)
        {
            int targetWater = Random.Round(18 * 18 / 3.0);
            bool water = true;
            Log("FillWater: " + countWater + " (" + targetWater + ")");
            if (countWater > targetWater)
            {
                Log("FillWater SWAP");
                //we actually need land to creep in on the excess water instead
                countWater = 18 * 18 - countWater;
                targetWater = 18 * 18 - targetWater;
                water = false;
            }
            while (countWater++ < targetWater)
                ChangeTile(water);
        }
        private static void ChangeTile(bool water)
        {
            //select a random water tile that has land neighbors
            foreach (Tile waterTile in RandMap())
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
            return (t1 == t2 || null != TBSUtil.PathFind(Random, t1, t2,
                    t => t.GetNeighbors().Where(n => n.Water == water).Select(n => Tuple.Create(n, 1)), Tile.GetDistance));
        }

        private static void CreateResources()
        {
            //each map sector gets 2 resources
            for (int a = 0; a < 2; ++a)
                for (int x = 0; x < 3; ++x)
                    for (int y = 0; y < 3; ++y)
                        CreateResource(() => GetSectorTile(x, y));

            //6 more randomly throughout map
            for (int b = 0; b < 6; ++b)
                CreateResource(() => GetRandomTile());
        }
        private static void CreateResource(Func<Tile> getTile)
        {
            Tile tile = null;
            int type = 0, amt = 0;
            do
            {
                if (tile != null && tile.Inf != null)
                    Log("CreateResource select: " + tile.Inf[0] + " (" + tile.X + "," + tile.Y + ")");
                tile = getTile();
            }
            while (tile.Inf != null && !int.TryParse(tile.Inf[1], out type));

            bool isNew = (tile.Inf == null);
            Tile neighbor = null;
            if (isNew)
            {
                List<Tile> neighbors = tile.GetNeighbors().ToList();
                int n = Random.Next(6);
                neighbor = n < neighbors.Count ? neighbors[n] : null;
                if (neighbor == null || neighbor.Inf == null || !int.TryParse(neighbor.Inf[1], out type))
                {
                    neighbor = null;
                    type = Random.RangeInt(1, 3);
                }
            }
            else
            {
                amt = int.Parse(tile.Inf[2]) * 6 + int.Parse(tile.Inf[3]);
            }

            if (!isNew)
                Log("CreateResource add: " + amt + " (" + tile.X + "," + tile.Y + ")");

            amt += GetResourceAmt();

            tile.Inf = new string[] { "T", type.ToString(), (amt / 6).ToString(), (amt % 6).ToString() };

            if (!isNew)
                Log("CreateResource final: " + tile.Inf[2] + "." + tile.Inf[3]);

            if (neighbor != null)
                Log("CreateResource neighbor: " + tile.Inf[0] + tile.Inf[1] + " (" + tile.X + "," + tile.Y + ")");
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
            Dictionary<string, Tile> tiles = PlayerStartTiles(new[] { "BLE", "BLK", "GRN", "PNK", "RED", "YLW" });
            PlayerResources(tiles);
        }
        private static Dictionary<string, Tile> PlayerStartTiles(string[] players)
        {
            Dictionary<string, Tile> result = new Dictionary<string, Tile>();

            foreach (string player in Random.Iterate(players))
            {
                Tile tile = null;
                foreach (Tile t1 in RandMap())
                    if (t1.Inf == null && !result.Values.Any(t2 => Tile.GetDistance(t1, t2) < 7))
                    {
                        tile = t1;
                        break;
                    }
                if (tile == null)
                {
                    Log("PlayerStartTiles RE-TRY");
                    return PlayerStartTiles(players);
                }

                result.Add(player, tile);
            }

            foreach (var pair in result)
                pair.Value.Inf = new[] { pair.Key, null };
            return result;
        }
        private static void PlayerResources(Dictionary<string, Tile> players)
        {
            int add = Random.RangeInt(1, 6) * Random.RangeInt(0, 5) + Random.RangeInt(0, 5);
            Log("PlayerResources: " + add);

            int idx = 0;
            Players = new Player[players.Count];
            foreach (var pair in Random.Iterate(players))
            {
                //starting resources and units are generated and stored in Player object
                Players[idx] = new Player(pair.Key, pair.Value, idx, add);
                ++idx;
            }
        }

        private static void SubtractPlayerExtra()
        {
            while (Random.Next(6 * 6 * 6) > 0)
            {
                List<Player> order = Random.Iterate(Players).ToList();
                int startIdx = Random.Next(3);

                bool subtract = true;
                int idx = startIdx;
                foreach (Player player in order)
                {
                    if (GetPlayerExtra(player)[idx] == 0)
                    {
                        subtract = false;
                        break;
                    }
                    idx++;
                    if (idx == 3)
                        idx = 0;
                }
                if (subtract)
                {
                    Log("SubtractPlayerExtra " + order[0] + " " + startIdx);
                    idx = startIdx;
                    foreach (Player player in order)
                    {
                        player.Resources[idx, 0]--;
                        idx++;
                        if (idx == 3)
                            idx = 0;
                    }
                }
            }
        }

        private static Dictionary<int, int> GetPlayerExtra(Player player)
        {
            var extra = new Dictionary<int, int>();
            for (int a = 0; a < 3; a++)
            {
                int e = player.Resources[a, 0] % (2 * (a + 1));
                if (a == 0 && e == 0 && player.Resources[a, 0] > 0 && Random.Next(6) == 0)
                    e = 1;
                if (e > 0 && player.Resources[a, 0] == 1 && player.Resources[a, 1] == 0)
                {
                    Log("GetPlayerExtra prevent: " + player.Name);
                    e = 0;
                }
                extra.Add(a, e);
            }
            return extra;
        }

        private static void CreateCitySpots()
        {
            //each sector gets a city spot
            foreach (Point sector in Random.Iterate(3, 3))
            {
                Tile tile;
                do
                    tile = GetSectorTile(sector.X, sector.Y);
                while (tile.Inf != null);
                tile.Inf = new[] { "CTY", null };
            }
        }

        private static IEnumerable<Tile> RandMap()
        {
            return Random.Iterate(EnumMap());
        }
        private static IEnumerable<Tile> EnumMap()
        {
            return Map.Cast<Tile>();
        }

        private static Tile GetRandomTile()
        {
            return Map[Random.Next(18), Random.Next(18)];
        }
        private static Tile GetSectorTile(int x, int y)
        {
            return Map[RandSector(x), RandSector(y)];
        }
        private static int RandSector(int sector)
        {
            return Random.RangeInt(sector * 6, sector * 6 + 5);
        }

        public static void Log(Object msg)
        {
            if (log)
                Console.WriteLine(msg);
        }
    }
}
