using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using CityWar;
using MattUtil;

namespace UnitBalance
{
    //this code sucks, I know
    public partial class Form1 : Form
    {
        bool fireEvent = true;
        public static UnitTypes unitTypes;

        public Form1()
        {
            InitializeComponent();
            this.txtInput.Focus();
        }

        //non visible info
        string costType = "";
        string race;
        bool air, carry;
        string a1Name, a2Name, a3Name;
        double gc = -1;

        Units units = new Units();

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            string[] input = this.txtInput.Text.Split('\t');

            for (int a = -1; ++a < input.Length;)
                input[a] = input[a].Trim();

            fireEvent = false;

            int i = -1;
            this.txtName.Text = input[++i];

            i += 2;
            costType = input[++i];

            i += 1;
            this.txtPplPercent.Text = input[++i];
            race = input[++i];

            this.txtType.Text = input[++i];
            this.txtHits.Text = input[++i];
            this.txtArmor.Text = input[++i];
            this.txtRegen.Text = input[++i];
            this.txtMove.Text = input[++i];

            i += 2;
            this.txtType1.Text = input[++i];
            this.txtNumber1.Text = input[++i];
            this.txtDivide1.Text = input[++i];
            this.txtLength1.Text = input[++i];

            this.txtType2.Text = input[++i];
            this.txtNumber2.Text = input[++i];
            this.txtDivide2.Text = input[++i];
            this.txtLength2.Text = input[++i];

            this.txtType3.Text = input[++i];
            this.txtNumber3.Text = input[++i];
            this.txtDivide3.Text = input[++i];
            this.txtLength3.Text = input[++i];

            air = (input[++i].Length > 0);
            carry = (input[++i].Length > 0);

            a1Name = input[++i];
            a2Name = input[++i];
            a3Name = input[++i];


            fireEvent = true;

            doCalc();

            this.txtInput.Clear();
        }

