using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CityWar;

namespace balance
{
    public partial class Units : Form
    {
        public const double minError = 3000;

        int[] Xs = { 12, 78, 114, 180, 246, 312, 348, 384, 420, 595, 770, 945 };
        int[] Ws = { 60, 30, 60, 60, 60, 30, 30, 30, 169, 169, 169 };

        public bool rebalanceAll = true;
        public UnitSchema us;
        public Units()
        {
            InitializeComponent();

            us = new UnitSchema();
        }

        public void getAll()
        {
            this.Controls.Clear();
            InitializeComponent();

            int y = 25, count = us.Unit.Count;
            foreach (UnitSchema.UnitRow r in us.Unit)
            {
                if (--count > 20)
                    continue;
                newBox(Xs[0], y, Ws[0], r.Name);
                newBox(Xs[1], y, Ws[1], r.Cost.ToString());
                newBox(Xs[4], y, Ws[4], r.Hits.ToString());
                newBox(Xs[5], y, Ws[5], r.Armor.ToString());
                newBox(Xs[6], y, Ws[6], r.Regen.ToString());
                newBox(Xs[7], y, Ws[7], r.Move.ToString());

                CostType costType;
                if (r.CostType == "A")
                    costType = CostType.Air;
                else if (r.CostType == "D")
                    costType = CostType.Death;
                else if (r.CostType == "E")
                    costType = CostType.Earth;
                else if (r.CostType == "N")
                    costType = CostType.Nature;
                else if (r.CostType == "W")
                    costType = CostType.Water;
                else
                    costType = CostType.Production;

                UnitType type;
                if (r.Type == "A")
                    type = UnitType.Air;
                else if (r.Type == "GWA")
                    type = UnitType.All;
                else if (r.Type == "GW")
                    type = UnitType.Amphibious;
                else if (r.Type == "G")
                    type = UnitType.Ground;
                else if (r.Type == "W")
                    type = UnitType.Water;
                else
                    throw new Exception();

                newBox(Xs[2], y, Ws[2], costType.ToString());
                newBox(Xs[3], y, Ws[3], type.ToString());

                int x = 8;
                foreach (UnitSchema.AttackRow ar in r.GetAttackRows())
                {
                    newBox(Xs[x], y, Ws[x], Attack.GetString(ar.Name, ar.Damage, ar.Divide_By, ar.Target_Type, ar.Length));
                    ++x;
                }

                newButton(Xs[Xs.Length - 1], y, "delete",
                    r.Name, new EventHandler(b_Click), true);

                y += 29;
            }

            newButton(( this.ClientSize.Width ) / 2 + 6, y,
           "Read", "", new EventHandler(read_Click), true);
            newButton(( this.ClientSize.Width ) / 2 - new Button().Width - 6, y,
                rebalanceAll ? "Balance" : "Write", "", new EventHandler(write_Click), true);

            this.Text = "units - " + us.Unit.Count;
        }

        private void BuildSummon_Load(object sender, EventArgs e)
        {
            getAll();
        }

        void write_Click(object sender, EventArgs e)
        {
            if (rebalanceAll)
            {
                RebalanceAll();
            }
            else
            {
                us.WriteXml("..\\..\\..\\Units.xml");
                MessageBox.Show("written");
            }
        }

