using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HOMM3
{
    class Program
    {
        public static MattUtil.MTRandom rand;

        public static string name;
        public static int players;
        public static int sizeI;
        public static double size;
        public static double zoneSize;

        public static Pack pack;
        public static Map map;
        public static Zone[] zones;
        public static Connections[] connections;

        static void Main()
        {
            rand = new();
            rand.StartTick();

            name = "Matt " + Program.rand.RangeInt(1000, 9999);
            do
                players = rand.GaussianOEInt(3.9, .169, .13, 2);
            while (players > 8);
            sizeI = Size();
            size = sizeI;
            if (sizeI == 98)
                sizeI = 99;

            pack = new();
            map = new();

            double zoneAvg = size / (.39 * Math.Pow(size + 6.5, .39));
            //int numZones = players;
            //if (zoneAvg > players)
            int numZones = rand.GaussianOEInt(zoneAvg + players, .21, .13, players);
            zoneSize = 1296 * size / (double)numZones;

            zones = new Zone[numZones];
            HashSet<int> freeZones = Enumerable.Range(0, numZones).ToHashSet();

            int value = rand.GaussianOEInt(104000, .26, .13, 39000);

            double avgPlZon = numZones / (double)players;
            bool pairPlayers = rand.Bool(Math.Pow((players - 1.69) / 7.8, .65));
            int wo = 1 + rand.Round(pairPlayers ? .39 : .65);

            Dictionary<int, int> playerZones = new();
            Stack<int> prev = new();
            Dictionary<int, List<int>> playerArea = new();
            double disposition = rand.GaussianCapped(5.2, .169, .91);
            double joinPct = rand.GaussianCapped(1.5, .21);
            bool moneyOnly = rand.Bool(.21);
            Zone.Monsters.Str monsters = rand.Bool(.26) ? Zone.Monsters.Str.weak : Zone.Monsters.Str.avg;
            foreach (int a in rand.Iterate(numZones).Take(players).ToList())
            {
                prev.Push(a);
                zones[a] = new Zone(Zone.Type.T.Human, wo, monsters, disposition, joinPct, moneyOnly, value);
                freeZones.Remove(a);
                int pid = zones[a].player_Towns.Ownership;
                playerArea[pid] = new();
                playerArea[pid].Add(a);
                playerZones.Add(a, pid);
            }

            static double s() => rand.Range(16900, 21000);
            double strongBase = s();
            double strong(bool useBase) => rand.GaussianOE(useBase ? strongBase : s(), .13, .13);

            double wideWeight = rand.GaussianCapped((.65 + players) / (1.69 + (double)numZones), .13);
            List<Connections> edges = new();
            double maxStr = double.MinValue;
            while (freeZones.Any())
            {
                bool ground = rand.Bool(.91);
                double wide = rand.Weighted(wideWeight);
                double v = 91000;
                double str;
                do
                    str = rand.GaussianOE(9100 * (1 - wide), .39, .13);
                while (str > s());
                monsters = (Zone.Monsters.Str)rand.Next(4);
                disposition = .91 + rand.Weighted(7.8, .65);
                joinPct = rand.GaussianCapped(1.5, .26);
                moneyOnly = rand.Bool(.26);
                if (freeZones.Count < playerZones.Count)
                {
                    ground &= rand.Bool(.78);
                    wide = 0;
                    monsters = Zone.Monsters.Str.strong;
                    disposition = 7.8;
                    moneyOnly |= rand.Bool(.52);
                    joinPct = moneyOnly ? joinPct : 0;
                    str *= rand.Range(1.3, 1.69);
                }
                else if (monsters == Zone.Monsters.Str.none)
                    if (rand.Bool(.91))
                        monsters = Zone.Monsters.Str.avg;
                    else
                        v /= 2.1;

                value = rand.GaussianOEInt(v, .39, .13, 13000);
                foreach (int b in rand.Iterate(playerZones.Keys))
                {
                    int c;
                    bool flag = freeZones.Any();
                    if (flag)
                        prev.Push(c = rand.SelectValue(freeZones));
                    else
                        c = prev.Pop();
                    if (zones[c] == null)
                    {
                        zones[c] = new Zone(Zone.Type.T.Treasure, 0, monsters, disposition, joinPct, moneyOnly, value);
                        playerArea[playerZones[b]].Add(c);
                    }
                    if (b != c)
                    {
                        maxStr = Math.Max(maxStr, str);
                        edges.Add(new Connections(zones[b].Id, zones[c].Id, ground, false, wide, null, str, !flag));
                        playerZones[c] = playerZones[b];
                        playerZones.Remove(b);
                        if (flag)
                        {
                            freeZones.Remove(c);
                            if (!freeZones.Any())
                                str = strong(true);
                        }
                    }
                }
            }
            while (playerZones.Any())
            {
                double str = strong(true);
                foreach (int d in rand.Iterate(playerZones.Keys))
                    if (playerZones.Any() && rand.Bool())
                    {
                        int e;
                        if (playerZones.Count == 1 && prev.Any())
                            e = prev.Pop();
                        else
                            e = rand.SelectValue(playerZones.Keys);
                        if (d != e)
                        {
                            edges.Add(new Connections(zones[d].Id, zones[e].Id, rand.Bool(), false, 0, null, str, true));
                            playerZones.Remove(rand.Bool() ? d : e);
                            playerZones.Remove(rand.Bool() ? d : e);
                        }
                    }
            }

            double intStr = strong(false) * .78;
            double extStr = strong(false) * rand.Range(1.3, 1.69);
            int intEdges = avgPlZon > 2 ? rand.GaussianCappedInt(Math.Sqrt(avgPlZon - 1.69), .26) : 0;
            int extEdges = rand.GaussianOEInt(Math.Sqrt(numZones - 1.69) / 2.6, .39, .065);

            double mines = 2.1 + .52 * Math.Sqrt(avgPlZon);
            int wo2 = rand.GaussianOEInt(mines / 7.8, .39, .13);
            if (wo == 1 && wo2 == 0)
                wo2 = rand.Round(pairPlayers ? .13 : .65);
            int mscg = rand.GaussianOEInt(mines, .13, .13, 1);
            int g = rand.GaussianOEInt(.52 + mines / 5.2, .26, .13);

            if (pairPlayers)
            {
                --extEdges;
                if (wo == 2 || wo2 > 1)
                {
                    wo2 -= rand.RangeInt(0, wo + wo2 > 2 ? 2 : 1);
                    if (wo2 < 0)
                        wo2 = 0;
                }
                mscg = rand.Round((mscg + 1) / 2.0);
                g = rand.Round(g / 2.0);
            }
            wo = wo2;

            foreach (var pair in rand.Iterate(playerArea))
            {
                Zone home = zones[pair.Value.Where(z => zones[z].player_Towns.Ownership > 0).Single()];
                IEnumerable<Zone> other = rand.Iterate(pair.Value.Where(z => zones[z].player_Towns.Ownership <= 0)).Select(z => zones[z]);

                int h2 = intEdges + rand.Next(2);
                if (pair.Value.Count > 1)
                    for (int h = 0; h < h2; h++)
                        if (rand.Bool())
                        {
                            int z1 = rand.SelectValue(pair.Value), z2;
                            do
                                z2 = rand.SelectValue(pair.Value);
                            while (z1 == z2);
                            edges.Add(new Connections(zones[z1].Id, zones[z2].Id, false, rand.Bool(.13), 0, null, intStr, false));
                        }
                for (int k = 0; k < extEdges; k++)
                    if (rand.Bool(1 / Math.Sqrt(players)))
                    {
                        Zone z1 = home;
                        if (other.Any() && rand.Bool(.91))
                            z1 = other.First();
                        int z2;
                        do
                        {
                            z2 = rand.Next(numZones);
                        } while (pair.Value.Contains(z2));
                        edges.Add(new Connections(z1.Id, zones[z2].Id, false, true, 0, rand.Bool(.169), extStr, true));
                    }

                int a = wo, b = mscg, c = g;
                double div = Math.Sqrt(pair.Value.Count - .39);
                foreach (Zone z in other)
                {
                    int d = rand.Round(a / div);
                    int e = rand.Round(b / div);
                    int f = rand.Round(c / div);
                    z.AddMines(d, e, f, !pairPlayers);
                    a -= d;
                    b -= e;
                    c -= f;
                }
                home.AddMines(a, b, c, !pairPlayers);
            }

            if (pairPlayers)
            {
                double str;
                do
                    str = rand.GaussianOE(9100, .21, .169);
                while (str > rand.Gaussian(maxStr, .13));
                while (playerArea.Count > 1)
                {
                    var p1 = rand.SelectValue(playerArea);
                    playerArea.Remove(p1.Key);
                    var p2 = rand.SelectValue(playerArea);
                    playerArea.Remove(p2.Key);
                    int z1 = rand.SelectValue(p1.Value);
                    int z2 = rand.SelectValue(p2.Value);
                    edges.Add(new Connections(zones[z1].Id, zones[z2].Id, rand.Bool(.65), false, 0, null, str, true));
                    foreach (int z in rand.Iterate(p1.Value).Concat(rand.Iterate(p2.Value)))
                        zones[z].PlaceMines();
                }
                if (playerArea.Any())
                    foreach (int z in rand.Iterate(playerArea.Single().Value))
                        zones[z].PlaceMines();
            }

            connections = edges.ToArray();
            Output();
        }

        private static int Size()
        {
            int[] allSizes = new int[] { 1, 2, 4, 8, 9, 16, 18, 25, 32, 36, 49, 50, 72, 98 };
            int[] surfaceSizes = new int[] { 1, 4, 9, 16, 25, 36, 49 };

            bool all = false;//

            int[] sizes = all ? allSizes : surfaceSizes;
            int min = sizes[0], max = sizes[^1];
            //int diff = max - min;

            double avg = 6.5 * Math.Sqrt(players / 3.9);
            //avg = rand.GaussianCapped(avg, .13, 1.3);
            //int size = min + rand.WeightedInt(diff, (avg - min) / (double)diff);
            int size;
            do
                size = rand.GaussianOEInt(avg, .26, .13, min);
            while (size > max);

            int prev = -1;
            foreach (int val in sizes)
            {
                if (size == val)
                    return val;
                if (size > prev && size < val)
                {
                    int span = val - prev;
                    return prev + rand.Round((size - prev) / (double)span) * span;
                }
                prev = val;
            }

            throw new Exception();
        }

        private static void Output()
        {
            List<List<string>> output = new();

            Output(output, 0, 0, "Pack");
            int x = 0, y = 1;
            pack.Output(output, ref x, ref y);

            Output(output, x, 0, "Map");
            y = 1;
            map.Output(output, ref x, ref y);

            Output(output, x, 0, "Zone");
            y = 1;
            Zone.Output(output, ref x, ref y, zones);

            Output(output, x, 0, "Connections");
            y = 1;
            Connections.Output(output, ref x, ref y, connections);

            string o = output.Select(c => c.Aggregate((a, b) => a + "\t" + b)).Aggregate((a, b) => a + "\r\n" + b);
            File.WriteAllText("C:/files/MMH3/HotA_RMGTemplates/matt/" + name + ".txt", o);
        }
        public static void Output(List<List<string>> output, int x, int y, object value)
        {
            while (output.Count <= y)
                output.Add(new List<string>());
            List<string> row = output[y];
            while (row.Count <= x)
                row.Add(null);
            output[y][x] = value == null || value.ToString().StartsWith("-1") ? null : value.ToString();
        }
    }
}
