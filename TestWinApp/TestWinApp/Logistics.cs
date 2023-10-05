using System;
using System.Collections.Generic;
using System.Linq;

namespace testwin
{
    internal class Logistics
    {
        internal static Dictionary<int, double>[] RunCombat(out String[] labels)
        {
            const int tests = 99999999;

            const int triggers = 6, bonuses = 5;
            const int maxes = 5;
            const int dice = 2;

            Dictionary<int, long>[,] rolls = new Dictionary<int, long>[bonuses + 1, triggers];

            double[] avgBonuses = new double[bonuses + 1], countBonuses = new double[bonuses + 1];
            double[,,] avgMaxes = new double[bonuses + 1, maxes - 1, dice], countMaxes = new double[bonuses + 1, maxes - 1, dice];
            double[,] avgDice = new double[bonuses + 1, dice - 1], countDice = new double[bonuses + 1, dice - 1];


            List<Combat> simulations = new();
            for (int bonus = 0; bonus <= bonuses; bonus++)
                for (int trigger = 1; trigger <= triggers; trigger++)
                {
                    rolls[bonus, trigger - 1] = new();
                    for (int die = 1; die <= dice; die++)
                        simulations.Add(new Combat(die, trigger, bonus));
                }

            int log = 0;
            for (int a = 0; a < tests; a++)
            {
                if (Program.Random.Next(tests) < ++log)
                {
                    log = 0;
                    Console.WriteLine($"{a / (float)tests:00.0%}");
                }

                foreach (var sim in simulations)
                {

                    int r1 = sim.Run();
                    int b = sim.bonus, c = sim.trigger - 1;

                    if (sim.dice > 1)
                    {
                        int d = sim.dice - 2;
                        avgDice[b, d] += r1;
                        countDice[b, d]++;
                    }
                    else
                    {
                        if (b <= bonuses && c < triggers)
                            AddCount(rolls[b, c], r1);
                        if (b < avgBonuses.Length)
                        {
                            avgBonuses[b] += r1;
                            countBonuses[b]++;
                        }
                    }

                    int max = r1;
                    for (int d = 1; d < maxes; d++)
                    {
                        int r2 = sim.Run();
                        max = Math.Max(max, r2);
                        avgMaxes[b, d - 1, sim.dice - 1] += max;
                        countMaxes[b, d - 1, sim.dice - 1]++;
                    }
                }
            }
            Console.WriteLine();

            labels = new String[(bonuses + 1) * triggers];
            Dictionary<int, double>[] dicts = new Dictionary<int, double>[(bonuses + 1) * triggers];
            int idx = 0;
            for (int bonus = 0; bonus <= bonuses; bonus++)
                for (int trigger = 1; trigger <= triggers; trigger++)
                {
                    labels[idx] = $"+{bonus} ({trigger})";
                    Dictionary<int, long> dict = rolls[bonus, trigger - 1];
                    double total = dict.Values.Sum() / 100;
                    dicts[idx] = dict.ToDictionary(p => p.Key, p => p.Value / (double)total);
                    idx++;
                }

            for (int a = 0; a < avgBonuses.Length; a++)
                Console.WriteLine($"+{a}: {(float)(avgBonuses[a] / countBonuses[a])}");
            Console.WriteLine();
            for (int d = 1; d <= dice; d++)
            {
                for (int b = 0; b <= bonuses; b++)
                {
                    for (int c = 1; c < maxes; c++)
                        Console.WriteLine($"+{b} ({d} dice) (max of {c + 1:00}): {(float)(avgMaxes[b, c - 1, d - 1] / countMaxes[b, c - 1, d - 1])}");
                    Console.WriteLine();
                }
            }
            for (int d = 2; d <= dice; d++)
            {
                for (int e = 0; e <= bonuses; e++)
                    Console.WriteLine($"+{e} ({d} dice): {(float)(avgDice[e, d - 2] / countDice[e, d - 2])}");
                Console.WriteLine();
            }

            return dicts;
        }

