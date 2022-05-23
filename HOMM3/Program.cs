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

        private static string name;
        public static string Name => name;
        private static double size;
        public static string Size => name;

        static void Main()
        {
            rand = new();
            rand.StartTick();

            name = "Matt " + Program.rand.RangeInt(1000, 9999);
            Player[] players = InitPlayers();
            int sizeInt = InitSize(players.Length);

            Pack pack = new();
            Map map = new(sizeInt);

            bool pairPlayers = rand.Bool(Math.Pow((players.Length - 1.69) / 7.8, .65));
            List<Zone> zones = Zone.InitZones(players, size);
            List<Connections> connections = Connections.InitConnections(players, size, zones.Count, pairPlayers);
            Player.InitMines(players, pairPlayers);

            double zoneSize = 1296 * size / (double)zones.Count;
            Player.Generate(players, connections, zoneSize);

            Output(pack, map, zones.ToArray(), connections.ToArray());
        }

        private static Player[] InitPlayers()
        {
            int numPlayers;
            do
                numPlayers = rand.GaussianOEInt(3.9, .169, .13, 2);
            while (numPlayers > 8);
            return Enumerable.Range(0, numPlayers).Select(_ => new Player()).ToArray();
        }
        private static int InitSize(int numPlayers)
        {
            int sizeInt = SelectSize(numPlayers);
            size = sizeInt;
            //idk why, but the largest map size is off by 1
            if (sizeInt == 98)
                sizeInt = 99;
            return sizeInt;
        }
        private static int SelectSize(int numPlayers)
        {
            int[] allSizes = new int[] { 1, 2, 4, 8, 9, 16, 18, 25, 32, 36, 49, 50, 72, 98 };
            int[] surfaceSizes = new int[] { 1, 4, 9, 16, 25, 36, 49 };

            bool all = false;//todo: underground

            int[] sizes = all ? allSizes : surfaceSizes;
            int min = sizes[0], max = sizes[^1];

            double avg = 6.5 * Math.Sqrt(numPlayers / 3.9);
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

        private static void Output(Pack pack, Map map, Zone[] zones, Connections[] connections)
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
