using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCWMap
{
    class Bal
    {
        private const int MAX = 100;

        private static Bal.Calculator calculator;

        private static List<Bal.Unit> units;

        public static void getOutput(string input)
        {
            Bal.units = new List<Unit>();
            Bal.calculator = new();
            Bal.parseInput(input);
            string o = Bal.run();
            System.IO.File.WriteAllText("out.txt", o);
        }

        private static string run()
        {
            Dictionary<string, double> unitWorth;
            string[,,] output = getOutput(out unitWorth);

            double[,,] dmgs = new double[2, MAX, MAX];
            foreach (Object element in Bal.units)
            {
                Bal.Unit u1 = (Bal.Unit)element;
                foreach (Bal.Unit u2 in Bal.units)
                {
                    double att = u1.getAttDef(u2)[0];
                    double def = u2.getAttDef(u1)[1];
                    if (def > att)
                    {
                        double temp = att;
                        att = def;
                        def = temp;
                    }

                    int b = (int)Math.Round(2.0 * att);
                    int c = (int)Math.Round(2.0 * def);
                    if (dmgs[0, b, c] == 0.0)
                    {
                        double[] dmg = Bal.calculator.getDamage(att, def);
                        for (int a = 0; a < 2; a++)
                        {
                            dmgs[a, b, c] = dmg[a];
                        }
                    }
                }
            }

            StringBuilder o = new StringBuilder();
            for (int a = 0; a < 6; a++)
            {
                o.Append("\t");
                for (int b = 0; b < Bal.units.Count; b++)
                {
                    o.Append((Bal.units[b]).name);
                    o.Append("\t");
                }
                o.Append("\r\n");

                for (int c = 0; c < Bal.units.Count; c++)
                {
                    o.Append((Bal.units[c]).name);
                    o.Append("\t");
                    for (int b = 0; b < Bal.units.Count; b++)
                    {
                        string cell = output[a, b, c];
                        if (cell != null)
                        {
                            o.Append(cell);
                        }
                        o.Append("\t");
                    }
                    o.Append("\r\n");
                }
                o.Append("\r\n");

                if (a == 1)
                {
                    for (int b = 0; b < Bal.units.Count; b++)
                    {
                        o.Append("\t");
                        o.Append(unitWorth[(Bal.units[b]).name]);
                    }
                    o.Append("\r\n\r\n");
                }
            }

            o.Append("\r\nAtt\tDef\tPos\tNeg\r\n");
            for (int b = 0; b < MAX; b++)
            {
                for (int c = 0; c < MAX; c++)
                {
                    if (dmgs[0, b, c] > 0.0)
                    {
                        o.Append(b / 2.0);
                        o.Append("\t");
                        o.Append(c / 2.0);
                        o.Append("\t");
                        o.Append(dmgs[0, b, c]);
                        o.Append("\t-");
                        o.Append(dmgs[1, b, c]);
                        o.Append("\r\n");
                    }
                }
            }

            return o.ToString();
        }

        private static string[,,] getOutput(out Dictionary<string, Double> unitWorth)
        {
            string[,,] output = new string[6, Bal.units.Count, Bal.units.Count];
            unitWorth = new Dictionary<string, Double>();

            double ttk = 1, ttkc = 0;
            for (int a = 0; a < Bal.units.Count; a++)
                for (int b = 0; b < Bal.units.Count; b++)
                {
                    Bal.Unit u1 = Bal.units[a];
                    Bal.Unit u2 = Bal.units[b];
                    if (u1.type != Bal.Type.A && u2.type == Bal.Type.A && u1.cantAttack(u2))
                    {
                        ttk *= GetTTK(u1, u2, 1);
                        ttkc++;
                    }
                }
            ttk = Math.Pow(ttk, 1 / ttkc);

            for (int c = 0; c < Bal.units.Count; c++)
            {
                Bal.Unit u1 = Bal.units[c];
                double total = 1.0;
                double count = 0.0;
                for (int d = 0; d < Bal.units.Count; d++)
                {
                    Bal.Unit u2 = Bal.units[d];
                    double[] unitOut = u1.getUnitRow(u2);
                    for (int e = 0; e < 6; e++)
                    {
                        if (unitOut[e] != 0.0)
                        {
                            output[e, c, d] = unitOut[e].ToString();
                        }
                    }
                    if (u1.name == u2.name)
                    {
                        continue;
                    }

                    const double singleDiv = 6, airDiv = 3, attkDiv = 3, alDiv = 5, gwDiv = 4;
                    double v1 = log(unitOut[0]);
                    double v2 = log(unitOut[1]);
                    double costMult = u2.cost / (double)u1.cost;
                    double sdMult = (singleDiv - 1) / singleDiv;
                    v1 = Math.Pow(v1, sdMult) * Math.Pow(log(unitOut[2]) * costMult, 1 / singleDiv);
                    v2 = Math.Pow(v2, sdMult) * Math.Pow(log(unitOut[3]) * costMult, 1 / singleDiv);
                    double value = v1;
                    double mult = 1;
                    if (u1.type != u2.type)
                    {
                        if (u1.type == Bal.Type.A || u2.type == Bal.Type.A)
                        {
                            if (u1.cantAttack(u2) || u2.cantAttack(u1))
                            {
                                mult = 1 / airDiv + 1 / attkDiv + 1 / alDiv;
                                value = Math.Pow(value, 1 / airDiv / mult) * Math.Pow(log(GetTTK(u1, u2, ttk)), 1 / attkDiv / mult) * Math.Pow(v2, 1 / alDiv / mult);
                            }
                        }
                        else
                        {
                            mult /= gwDiv;
                        }
                    }

                    total *= Math.Pow(value, mult);
                    count += mult / 2.0;
                }
                unitWorth.Add(u1.name, Math.Pow(total, 1.0 / count));
            }

            return output;
        }
        private static double GetTTK(Unit u1, Unit u2, double div)
        {
            if (u2.cantAttack(u1))
                return 1 / GetTTK(u2, u1, div);
            return (u1.hits * 6) / (u2.move * u2.getUnitRow(u1)[4]) / div * u2.cost / (double)u1.cost;
        }
        private static double log(double v)
        {
            bool flip = v < 1;
            if (flip) v = 1 / v;
            v = 1 + Math.Log(v);
            if (flip) v = 1 / v;
            return v;
        }

        private static void parseInput(string input)
        {
            foreach (string r in input.Split(new String[] { "\r\n" }, StringSplitOptions.None))
            {
                string unitRow = r;
                if (!unitRow.StartsWith("Cost\tUnit\tType\tAtt\tDef\tHP\tHeal\tMove\tSpecial"))
                {
                    unitRow = unitRow.Trim();
                    if (unitRow.Length == 0)
                    {
                        break;
                    }
                    Bal.units.Add(new Bal.Unit(unitRow));
                }
            }
        }

        private class Calculator
        {

            private Dictionary<int, double[]> cache;

            public Calculator()
            {
                this.cache = new Dictionary<int, double[]>();
            }

            public double[] getDamage(double attDouble, double defDouble)
            {
                int att = (int)attDouble;
                int def = (int)defDouble;
                double attRmdr = attDouble % 1.0;
                double defRmdr = defDouble % 1.0;
                double[] retVal = new double[2];
                double count = 0.0;
                do
                {
                    count += 1.0;
                    double[] values = get(att, def);
                    retVal[0] += values[0];
                    retVal[1] += values[1];
                } while (((defRmdr != 0.0) && (def++ == (int)defDouble))
                        || ((attRmdr != 0.0) && (att++ == (int)attDouble)));
                retVal[0] /= count;
                retVal[1] /= count;
                return retVal;
            }

            private double[] get(int att, int def)
            {
                if (att == 0)
                {
                    return new double[] { 0.0, def * 1.25 };
                }
                if (def == 0)
                {
                    return new double[] { att * 1.25, 0.0 };
                }

                int attDef = att * MAX + def;
                double[] retVal;

                if (!this.cache.TryGetValue(attDef, out retVal))
                {
                    retVal = calculate(att, def);

                    this.cache.Add(attDef, retVal);
                    int k2 = def * MAX + att;
                    if (!this.cache.ContainsKey(k2))
                        this.cache.Add(k2, new double[] { retVal[1], retVal[0] });
                }
                return retVal;
            }

            private double[] calculate(int att, int def)
            {
                int[] attArr = getArr(att);
                int[] defArr = getArr(def);

                double pos = 0.0;
                double neg = 0.0;
                double tot = 0.0;
                for (int a = 0; a < attArr.Length; a++)
                {
                    for (int d = 0; d < defArr.Length; d++)
                    {
                        double chance = attArr[a] * defArr[d];
                        tot += chance;
                        if (a > d)
                        {
                            pos += chance * (a - d) / 2.0;
                        }
                        else
                        {
                            neg += chance * (d - a) / 2.0;
                        }
                    }
                }

                return new double[] { pos / tot, neg / tot };
            }

            private int[] getArr(int amt)
            {
                int[] cur = { 1, 1, 1, 1, 1, 1 };
                for (int a = 2; a <= amt; a++)
                {
                    int len = 5 * a + 1;
                    int div = (int)Math.Floor((len + 1) / 2.0);
                    int[] prev = new int[div];
                    prev[0] = cur[0];
                    for (int b = 1; b < div; b++)
                    {
                        prev[b] = (prev[(b - 1)] + cur[b]);
                    }
                    cur = new int[len];
                    for (int c = 0; c < div; c++)
                    {
                        cur[c] = prev[c];
                        if (c > 5)
                        {
                            cur[c] -= prev[(c - 6)];
                        }
                    }
                    for (int d = len; d > div; d--)
                    {
                        cur[(d - 1)] = cur[(len - d)];
                    }
                }
                return cur;
            }

        }

        private enum Type
        {
            A, G, W,
        }

        private class Unit
        {

            public readonly string name;
            public readonly Bal.Type type;
            public readonly int cost;
            public readonly int attack;
            public readonly int defense;
            public readonly int hits;
            public readonly int move;

            private readonly Bal.Unit.Special special;

            public Unit(string input)
            {
                string[] unitRow = input.Split('\t');
                if (unitRow.Length < 9)
                {
                    throw new Exception(unitRow.Length.ToString());
                }

                this.cost = int.Parse(unitRow[0].Trim());
                this.name = unitRow[1].Trim();
                this.type = (Bal.Type)Enum.Parse(typeof(Bal.Type), unitRow[2].Trim());
                this.attack = int.Parse(unitRow[3].Trim());
                this.defense = int.Parse(unitRow[4].Trim());
                this.hits = int.Parse(unitRow[5].Trim());
                this.move = int.Parse(unitRow[7].Trim());

                string special = unitRow[8].Trim();
                if (special.Length > 0)
                {
                    this.special = new Bal.Unit.Special(special);
                }
            }

            public double[] getUnitRow(Unit other)
            {
                double[] retVal = new double[6];
                double[] attDefT = getAttDef(other);
                double[] attDefO = other.getAttDef(this);
                double[] attDmg = Bal.calculator.getDamage(attDefT[0], attDefO[1]);

                if (this.name != other.name)
                {
                    double[] defDmg = Bal.calculator.getDamage(attDefO[0], attDefT[1]);
                    int mult = lcm(this.cost, other.cost);
                    int numT = mult / this.cost;
                    int numO = mult / other.cost;

                    double[] multVals = getVals(other, attDmg, defDmg, numT, numO);
                    retVal[0] = multVals[0];
                    retVal[1] = multVals[1];

                    double[] singleVals = getVals(other, attDmg, defDmg, 1, 1);
                    retVal[2] = singleVals[0];
                    retVal[3] = singleVals[1];
                }

                retVal[4] = attDmg[0];
                retVal[5] = (-attDmg[1]);
                return retVal;
            }

            public double[] getAttDef(Unit other)
            {
                double[] attDef = new double[2];
                attDef[0] = this.attack;
                attDef[1] = this.defense;

                if ((this.special != null) && (this.special.targets(other)))
                {
                    attDef[0] += this.special.attBonus;
                    attDef[1] += this.special.defBonus;
                }
                if ((other.special != null) && (other.special.targets(this)))
                {
                    attDef[0] /= other.special.attDiv;
                    attDef[1] /= other.special.defDiv;
                }

                return attDef;
            }

            private int lcm(int a, int b)
            {
                int n = Math.Max(a, b);
                while ((n % a != 0) || (n % b != 0))
                {
                    n++;
                }
                return n;
            }

            private double[] getVals(Unit other, double[] attDmg, double[] defDmg, int numT, int numO)
            {
                double value = calcWorth(other, attDmg[0], defDmg[1], defDmg[0], attDmg[1], numT, numO);

                double onlyT = calcWorth(other, attDmg[0], 0.0, 0.0, attDmg[1], numT, numO);
                double onlyO = calcWorth(other, 0.0, defDmg[1], defDmg[0], 0.0, numT, numO);
                bool tAttacks = onlyO <= value;
                bool oAttacks = onlyT >= value;
                if (!tAttacks)
                {
                    if (oAttacks)
                    {
                        value = onlyO;
                    }
                    else
                    {
                        value = this.cost * numT / (double)(other.cost * numO);
                        if (onlyT > value)
                            value = onlyT;
                        else if (onlyO < value)
                            value = onlyO;
                    }
                }
                else if (!oAttacks)
                {
                    value = onlyT;
                }

                double[] retVal = new double[2];
                if (cantAttack(other))
                {
                    retVal[0] = onlyO;
                    retVal[1] = Math.Max(value, onlyO);
                }
                else if (other.cantAttack(this))
                {
                    retVal[0] = onlyT;
                    retVal[1] = Math.Min(value, onlyT);
                }
                else
                {
                    retVal[0] = value;
                }
                return retVal;
            }

            private double calcWorth(Unit other, double att, double negDef, double def, double negAtt, int startT,
                    int startO)
            {
                double hitT = this.hits;
                double hitO = other.hits;
                int numT = startT;
                int numO = startO;
                while ((numT > 0) && (numO > 0))
                {
                    double totAtt = att * numT * this.move + negDef * numO * other.move - other.getRegen();
                    double totDef = def * numO * other.move + negAtt * numT * this.move - getRegen();
                    if (totAtt < 0.0)
                    {
                        return 0.0;
                    }
                    if (totDef < 0.0)
                    {
                        return (1.0 / 0.0);
                    }

                    double timeT = hitT / totDef;
                    double timeO = hitO / totAtt;
                    if (timeT > timeO)
                    {
                        numO--;
                        hitO = other.hits;
                        hitT -= timeO * totDef;
                    }
                    else if (timeT < timeO)
                    {
                        numT--;
                        hitT = this.hits;
                        hitO -= timeT * totAtt;
                    }
                    else
                    {
                        numT--;
                        numO--;
                        hitT = this.hits;
                        hitO = other.hits;
                    }
                }

                double retVal;
                if (numT > 0)
                {
                    retVal = this.hits * startT / (this.hits * startT - ((numT - 1) * this.hits + hitT));
                }
                else
                {
                    retVal = (other.hits * startO - ((numO - 1) * other.hits + hitO)) / (other.hits * startO);
                }
                return retVal;
            }

            private double getRegen()
            {
                if (this.special != null)
                {
                    return this.special.regen;
                }
                return 0.0;
            }

            public bool cantAttack(Unit other)
            {
                return (this.type != Bal.Type.A) && (other.type == Bal.Type.A)
                        && ((this.special == null) || (!this.special.attackAir));
            }

            public override string ToString()
            {
                return name;
            }

            private class Special
            {

                private const string _ATTACK_AIR = "can attack A";
                private const string _PLUS = "+";
                private const string _MINUS = "-";
                private const string _REGEN_S = "regens ";
                private const string _REGEN_E = " HP";
                private const string _VS = " vs ";
                private const string _HAVE = " have ";
                private const string _SPACE = " ";
                private const string _DIV = "/";
                private const string _COMMA = ",";
                private const string _ATTACK = "Att";
                private const string _DEFENSE = "Def";
                private const string _BEASTS = "beasts";
                private static string[] _BEASTS_LIST = { "Hydra", "Dragon", "Wyrm", "Elemental" };

                public bool attackAir = false;
                public int regen = 0;
                public int attBonus = 0;
                public int defBonus = 0;
                public int attDiv = 1;
                public int defDiv = 1;

                private HashSet<string> targUnits = null;
                private HashSet<Bal.Type> targTypes = null;

                public Special(string input)
                {
                    if (_ATTACK_AIR == input)
                    {
                        this.attackAir = true;
                    }
                    else if ((input.StartsWith(_REGEN_S)) && (input.EndsWith(_REGEN_E)))
                    {
                        this.regen = Parse(_PLUS,
                                input.Substring(_REGEN_S.Length, input.Length - _REGEN_E.Length - _REGEN_S.Length));
                    }
                    else
                    {
                        if (input.Contains(_VS))
                        {
                            string[] p1 = input.Split(new String[] { _VS }, StringSplitOptions.None);
                            string[] p2 = p1[0].Split(new String[] { _SPACE }, StringSplitOptions.None);

                            bool[] attDef = parseAttDef(p2[1]);
                            bool neg = input.Contains(_MINUS);
                            int value = Parse(neg ? _MINUS : _PLUS, p2[0]) * (neg ? -1 : 1);
                            if (attDef[0])
                                this.attBonus = value;
                            if (attDef[1])
                                this.defBonus = value;

                            parseTargets(p1[1]);
                        }
                        else if (input.Contains(_HAVE))
                        {
                            string[] p1 = input.Split(new String[] { _HAVE }, StringSplitOptions.None);
                            string[] p2 = p1[1].Split(new String[] { _DIV }, StringSplitOptions.None);

                            bool[] attDef = parseAttDef(p2[0]);
                            int value = Parse(null, p2[1]);
                            if (value != 2)
                            {
                                throw new Exception(value.ToString());
                            }
                            if (attDef[0])
                            {
                                this.attDiv = value;
                            }
                            if (attDef[1])
                            {
                                this.defDiv = value;
                            }

                            parseTargets(p1[0]);
                        }
                        else
                        {
                            throw new Exception(input);
                        }
                    }

                    if (this.targTypes != null && this.targTypes.Contains(Bal.Type.A) && (attBonus > 0 || defDiv > 1))
                    {
                        this.attackAir = true;
                    }
                }

                private int Parse(string begins, string input)
                {
                    if ((begins != null) && (!input.StartsWith(begins)))
                    {
                        throw new Exception(begins + "   " + input);
                    }
                    if (begins != null)
                    {
                        input = input.Substring(begins.Length);
                    }
                    return int.Parse(input);
                }

                private bool[] parseAttDef(string input)
                {
                    bool[] retVal = new bool[2];
                    foreach (string p in input.Split(new String[] { _COMMA }, StringSplitOptions.None))
                    {
                        if (_ATTACK == p)
                        {
                            if (retVal[0])
                            {
                                throw new Exception(input);
                            }
                            retVal[0] = true;
                        }
                        else if (_DEFENSE == p)
                        {
                            if (retVal[1])
                            {
                                throw new Exception(input);
                            }
                            retVal[1] = true;
                        }
                        else
                        {
                            throw new Exception(input);
                        }
                    }
                    return retVal;
                }

                private void parseTargets(string input)
                {
                    if (_BEASTS == input)
                    {
                        foreach (string beast in _BEASTS_LIST)
                        {
                            TargUnits().Add(beast);
                        }
                    }
                    else
                    {
                        foreach (string target in input.Split(new String[] { _COMMA }, StringSplitOptions.None))
                        {
                            Bal.Type type;
                            if (Enum.TryParse(target, out type))
                            {
                                TargTypes().Add(type);
                            }
                            else
                            {
                                TargUnits().Add(target);
                            }
                        }
                    }
                }

                private HashSet<string> TargUnits()
                {
                    if (this.targUnits == null)
                    {
                        this.targUnits = new HashSet<string>();
                    }
                    return this.targUnits;
                }

                private HashSet<Bal.Type> TargTypes()
                {
                    if (this.targTypes == null)
                    {
                        this.targTypes = new HashSet<Bal.Type>();
                    }
                    return this.targTypes;
                }

                public bool targets(Bal.Unit other)
                {
                    return ((this.targUnits != null) && (this.targUnits.Contains(other.name)))
                            || ((this.targTypes != null) && (this.targTypes.Contains(other.type)));
                }

            }

        }

    }


}