        private void doCalc()
        {
            try
            {
                UnitType type = UnitTypes.GetType(this.txtType.Text);

                int hits = int.Parse(this.txtHits.Text);
                int armor = int.Parse(this.txtArmor.Text);
                int regen = int.Parse(this.txtRegen.Text);
                int move = int.Parse(this.txtMove.Text);

                int attacks = 0;

                int length1 = 0;
                int damage1 = 0;
                int divide1 = 1;
                if (!(this.txtType1.Text == "-" || this.txtType1.Text == ""))
                {
                    length1 = int.Parse(this.txtLength1.Text);
                    damage1 = int.Parse(this.txtNumber1.Text);
                    divide1 = int.Parse(this.txtDivide1.Text);
                    ++attacks;
                }

                int length2 = 0;
                int damage2 = 0;
                int divide2 = 1;
                if (!(this.txtType2.Text == "-" || this.txtType2.Text == ""))
                {
                    length2 = int.Parse(this.txtLength2.Text);
                    damage2 = int.Parse(this.txtNumber2.Text);
                    divide2 = int.Parse(this.txtDivide2.Text);
                    ++attacks;
                }

                int length3 = 0;
                int damage3 = 0;
                int divide3 = 1;
                if (!(this.txtType3.Text == "-" || this.txtType3.Text == ""))
                {
                    length3 = int.Parse(this.txtLength3.Text);
                    damage3 = int.Parse(this.txtNumber3.Text);
                    divide3 = int.Parse(this.txtDivide3.Text);
                    ++attacks;
                }

                Abilities ability;
                if (air)
                    ability = Abilities.Aircraft;
                else if (carry)
                    ability = Abilities.AircraftCarrier;
                else
                    ability = Abilities.None;

                Attack[] Attacks = new Attack[attacks];
                if (damage1 > 0)
                    Attacks[0] = CreateAttack(this.txtType1.Text, length1, damage1, divide1);
                if (damage2 > 0)
                    Attacks[1] = CreateAttack(this.txtType2.Text, length2, damage2, divide2);
                if (damage3 > 0)
                    Attacks[2] = CreateAttack(this.txtType3.Text, length3, damage3, divide3);

                //calculated values
                this.txtRegRate.Text = ((hits / ((double)regen * (move == 0 ? 1 : move)))).ToString("0.0");
                UnitType unitType = UnitTypes.GetType(this.txtType.Text);
                this.txtHitWorth.Tag = HitWorth(unitType, hits, armor);
                this.txtHitWorth.Text = ((double)this.txtHitWorth.Tag).ToString("0.0");
                double weaponMove = Balance.GetMove(unitType, move, air);
                bool isThree = this.txtName.Text.Contains("*");
                ShowWeaponValue(this.txtDamage1, this.txtType1, unitType, Attacks[0]?.Target, damage1, divide1, length1, move, air, isThree, 1);
                ShowWeaponValue(this.txtDamage2, this.txtType2, unitType, Attacks[1]?.Target, damage2, divide2, length2, weaponMove, air, isThree, 2);
                ShowWeaponValue(this.txtDamage3, this.txtType3, unitType, Attacks[2]?.Target, damage3, divide3, length3, weaponMove, air, isThree, 3);

                double cost = Balance.GetCost(unitTypes, type, this.txtName.Text.Contains("*"), ability, hits, armor, regen, move, Attacks, out gc);

                double ActCost = Math.Round(cost);

                this.txtPpl.Clear();
                //check that the rounding is within an acceptable percent error
                double invPctErr = ActCost / Math.Abs(cost - ActCost);
                this.txtPctError.Text = invPctErr.ToString("0");
                if (invPctErr > Units.minError || units.rebalanceAll)
                {
                    double pplPercent = double.Parse(this.txtPplPercent.Text);
                    int ppl = (int)Math.Round(pplPercent / 10 * ActCost);
                    int other = (int)Math.Round((10 - pplPercent) / 10 * ActCost);

                    while (ppl + other > ActCost)
                        --other;
                    while (ppl + other < ActCost)
                        ++other;

                    this.txtOutput.Text = getOutput(ppl, other);
                    this.txtCost.Text = other.ToString();
                    this.txtPpl.Text = ppl.ToString();

                    this.btnSave.Visible = true;
                }
                else
                {
                    this.txtCost.Text = cost.ToString("0.0");
                    this.txtCost.Text = cost.ToString();
                    this.btnSave.Visible = false;

                    this.txtOutput.Clear();
                }

                //if (units.rebalanceAll)
                //    this.btnSave_Click(null, null);
            }
            catch (Exception exception)
            {
                this.txtOutput.Text = exception.StackTrace;
            }
        }

        private static double HitWorth(UnitType unitType, int hits, int armor)
        {
            double hitArmor = Balance.GetArmor(unitType, armor);
            double hitDiv = Balance.HitWorth(1, Balance.GetAverageDamage(unitTypes.GetAverageDamage(), unitTypes.GetAverageAP(null), unitTypes.GetAverageArmor(null)));
            return Balance.HitWorth(unitTypes, unitType, hits, hitArmor) / hitDiv;
        }

        private static void ShowWeaponValue(TextBox txtValue, TextBox txtType, UnitType type, EnumFlags<TargetType> targets, double damage, double divide, double length, double move, bool air, bool isThree, int num)
        {
            double value;
            if (txtType.Text == "-" || txtType.Text == "")
                value = double.NaN;
            else
                value = Balance.Weapon(unitTypes, type, targets, damage, divide, length, move, air, isThree, num);
            ShowWeaponValue(txtValue, value);
        }
        private static void ShowWeaponValue(TextBox txtValue, double value)
        {
            if (double.IsNaN(value))
            {
                txtValue.Tag = "-";
                txtValue.Text = "";
            }
            else
            {
                txtValue.Tag = value;
                txtValue.Text = value.ToString("0.0");
            }
        }

