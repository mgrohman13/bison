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
        private static int numPlayers;
        public static int NumPlayers => numPlayers;
        private static double size;
        //public static double Size => size;
        //private static string humanColor;
        //public static string HumanColor => humanColor;

        static void Main()
        {
            rand = new();
            rand.StartTick();

            name = "Matt " + Program.rand.RangeInt(1000, 9999);
            bool pairPlayers = rand.Bool();// rand.Bool(Math.Pow((Program.NumPlayers - 1.69) / 7.8, .65));
            Player[] players = InitPlayers(pairPlayers);
            int sizeInt = InitSize(pairPlayers);

            Pack pack = new();
            Map map = new(sizeInt);

            List<Zone> zones = Zone.InitZones(players, size);
            List<Connections> connections = Connections.InitConnections(players, size, zones.Count, pairPlayers);
            Player.InitMines(players, zones, pairPlayers, size);

            Player.Generate(players, connections);

            map.Generate(size, zones.Count);
            Output(pack, map, zones.ToArray(), connections.ToArray());
        }

        private static Player[] InitPlayers(bool pairPlayers)
        {
            do
                numPlayers = rand.GaussianOEInt(pairPlayers ? 4.5 : 3.75, .13, .091, pairPlayers ? 3 : 2);
            while (numPlayers > 8);

            Console.WriteLine("players = {0}", numPlayers);

            //this blows, but human has to be red or things get fucky
            int human = 0;
            name += string.Format(" ({0})", numPlayers);

            //int human = rand.Next(numPlayers);
            //string humanColor = GetColor(human);
            //name += string.Format(" ({0}, {1})", numPlayers, humanColor);
            //Console.WriteLine("human = {0} ({1})", human + 1, humanColor);

            int strongCount = rand.RangeInt(1, numPlayers - (pairPlayers ? 3 : 2));
            if (strongCount < 1)
                strongCount = 1;
            HashSet<int> strongAIs = rand.Iterate(Enumerable.Range(0, numPlayers).Where(id => id != human)).Take(strongCount).ToHashSet();

            var players = Enumerable.Range(0, numPlayers).Select(id => new Player(human == id, strongAIs.Contains(id))).ToArray();
            if (pairPlayers)
                Player.SetPair(players[human], rand.SelectValue(players.Where(p => !p.Human && !p.AIstrong)));
            return rand.Iterate(players.Concat(strongAIs.Select(id => new Player(players[id])))).ToArray();
        }
        //private static string GetColor(int human)
        //{
        //    switch (human)
        //    {
        //        case 0:
        //            return "Red";
        //        case 1:
        //            return "Blue";
        //        case 2:
        //            return "Tan";
        //        case 3:
        //            return "Green";
        //        case 4:
        //            return "Orange";
        //        case 5:
        //            return "Purple";
        //        case 6:
        //            return "Teal";
        //        case 7:
        //            return "Pink";
        //        default: throw new Exception();
        //    }
        //}

        private static int InitSize(bool pairPlayers)
        {
            int sizeInt = SelectSize(pairPlayers);
            size = sizeInt;
            //idk why, but the largest map size is off by 1
            if (sizeInt == 98)
                sizeInt = 99;
            return sizeInt;
        }
        private static int SelectSize(bool pairPlayers)
        {
            int[] allSizes = new int[] { 1, 2, 4, 8, 9, 16, 18, 25, 32, 36, 49, 50, 72, 98 };
            int[] surfaceSizes = new int[] { 1, 4, 9, 16, 25, 36, 49 };

            bool all = rand.Bool(.78);//todo: underground

            int[] sizes = all ? allSizes : surfaceSizes;
            int min = sizes[0], max = sizes[^1];

            double avg = 6.5 * Math.Sqrt((numPlayers - (pairPlayers ? .5 : 0)) / 2.6);
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
            File.WriteAllText(string.Format("C:/files/MMH3/HotA_RMGTemplates/matt/{0}.txt", name), o);
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
