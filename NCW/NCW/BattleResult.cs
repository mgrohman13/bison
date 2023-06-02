using System.Collections.Generic;

namespace NCWMap
{
    internal class BattleResult
    {
        public double AttKill
        {
            get;
            private set;
        }
        public double DefKill
        {
            get;
            private set;
        }
        public double AttDmg
        {
            get;
            private set;
        }
        public double DefDmg
        {
            get;
            private set;
        }

        public BattleResult(int att, int def)
            : this(att, def, false)
        {
        }
        public BattleResult(int att, int def, int attHP, int defHP)
            : this(att, def, false, attHP, defHP)
        {
        }
        public BattleResult(int att, int def, bool divDef)
            : this(att, def, divDef, int.MaxValue, int.MaxValue)
        {
        }
        public BattleResult(int att, int def, bool divDef, int attHP, int defHP)
        {
            if (divDef)
            {
                int halfDef = def / 2;
                Calculate(att, halfDef, attHP, defHP);
                if (halfDef * 2 != def)
                {
                    BattleResult retVal2 = new BattleResult(att, halfDef + 1, attHP, defHP);
                    AttKill = ( AttKill + retVal2.AttKill ) / 2.0;
                    DefKill = ( DefKill + retVal2.DefKill ) / 2.0;
                    AttDmg = ( AttDmg + retVal2.AttDmg ) / 2.0;
                    DefDmg = ( DefDmg + retVal2.DefDmg ) / 2.0;
                }
            }
            else
            {
                Calculate(att, def, attHP, defHP);
            }
        }

        private void Calculate(int att, int def, int attHP, int defHP)
        {
            int[] attArr = GetArray(att);
            int[] defArr = GetArray(def);

            double pos = 0.0;
            double neg = 0.0;
            double kill = 0.0;
            double die = 0.0;
            double tot = 0.0;

            for (int a = 0 ; a < attArr.Length ; a++)
            {
                for (int d = 0 ; d < defArr.Length ; d++)
                {
                    double chance = attArr[a] * defArr[d];
                    tot += chance;

                    int dmg, hp;
                    if (a > d)
                    {
                        dmg = a - d;
                        hp = defHP;
                    }
                    else
                    {
                        dmg = d - a;
                        hp = attHP;
                    }

                    if (dmg / 2.0 > hp)
                        dmg = hp * 2;
                    double addDmg = dmg / 2.0 * chance;
                    double addKill = ( ( dmg / 2 >= hp ? .5 : 0.0 ) + ( ( dmg + 1 ) / 2 >= hp ? .5 : 0.0 ) ) * chance;

                    if (a > d)
                    {
                        pos += addDmg;
                        kill += addKill;
                    }
                    else
                    {
                        neg += addDmg;
                        die += addKill;
                    }
                }
            }

            AttDmg = neg / tot;
            DefDmg = pos / tot;
            AttKill = die / tot;
            DefKill = kill / tot;
        }

        private static Dictionary<int, int[]> Cache = new Dictionary<int, int[]>();
        private static int[] GetArray(int amt)
        {
            int[] cur;
            if (!Cache.TryGetValue(amt, out cur))
            {
                if (amt == 0)
                {
                    cur = new[] { 1 };
                }
                else
                {
                    cur = new[] { 1, 1, 1, 1, 1, 1 };
                    for (int a = 2 ; a <= amt ; a++)
                    {
                        int len = 5 * a + 1;
                        int div = ( len + 1 ) / 2;
                        int[] prev = new int[div];
                        prev[0] = cur[0];
                        for (int b = 1 ; b < div ; b++)
                            prev[b] = prev[b - 1] + cur[b];
                        cur = new int[len];
                        for (int c = 0 ; c < div ; c++)
                        {
                            cur[c] = prev[c];
                            if (c > 5)
                                cur[c] -= prev[c - 6];
                        }
                        for (int d = len ; d > div ; d--)
                            cur[d - 1] = cur[len - d];
                    }
                }
                Cache.Add(amt, cur);
            }
            return cur;
        }
    }
}