        private static Attack CreateAttack(string type, int length, int damage, int divide)
        {
            EnumFlags<TargetType> target = new EnumFlags<TargetType>();
            if (type.Contains("A"))
                target.Add(TargetType.Air);
            if (type.Contains("G"))
                target.Add(TargetType.Ground);
            if (type.Contains("W"))
                target.Add(TargetType.Water);
            return new Attack(target, length, damage, divide);
        }

        //output to be pasted into excel
        private string getOutput(int ppl, int other)
        {
            StringBuilder output = new StringBuilder("");
            output.Append(this.txtName.Text + "\t");
            output.Append((ppl + other).ToString() + "\t");
            output.Append(other.ToString() + "\t");
            output.Append(costType + "\t");
            output.Append(ppl.ToString() + "\t");
            output.Append(this.txtPplPercent.Text + "\t");
            output.Append(race + "\t");
            output.Append(this.txtType.Text + "\t");
            output.Append(this.txtHits.Text + "\t");
            output.Append(this.txtArmor.Text + "\t");
            output.Append(this.txtRegen.Text + "\t");
            output.Append(this.txtMove.Text + "\t");
            output.Append("\t\t");
            output.Append(this.txtType1.Text + "\t");
            output.Append(this.txtNumber1.Text + "\t");
            output.Append(this.txtDivide1.Text + "\t");
            output.Append(this.txtLength1.Text + "\t");
            output.Append(this.txtType2.Text + "\t");
            output.Append(this.txtNumber2.Text + "\t");
            output.Append(this.txtDivide2.Text + "\t");
            output.Append(this.txtLength2.Text + "\t");
            output.Append(this.txtType3.Text + "\t");
            output.Append(this.txtNumber3.Text + "\t");
            output.Append(this.txtDivide3.Text + "\t");
            output.Append(this.txtLength3.Text + "\t");
            output.Append((air ? "X" : "") + "\t");
            output.Append((carry ? "X" : "") + "\t");
            output.Append(a1Name + "\t");
            output.Append(a2Name + "\t");
            output.Append(a3Name + "\t");
            output.Append(GC() + "\t");
            output.Append(txtHitWorth.Tag + "\t");
            output.Append(this.txtDamage1.Tag + "\t");
            output.Append(this.txtDamage2.Tag + "\t");
            output.Append(this.txtDamage3.Tag + "\t");

            return output.ToString().Trim();
        }

