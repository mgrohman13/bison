﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HOMM3
{
    public class Program
    {
        public static readonly MattUtil.MTRandom rand = new();

        private static string name;
        public static string Name => name;
        private static int numPlayers;
        public static int NumPlayers => numPlayers;
        private static double size;
        public static double Size => size;
        private static bool pairPlayer;
        public static bool PairPlayer => pairPlayer;

        static void Main()
        {
            rand.StartTick();

            //Console.WriteLine(rand.Gaussian());

            name = "Matt " + rand.RangeInt(1000, 9999);
            pairPlayer = rand.Bool();// rand.Bool(Math.Pow((Program.NumPlayers - 1.69) / 7.8, .65));
            Log.Out("pairPlayer: {0}", pairPlayer);
            Player[] players = InitPlayers();
            int sizeInt = InitSize();

            Pack pack = new();
            Map map = new(sizeInt);

            List<Zone> zones = Zone.InitZones(players, size);
            Log.Out("numZones: {0}", zones.Count);
            List<Connections> connections = Connections.InitConnections(players, size, zones.Count);
            Player.InitMines(players, zones, size);

            Player.Generate(players, connections);

            map.Generate(size, zones.Count);
            Output(pack, map, zones.ToArray(), connections.ToArray());

            Log.Write(string.Format("{0}.log", name));
        }

        public static int GaussianOEIntWithMax(double avg, double deviation, double oe, int max, int min = 0)
        {
            double result = GaussianOEWithMax(true, avg, deviation, oe, max, min);
            if (result != (int)result)
                throw new Exception();
            return (int)result;
        }
        public static double GaussianOEWithMax(double avg, double deviation, double oe, double max, double min = 0)
        {
            return GaussianOEWithMax(false, avg, deviation, oe, max, min);
        }
        private static double GaussianOEWithMax(bool isInt, double avg, double deviation, double oe, double max, double min1)
        {
            int min2 = (int)min1;
            if (isInt && min2 != min1)
                throw new Exception();
            double result;
            double min3 = 2 * avg - max;
            int min4 = (int)Math.Ceiling(min3);
            if (isInt && min4 > avg)
                return rand.Round(avg);
            if ((isInt ? min4 : min3) >= min1)
                result = isInt ? rand.GaussianCappedInt(avg, deviation, min4) : rand.GaussianCapped(avg, deviation, min3);
            else // this logic will result in an actual lower average value than the parameter
                do
                    result = isInt ? rand.GaussianOEInt(avg, deviation, oe, min2) : rand.GaussianOE(avg, deviation, oe, min1);
                while (result > max);
            return result;
        }

        private static Player[] InitPlayers()
        {
            numPlayers = GaussianOEIntWithMax(pairPlayer ? 4.5 : 3.75, .13, .091, 8, pairPlayer ? 3 : 2);

            Log.Out("numPlayers: {0}", numPlayers);

            //This is extremely annoying, but the human has to be red or the game places the player in the red's spot anyways.
            //It can be avoided by generating the map in the map editor first, but that is silly.
            //However, by making this template generator assume red, you can actually pick a random color when you start the game, and be placed in the correct spot.
            //The only downside is it makes it more confusing when reconciling the generated template with the map you played on.
            //Therefore we make the human always player 0, but display a different player as the one you should select.
            int human = 0;
            int manualSelect = rand.Next(numPlayers);
            string humanColor = GetColor(manualSelect);
            name += string.Format(" ({0}, {1})", numPlayers, humanColor);
            Log.Out("human (template, game): {0}, {1}", GetColor(human), humanColor);

            int strongCount = rand.RangeInt(1, numPlayers - (pairPlayer ? 3 : 2));
            if (strongCount < 1)
                strongCount = 1;
            HashSet<int> strongAIs = rand.Iterate(Enumerable.Range(0, numPlayers).Where(id => id != human)).Take(strongCount).ToHashSet();

            int ShiftAI(int ai) => ai <= manualSelect ? ai - 1 : ai;
            List<string> AIOut(Func<int, int> F) => strongAIs.OrderBy(x => x).Select(F).Select(GetColor).ToList();
            Log.Out("strong: {0}, {1}", AIOut(x => x), AIOut(ShiftAI));

            var players = Enumerable.Range(0, numPlayers).Select(id => new Player(human == id, strongAIs.Contains(id))).ToArray();
            if (pairPlayer)
            {
                Player paired = rand.SelectValue(players.Where(p => !p.Human && !p.AIstrong));
                Player.SetPair(players[human], paired);
                Log.Out("paired: {0}, {1}", GetColor(paired.ID), GetColor(ShiftAI(paired.ID)));
            }
            return rand.Iterate(players.Concat(strongAIs.Select(id => new Player(players[id])))).ToArray();
        }
        public static string GetColor(int human)
        {
            return human switch
            {
                0 => "Red",
                1 => "Blue",
                2 => "Tan",
                3 => "Green",
                4 => "Orange",
                5 => "Purple",
                6 => "Teal",
                7 => "Pink",
                _ => throw new Exception(),
            };
        }

        private static int InitSize()
        {
            int sizeInt = SelectSize();
            Log.Out("picked: {0}", sizeInt);
            size = sizeInt;
            //idk why, but the largest map size is off by 1
            if (sizeInt == 98)
                sizeInt = 99;
            return sizeInt;
        }
        private static int SelectSize()
        {
            int[] allSizes = new int[] { 1, 2, 4, 8, 9, 16, 18, 25, 32, 36, 49, 50, 72, 98 };
            int[] surfaceSizes = new int[] { 1, 4, 9, 16, 25, 36, 49 };

            bool all = rand.Bool();//todo: underground
            Log.Out("all sizes: {0}", all);

            int[] sizes = all ? allSizes : surfaceSizes;
            int min = sizes[0], max = sizes[^1];

            double avg = 5.2 * Math.Pow((numPlayers - (pairPlayer ? .5 : 0)) / 1.69, .65);
            Log.Out("avg: {0}", avg);
            int size = GaussianOEIntWithMax(avg, .26, .13, max, min);
            Log.Out("value: {0}", size);

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