        internal static Dictionary<int, double>[] RunPlayers(out String[] labels)
        {
            const int tests = 999999;

            Dictionary<int, long> u = new(), s = new();
            Dictionary<int, Tuple<double, double>> t = new();
            double corSum = 0, corCount = 0, deCorSum = 0, deCorCount = 0;
            static void AddAvg(Dictionary<int, Tuple<double, double>> d, int k, double v)
            {
                if (!d.TryGetValue(k, out Tuple<double, double> t))
                    t = new(0, 0);
                d[k] = new(t.Item1 + v, t.Item2 + 1);
            }
            void AddCorrelation(int u, int s, bool c)
            {
                if (c)
                {
                    corSum += u + s;
                    corCount++;
                }
                else
                {
                    deCorSum += u + s;
                    deCorCount++;
                }
            }

            bool loop = true;
            for (int turns = 1; loop; turns = Program.Random.RangeInt(turns + 1, turns * 2))
            {
                loop = turns < 1300;
                int amt = Program.Random.Round(tests / turns);

                List<Player> simulations = new();
                List<List<PlayerValues>> result = new();

                for (int a = 0; a < amt; a++)
                    simulations.Add(new Player());

                for (int b = 0; b < turns; b++)
                {
                    result.Add(new List<PlayerValues>());
                    foreach (Player simulation in simulations)
                    {
                        result[b].Add(simulation.Run(out int u1, out int s1, out bool c1, out int u2, out int s2, out bool c2));
                        AddCount(u, u1);
                        AddCount(u, u2);
                        AddCount(s, s1);
                        AddCount(s, s2);
                        AddCorrelation(u1, s1, c1);
                        AddCorrelation(u2, s2, c2);
                    }
                }

                double total = 0, supply = 0, final = 0, order = 0;
                for (int c = 0; c < amt; c++)
                    for (int d = 0; d < turns; d++)
                    {
                        PlayerValues v = result[d][c];
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
            static void Flatten(Dictionary<int, long> o, Dictionary<int, double> n)
            {
                double sum = o.Values.Sum();
                foreach (var pair in o)
                {
                    double pct = pair.Value / sum;
                    n.Add(pair.Key, 100 * pct);
                    Console.WriteLine($"{pair.Key}\t{pct}");
                }
                Console.WriteLine();
            }

            corSum /= corCount;
            deCorSum /= deCorCount;
            Console.WriteLine($"correlated: {corSum}");
            Console.WriteLine($"decorrelated: {deCorSum}");
            Console.WriteLine($"ratio: {corSum / deCorSum}");
            Console.WriteLine();

            labels = new[] { "Units", "Supply", "Total/Turn" };
            return new[] { incUnit, turnTotals, incSupply };
        }

        static void AddCount(Dictionary<int, long> d, int v)
        {
            d.TryGetValue(v, out long c);
            d[v] = c + 1;
        }


        class Combat
        {
            public readonly int dice, trigger, bonus;
            public Combat(int dice, int trigger, int bonus)
            {
                this.dice = dice;
                this.trigger = trigger;
                this.bonus = bonus;
            }
            public int Run()
            {
                int sum = 0, dice = this.dice;

                bool trigger;
                do
                {
                    trigger = false;
                    for (int a = 0; a < dice; a++)
                    {
                        int r = Roll();
                        if (r == this.trigger && !trigger)
                            trigger = true;
                        else
                            sum += r + Math.Min(this.bonus, r - 1);
                    }
                    dice = 2;
                } while (trigger);

                return sum;
            }

            private int Roll()
            {
                return Program.Random.RangeInt(1, 6);
            }
        }

        class Player
        {
            private readonly PlayerValues current = new();
            public PlayerValues Run(out int u1, out int s1, out bool c1, out int u2, out int s2, out bool c2)
            {
                current.CombatRound();
                current.BothRounds(out u1, out s1, out c1);
                current.BothRounds(out u2, out s2, out c2);
                return new(current);
            }
        }

        class PlayerValues
        {
            public const int LOSS_AMT = 20;
            public int Units = 13, Supply = 2, Points = 0, Total = 0, Correlation = 0;

            public PlayerValues() { }
            public PlayerValues(PlayerValues v)
            {
                this.Units = v.Units;
                this.Supply = v.Supply;
                this.Points = v.Points;
                this.Total = v.Total;
                this.Correlation = v.Correlation;
            }

            public void BothRounds(out int units, out int supply, out bool correlation)
            {
                AddUnits(out units, out supply, out correlation);
                LoseUnits();
                Points++;
            }

            private void AddUnits(out int units, out int supply, out bool correlation)
            {
                correlation = false;

                int roll;
                units = supply = 0;
                while ((roll = Roll()) != 1)
                {
                    if (roll == 2)
                    {
                        correlation = true;
                    }
                    else if (roll == 6)
                    {
                        int r2 = Roll();
                        if (r2 == 6)
                            supply++;
                        else if (r2 > 2)
                            units += 3;
                    }
                    else
                    {
                        units++;
                    }
                }

                units = Program.Random.Round(units / 3.0);

                Units += units;
                Supply += supply;
                Total += units + supply;

                if (correlation)
                    Correlation++;
            }

            public void CombatRound()
            {
                int group = Roll() + Roll();
                int count = Units + Supply - group;
                for (int a = 1; count > 0; a++)
                {
                    int next = Math.Min(count, group * a);
                    count -= next;

                    Points += Program.Random.Round(next / (float)a);
                }
            }

            internal void LoseUnits()
            {
                int total = Points / LOSS_AMT;
                if (total > 0)
                {
                    int loss;

                    int diff = Math.Max(Units, Supply) - Math.Min(Units, Supply);
                    if (diff > 0)
                    {
                        loss = Math.Min(total, diff);
                        if (Units > Supply)
                            Units -= loss;
                        else
                            Supply -= loss;
                        total -= loss;
                    }

                    if (total > 0)
                    {
                        loss = Program.Random.Round(total / 2.0);
                        Units -= loss;
                        Supply -= total - loss;
                    }

                    Points %= LOSS_AMT;
                }
            }

            private int Roll()
            {
                return Program.Random.RangeInt(1, 6);
            }

            public override string ToString()
            {
                return $"{Units}\t{Supply}\t{Points} ({Total})";
            }
        }
    }
}
