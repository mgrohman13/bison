using MattUtil;
using MattUtil.RealTimeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace testwin
{
    static class Program
    {
        public static MTRandom Random;

        [STAThread]
        static void Main()
        {
            Random = new MTRandom();
            Random.StartTick();

            //Dictionary<int, double>[] result = Logistics.RunCombat(out String[] labels);

            int log = 0;
            const int tests = 9999999;
            var result = new[] { new Dictionary<int, double>() };
            String[] labels = new[] { "CityWar Treasure AddTo Inc" };
            for (int a = 0; a < tests; a++)
            {
                if (Program.Random.Next(tests) < ++log)
                {
                    log = 0;
                    Console.WriteLine($"{a / (float)tests:00.0%}");
                }
                int inc = Game.Random.GaussianOEInt(Game.Random.Range(2.6, 5.2), Game.Random.DoubleHalf(), Game.Random.Weighted(.21));
                result[0].TryGetValue(inc, out double value);
                result[0][inc] = value + 1;
            }
            foreach (int k in result[0].Keys.ToList())
                result[0][k] /= tests;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(labels, result));
        }
    }

}
