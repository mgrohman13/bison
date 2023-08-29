using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace testwin
{
    static class Program
    {
        public static MTRandom rand;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            rand = new MTRandom();
            rand.StartTick();

            Dictionary<int, int> u = new(), s = new();
            Dictionary<int, Tuple<double, double>> t = new();
            //incUnit = new(); incSupply = new(); t = new();
            static void AddCount(Dictionary<int, int> d, int v)
            {
                d.TryGetValue(v, out int c);
                d[v] = c + 1;
            }
            static void AddAvg(Dictionary<int, Tuple<double, double>> d, int k, double v)
            {
                if (!d.TryGetValue(k, out Tuple<double, double> t))
                    t = new(0, 0);
                d[k] = new(t.Item1 + v, t.Item2 + 1);
            }

            const int sim = 1000000;
            for (int turns = 1; turns <= 2600; turns = rand.RangeInt(turns + 1, turns * 2))
            {
                int amt = rand.Round(sim / turns);

                List<Simulation> simulations = new();
                List<List<Values>> result = new();

                for (int a = 0; a < amt; a++)
                    simulations.Add(new Simulation());

                for (int b = 0; b < turns; b++)
                {
                    result.Add(new List<Values>());
                    foreach (Simulation simulation in simulations)
                    {
                        result[b].Add(simulation.Run(out int u1, out int s1, out int u2, out int s2));
                        AddCount(u, u1);
                        AddCount(u, u2);
                        AddCount(s, s1);
                        AddCount(s, s2);
                    }
                }

                double total = 0, supply = 0, final = 0, order = 0;
                for (int c = 0; c < amt; c++)
                    for (int d = 0; d < turns; d++)
                    {
                        Values v = result[d][c];
                        AddAvg(t, d, v.Units + v.Supply);
                        if (d == turns - 1)
                        {
                            total += v.Total;
                            supply += v.Supply - 2;

                            final += v.Units + v.Supply - 15;

                            order += v.Correlation;
                        }
                    }
                total /= turns * amt;
                supply /= turns * amt;
                //;// * 2;
                double loss = total - (final / (turns * amt));
                final /= amt;
                final += 15;
                order /= amt;
                Console.WriteLine($"turns {turns}");
                Console.WriteLine($"supply {supply}");
                Console.WriteLine($"total {total}");
                Console.WriteLine($"final {final}");
                Console.WriteLine($"loss {loss} ({loss / total:00%})");
                Console.WriteLine($"order {order} ({order / turns:00%})");
                Console.WriteLine();
            }

            Dictionary<int, double> incUnit = new(), turnTotals = new(), incSupply = new();
            foreach (var pair in t)
                turnTotals.Add(pair.Key, pair.Value.Item1 / pair.Value.Item2);
            Flatten(u, incUnit);
            Flatten(s, incSupply);
            static void Flatten(Dictionary<int, int> o, Dictionary<int, double> n)
            {
                double sum = o.Values.Sum();
                foreach (var pair in o)
                    n.Add(pair.Key, 100 * pair.Value / sum);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(incUnit, turnTotals, incSupply));
        }
    }

    class Simulation
    {
        private readonly Values current = new();
        public Values Run(out int u1, out int s1, out int u2, out int s2)
        {
            current.CalcPoints();
            current.LoseUnits();
            current.AddUnits(out u1, out s1);
            current.AddUnits(out u2, out s2);
            return new(current);
        }
    }

    class Values
    {
        public const int LOSS_AMT = 20;
        public int Units = 13, Supply = 2, Points = 0, Total = 0, Correlation = 0;

        public Values() { }
        public Values(Values v)
        {
            this.Units = v.Units;
            this.Supply = v.Supply;
            this.Points = v.Points;
            this.Total = v.Total;
            this.Correlation = v.Correlation;
        }

        public void AddUnits(out int units, out int supply)
        {
            bool correlation = false;

            int roll;
            units = supply = 0;
            while ((roll = Roll()) != 1)
            {
                correlation |= roll == 4;

                if (roll == 6)
                {
                    int r2 = Roll();
                    if (r2 == 6)
                        supply++;
                    else if (r2 == 5)
                        units++;
                }
                else
                {
                    units++;
                }
            }

            units = Program.rand.Round(units / 3.0);

            Units += units;
            Supply += supply;
            Total += units + supply;

            if (correlation)
                Correlation++;
        }

        public void CalcPoints()
        {
            int group = Roll() + Roll();
            int count = Units + Supply - group;
            for (int a = 1; count > 0; a++)
            {
                int next = Math.Min(count, group * a);
                count -= next;

                Points += Program.rand.Round(next / (float)a);
            }
        }

        internal void LoseUnits()
        {
            Units -= Points / LOSS_AMT;
            Points %= LOSS_AMT;
            //if (Units < 0)
            //{
            //    Supply += Units;
            //    Units = 0;
            //}
        }

        private int Roll()
        {
            return Program.rand.RangeInt(1, 6);
        }

        public override string ToString()
        {
            return $"{Units}\t{Supply}\t{Points} ({Total})";
        }
    }

}
