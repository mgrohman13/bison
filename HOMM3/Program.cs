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

            Dictionary<int, int> edgeRef;
            Dictionary<int, int> playerZones = new();
            Stack<int> prev = new Stack<int>();
            Dictionary<int, List<int>> playerArea = new();
            foreach (int a in rand.Iterate(numZones).Take(players).ToList())
            {
                prev.Push(a);
                zones[a] = new Zone(Zone.Type.T.Human, Zone.Monsters.Str.avg, value);
                freeZones.Remove(a);
                int pid = zones[a].player_Towns.Ownership;
                playerArea[pid] = new List<int>();
                playerArea[pid].Add(a);
                playerZones.Add(a, pid);
            }

            static double strong() => rand.GaussianOE(21000, .13, .13);

            double str;
            List<Connections> edges = new List<Connections>();
            while (freeZones.Any())
            {
                double v = 91000;
                str = rand.GaussianOE(6500, .39, .13);
                Zone.Monsters.Str monsters = (Zone.Monsters.Str)rand.Next(4);
                if (freeZones.Count < playerZones.Count)
                    monsters = Zone.Monsters.Str.strong;
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
                        zones[c] = new Zone(Zone.Type.T.Treasure, monsters, value);
                        playerArea[playerZones[b]].Add(c);
                    }
                    if (b != c)
                    {
                        edges.Add(new Connections(zones[b].Id, zones[c].Id, str, !flag));
                        playerZones[c] = playerZones[b];
                        playerZones.Remove(b);
                        if (flag)
                        {
                            freeZones.Remove(c);
                            if (!freeZones.Any())
                                str = strong();
                        }
                    }
                }
            }
            while (playerZones.Any())
            {
                str = strong();
                foreach (int d in rand.Iterate(playerZones.Keys))
                    if (playerZones.Any())
                    {
                        int e;
                        if (playerZones.Count == 1 && prev.Any())
                            e = prev.Pop();
                        else
                            e = rand.SelectValue(playerZones.Keys);
                        if (d != e)
                        {
                            edges.Add(new Connections(zones[d].Id, zones[e].Id, str, true));
                            playerZones.Remove(rand.Bool() ? d : e);
                            playerZones.Remove(rand.Bool() ? d : e);
                        }
                    }
            }

            double avgPlZon = numZones / (double)players;
            double intStr = strong() * .91;
            double extStr = strong() * 1.69;
            int intEdges = avgPlZon > 2 ? rand.GaussianCappedInt(Math.Sqrt(avgPlZon - 1.69), .26) : 0;
            int extEdges = rand.GaussianOEInt(Math.Sqrt(numZones - 1.69) / 2.6, .39, .065);

            double mines = 2.1 + .52 * Math.Sqrt(avgPlZon);
            int wo = rand.GaussianOEInt(mines / 7.8, .39, .13);
            int mscg = rand.GaussianOEInt(mines, .13, .13);
            int g = rand.GaussianOEInt(.52 + mines / 5.2, .26, .13);
            foreach (var pair in playerArea)
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
                            edges.Add(new Connections(zones[z1].Id, zones[z2].Id, intStr, false));
                        }
                for (int k = 0; k < extEdges; k++)
                {
                    Zone z1 = home;
                    if (other.Any() && rand.Bool(.91))
                        z1 = other.First();
                    int z2;
                    do
                    {
                        z2 = rand.Next(numZones);
                    } while (pair.Value.Contains(z2));
                    edges.Add(new Connections(z1.Id, zones[z2].Id, extStr, true));
                }

                int a = wo, b = mscg, c = g;
                double div = Math.Sqrt(pair.Value.Count - .39);
                foreach (Zone z in other)
                {
                    int d = rand.Round(a / div);
                    int e = rand.Round(b / div);
                    int f = rand.Round(c / div);
                    z.SetMines(d, e, f);
                    a -= d;
                    b -= e;
                    c -= f;
                }
                home.SetMines(a, b, c);
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