        private void RebalanceAll()
        {
            if (us.Unit.Count > 0)
            {
                int count = us.Unit.Count;
                BalUnit[] units = new BalUnit[count];
                foreach (UnitSchema.UnitRow row in Game.Random.Iterate(us.Unit))
                {
                    Abilities ability;
                    if (row.Special == "AircraftCarrier")
                        ability = Abilities.AircraftCarrier;
                    else if (row.Special == "Aircraft")
                        ability = Abilities.Aircraft;
                    else
                        ability = Abilities.None;
                    CityWar.UnitType type;
                    if (row.Type == "A")
                        type = CityWar.UnitType.Air;
                    else if (row.Type == "GWA")
                        type = CityWar.UnitType.Immobile;
                    else if (row.Type == "GW")
                        type = CityWar.UnitType.Amphibious;
                    else if (row.Type == "G")
                        type = CityWar.UnitType.Ground;
                    else if (row.Type == "W")
                        type = CityWar.UnitType.Water;
                    else
                        throw new Exception();
                    UnitSchema.AttackRow[] attackRows = row.GetAttackRows();
                    int numAttacks = attackRows.Length;
                    Attack[] attacks = new Attack[numAttacks];
                    for (int i = 0 ; i < numAttacks ; ++i)
                    {
                        UnitSchema.AttackRow attackRow = attackRows[i];

                        MattUtil.EnumFlags<TargetType> targets = new MattUtil.EnumFlags<TargetType>();

                        if (attackRow.Target_Type.Contains("G"))
                            targets.Add(TargetType.Ground);
                        if (attackRow.Target_Type.Contains("W"))
                            targets.Add(TargetType.Water);
                        if (attackRow.Target_Type.Contains("A"))
                            targets.Add(TargetType.Air);

                        attacks[i] = new Attack(targets, attackRow.Length, attackRow.Damage, attackRow.Divide_By);
                    }
                    units[--count] = new BalUnit(row.Move, row.Regen, ability, row.Armor, type, attacks, row.IsThree, row.Hits, row.Name);
                }

                bool balanced = false, accepted = false, randing = false;
                int ignore = -1;
                double costMult = 1, low = -1, high = -1;
                while (!balanced)
                {
                    if (accepted)
                    {
                        if (high == -1)
                            //we are searching for a lower bound outside of the balanced range
                            costMult = low;
                        else if (randing)
                            //try to pick a uniform random mult from within the balanced range
                            costMult = Game.Random.Range(low, high);
                        else
                            //we are searching for an upper bound outside of the balanced range
                            costMult = high;
                    }

                    //for debugging...
                    Console.WriteLine(costMult);

                    int firstCost = -1;
                    double temp = 0;

                    balanced = true;
                    foreach (BalUnit unit in units)
                    {
                        double cost = CityWar.Balance.getCost(unit.MaxMove, unit.BaseRegen, unit.Ability, unit.BaseArmor, unit.Type, unit.Attacks, unit.isThree, unit.maxHits, costMult);
                        double ActCost = Math.Round(cost);
                        //inverse percent error
                        double invPctErr = ActCost / Math.Abs(cost - ActCost);

                        if (firstCost == -1)
                            firstCost = (int)ActCost;
                        if (accepted && temp == 0)
                            if (high == -1)
                                //intial upper bound guess
                                temp = costMult * ( ActCost + ActCost / minError ) / cost + 1 / minError;
                            else if (randing && invPctErr <= minError)
                                //whether we are too high or too low
                                temp = cost - ActCost;

                        //check that the rounding is within the acceptable percent error
                        if (ignore == firstCost || invPctErr <= minError)
                        {
                            balanced = false;
                            if (!accepted)
                            {
                                //immediately step to near the next whole-integer cost
                                costMult *= ( ( (int)cost + 1 ) - ( (int)cost + 1 ) / minError ) / cost;
                                costMult += Game.Random.DoubleHalf(1 / minError);
                            }
                            break;
                        }
                    }

                    if (accepted)
                    {
                        if (high == -1)
                        {
                            if (balanced)
                            {
                                //find a lower bound that is outside of the balanced range
                                balanced = false;
                                low -= 1 / minError;
                            }
                            else
                            {
                                //set an intial upper bound guess
                                high = temp;
                            }
                        }
                        else if (randing)
                        {
                            //if we have picked a mult outside of the balanced range, bring the relevant endpoint closer
                            if (!balanced)
                                if (temp > 0)
                                    high = costMult;
                                else if (temp < 0)
                                    low = costMult;
                                else
                                    throw new Exception();
                        }
                        else if (balanced)
                        {
                            //find an upper bound that is outside of the balanced range
                            balanced = false;
                            high += 1 / minError;
                        }
                        else
                        {
                            //we now have lower and upper bounds outside of the balanced range
                            //so we can pick a uniform random mult from within that range
                            randing = true;
                        }
                    }
                    else if (balanced)
                    {
                        balanced = false;
                        accepted = ( MessageBox.Show(units[0].name + " - " + firstCost,
                                "Accept Balance", MessageBoxButtons.YesNo) == DialogResult.Yes );
                        if (accepted)
                            //set an intial lower bound guess
                            low = costMult - 1 / minError;
                        else
                            //do not consider this cost balanced again
                            ignore = firstCost;
                    }
                }

                us.Clear();
                us.CostMult.AddCostMultRow(costMult);
                us.WriteXml("..\\..\\..\\Units.xml");
                rebalanceAll = false;
                getAll();
            }
            else
            {
                MessageBox.Show("no rows");
            }
        }

        struct BalUnit
        {
            public BalUnit(int MaxMove, int BaseRegen, Abilities Ability, int BaseArmor, CityWar.UnitType Type, Attack[] Attacks, bool isThree, int maxHits, string name)
            {
                this.MaxMove = MaxMove;
                this.BaseRegen = BaseRegen;
                this.Ability = Ability;
                this.BaseArmor = BaseArmor;
                this.Type = Type;
                this.Attacks = Attacks;
                this.isThree = isThree;
                this.maxHits = maxHits;
                this.name = name;
            }
            public int MaxMove;
            public int BaseRegen;
            public Abilities Ability;
            public int BaseArmor;
            public CityWar.UnitType Type;
            public Attack[] Attacks;
            public bool isThree;
            public int maxHits;
            public string name;
        }

        void read_Click(object sender, EventArgs e)
        {
            try
            {
                us.ReadXml("..\\..\\..\\Units.xml");
                rebalanceAll = false;
                getAll();
            }
            catch
            {
                us = new UnitSchema();
                rebalanceAll = true;
                getAll();
            }
        }

        void b_Click(object sender, EventArgs e)
        {
            us.Unit.RemoveUnitRow(us.Unit.FindByName((string)( (Button)sender ).Tag));

            getAll();
        }

        private void newBox(int x, int y, int width, string text)
        {
            TextBox box = new TextBox();
            box.Location = new Point(x, y);
            box.Width = width;
            box.ReadOnly = true;
            box.BackColor = Color.Silver;
            box.Text = text;
            Controls.Add(box);
        }

        private void newButton(int x, int y, string text, string tag, EventHandler eh, bool enabled)
        {
            Button b = new Button();
            b.Location = new Point(x, y);
            b.Text = text;
            b.BackColor = Color.Silver;
            b.Tag = tag;
            b.Click += eh;
            Controls.Add(b);

            b.Enabled = enabled;
        }
    }

    public enum CostType
    {
        Air,
        Death,
        Earth,
        Nature,
        Production,
        Water
    }

    public enum UnitType
    {
        Ground,
        Water,
        Air,
        Amphibious,
        All
    }
}