        private string GC()
        {
            if (double.IsNaN(gc))
                return "-";
            return gc.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.txtOutput.Text = GetAllOutput(this.units);
        }
        private static string GetAllOutput(Units units)
        {
            StringBuilder output = new StringBuilder("");
            string r = null;
            foreach (UnitSchema.UnitRow row in units.us.Unit.OrderBy(u => u.Race).ThenBy(u => u.Cost + u.People).ThenBy(u => u.Name))
            {
                if (r != null && r != row.Race)
                    output.Append(Environment.NewLine);

                output.Append(row.Name + (row.IsThree ? "*" : "") + "\t");
                output.Append((row.People + row.Cost).ToString() + "\t");
                output.Append(row.Cost.ToString() + "\t");
                output.Append((row.CostType == "" ? "' " : row.CostType) + "\t");
                output.Append(row.People.ToString() + "\t");
                output.Append(Math.Round(row.People / (double)(row.People + row.Cost) * 10) + "\t");
                output.Append(row.Race + "\t");
                output.Append(row.Type + "\t");
                output.Append(row.Hits + "\t");
                output.Append(row.Armor + "\t");
                output.Append(row.Regen + "\t");
                output.Append(row.Move + "\t");
                output.Append("\t\t");

                Abilities ability;
                if (row.Special == "Aircraft")
                    ability = Abilities.Aircraft;
                else if (row.Special == "AircraftCarrier")
                    ability = Abilities.AircraftCarrier;
                else
                    ability = Abilities.None;
                Attack[] Attacks = new Attack[row.GetAttackRows().Length];
                UnitType type = UnitTypes.GetType(row.Type);
                double weaponMove = Balance.GetMove(type, row.Move, row.Special == "Aircraft");
                string[] attacknames = new string[3];
                double[] attackValues = new double[3];
                int idx = 0;
                foreach (UnitSchema.AttackRow attackRow in row.GetAttackRows())
                {
                    output.Append(attackRow.Target_Type + "\t");
                    output.Append(attackRow.Damage + "\t");
                    output.Append(attackRow.Divide_By + "\t");
                    output.Append(attackRow.Length + "\t");
                    attacknames[idx] = attackRow.Name;
                    Attacks[idx] = CreateAttack(attackRow.Target_Type, attackRow.Length, attackRow.Damage, attackRow.Divide_By);
                    attackValues[idx] = Balance.Weapon(unitTypes, type, Attacks[idx].Target, attackRow.Damage, attackRow.Divide_By, attackRow.Length, weaponMove, (row.Special == "Aircraft"), row.IsThree, idx + 1);
                    ++idx;
                }
                while (idx++ < 3)
                    output.Append("'-\t'-\t'-\t'-\t");

                output.Append(((row.Special == "Aircraft") ? "X" : "' ") + "\t");
                output.Append(((row.Special == "AircraftCarrier") ? "X" : "' ") + "\t");
                output.Append(attacknames[0] + "\t");
                output.Append(attacknames[1] + "\t");
                output.Append(attacknames[2] + "\t");
                double gc;
                Balance.GetCost(unitTypes, type, row.IsThree, ability, row.Hits, row.Armor, row.Regen, row.Move, Attacks, out gc);
                double hw = HitWorth(type, row.Hits, row.Armor);
                output.Append((double.IsNaN(gc) ? "-" : gc.ToString()) + "\t");
                output.Append(hw + "\t");
                output.Append(attackValues[0] + "\t");
                output.Append(attackValues[1] + "\t");
                output.Append(attackValues[2] + "\t");
                output.Append(Environment.NewLine);

                r = row.Race;
            }

            return output.ToString();
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            if (fireEvent)
                doCalc();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            units.Show();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (this.txtMove.Text == "0")
                this.txtPpl.Text = "0";

            string special = "";
            if (air)
                special = "Aircraft";
            else if (carry)
                special = "AircraftCarrier";
            try
            {
                units.us.Unit.RemoveUnitRow(units.us.Unit.FindByName(this.txtName.Text.Trim('*')));
            }
            catch
            {
            }
            units.us.Unit.AddUnitRow(this.txtName.Text.Trim('*'), int.Parse(this.txtCost.Text),
                this.txtType.Text, int.Parse(this.txtHits.Text), int.Parse(this.txtArmor.Text),
                int.Parse(this.txtRegen.Text), int.Parse(this.txtMove.Text), costType,
                this.txtName.Text.Contains("*"), special, int.Parse(this.txtPpl.Text), race);

            if (this.txtType1.Text != "-" && this.txtType1.Text != "")
            {
                units.us.Attack.AddAttackRow(this.txtType1.Text, int.Parse(this.txtLength1.Text),
                    int.Parse(this.txtNumber1.Text), int.Parse(this.txtDivide1.Text), units.us.Unit.FindByName(this.txtName.Text.Trim('*')), a1Name);

                if (this.txtType2.Text != "-" && this.txtType2.Text != "")
                {
                    units.us.Attack.AddAttackRow(this.txtType2.Text, int.Parse(this.txtLength2.Text),
                        int.Parse(this.txtNumber2.Text), int.Parse(this.txtDivide2.Text), units.us.Unit.FindByName(this.txtName.Text.Trim('*')), a2Name);

                    if (this.txtType3.Text != "-" && this.txtType3.Text != "")
                    {
                        units.us.Attack.AddAttackRow(this.txtType3.Text, int.Parse(this.txtLength3.Text),
                            int.Parse(this.txtNumber3.Text), int.Parse(this.txtDivide3.Text), units.us.Unit.FindByName(this.txtName.Text.Trim('*')), a3Name);
                    }
                }
            }

            units.getAll();

            this.txtOutput.SelectAll();
        }
    }
}
