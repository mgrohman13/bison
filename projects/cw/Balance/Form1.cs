using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using CityWar;

namespace balance
{
    //this code sucks, I know
    public partial class Form1 : Form
    {
        bool fireEvent = true;

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

            for (int a = -1 ; ++a < input.Length ; )
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

            air = ( input[++i].Length > 0 );
            carry = ( input[++i].Length > 0 );

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
                CityWar.UnitType type = getType(this.txtType.Text);

                int hits = int.Parse(this.txtHits.Text);
                int armor = int.Parse(this.txtArmor.Text);
                int regen = int.Parse(this.txtRegen.Text);
                int move = int.Parse(this.txtMove.Text);

                int attacks = 0;

                int length1 = 0;
                int damage1 = 0;
                int divide1 = 1;
                if (!( this.txtType1.Text == "-" || this.txtType1.Text == "" ))
                {
                    length1 = int.Parse(this.txtLength1.Text);
                    damage1 = int.Parse(this.txtNumber1.Text);
                    divide1 = int.Parse(this.txtDivide1.Text);
                    ++attacks;
                }

                int length2 = 0;
                int damage2 = 0;
                int divide2 = 1;
                if (!( this.txtType2.Text == "-" || this.txtType2.Text == "" ))
                {
                    length2 = int.Parse(this.txtLength2.Text);
                    damage2 = int.Parse(this.txtNumber2.Text);
                    divide2 = int.Parse(this.txtDivide2.Text);
                    ++attacks;
                }

                int length3 = 0;
                int damage3 = 0;
                int divide3 = 1;
                if (!( this.txtType3.Text == "-" || this.txtType3.Text == "" ))
                {
                    length3 = int.Parse(this.txtLength3.Text);
                    damage3 = int.Parse(this.txtNumber3.Text);
                    divide3 = int.Parse(this.txtDivide3.Text);
                    ++attacks;
                }

                //calculated values
                this.txtRegRate.Text = ( ( hits / ( (double)regen * ( move == 0 ? 1 : move ) ) ) ).ToString("0.0");
                CityWar.UnitType unitType = getType(this.txtType.Text);
                double hitArmor = CityWar.Balance.getArmor(unitType, armor);
                this.txtHitWorth.Text = ( (double)( CityWar.Balance.hitWorth(hits, hitArmor) / CityWar.Balance.hitWorth(1, CityWar.Balance.AverageArmor) ) ).ToString("0.0");
                double weaponMove = CityWar.Balance.getMove(unitType, move, air);
                bool isThree = this.txtName.Text.Contains("*");
                if (this.txtType1.Text == "-" || this.txtType1.Text == "")
                    this.txtDamage1.Clear();
                else
                    this.txtDamage1.Text = ( (double)( CityWar.Balance.weapon(this.txtType1.Text.Length, damage1, divide1, length1, weaponMove, air, isThree, 1) ) ).ToString("0.0");
                if (this.txtType2.Text == "-" || this.txtType2.Text == "")
                    this.txtDamage2.Clear();
                else
                    this.txtDamage2.Text = ( (double)( CityWar.Balance.weapon(this.txtType2.Text.Length, damage2, divide2, length2, weaponMove, air, isThree, 2) ) ).ToString("0.0");
                if (this.txtType3.Text == "-" || this.txtType2.Text == "")
                    this.txtDamage3.Clear();
                else
                    this.txtDamage3.Text = ( (double)( CityWar.Balance.weapon(this.txtType3.Text.Length, damage3, divide3, length3, weaponMove, air, isThree, 3) ) ).ToString("0.0");

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

                double cost = CityWar.Balance.getCost(move, regen, ability, armor, type, Attacks, this.txtName.Text.Contains("*"), hits, out gc);

                double ActCost = Math.Round(cost);

                this.txtPpl.Clear();
                //check that the rounding is within an acceptable percent error
                double invPctErr = ActCost / Math.Abs(cost - ActCost);
                this.txtPctError.Text = invPctErr.ToString("0");
                if (invPctErr > Units.minError || units.rebalanceAll)
                {
                    double pplPercent = double.Parse(this.txtPplPercent.Text);
                    int ppl = (int)Math.Round(pplPercent / 10 * ActCost);
                    int other = (int)Math.Round(( 10 - pplPercent ) / 10 * ActCost);

                    while (ppl + other > ActCost)
                        --other;
                    while (ppl + other < ActCost)
                        ++other;

                    this.txtOutput.Text = getOutput(ppl, other);
                    this.txtCost.Text = other.ToString();
                    this.txtPpl.Text = ppl.ToString();

                    this.btnSave.Enabled = true;
                }
                else
                {
                    this.txtCost.Text = cost.ToString("0.0");
                    this.txtCost.Text = cost.ToString();
                    this.btnSave.Enabled = false;

                    this.txtOutput.Clear();
                }

                if (units.rebalanceAll)
                    this.btnSave_Click(null, null);
            }
            catch (Exception exception)
            {
                this.txtOutput.Text = exception.StackTrace;
            }
        }

        private static CityWar.UnitType getType(String typeStr)
        {
            switch (typeStr)
            {
            case "W":
                return CityWar.UnitType.Water;
            case "G":
                return CityWar.UnitType.Ground;
            case "A":
                return CityWar.UnitType.Air;
            case "GW":
                return CityWar.UnitType.Amphibious;
            case "GWA":
                return CityWar.UnitType.Immobile;
            default:
                throw new Exception();
            }
        }

        private Attack CreateAttack(string type, int length, int damage, int divide)
        {
            MattUtil.EnumFlags<TargetType> target = new MattUtil.EnumFlags<TargetType>();
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
            output.Append(( ppl + other ).ToString() + "\t");
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
            output.Append(( air ? "X" : "" ) + "\t");
            output.Append(( carry ? "X" : "" ) + "\t");
            output.Append(a1Name + "\t");
            output.Append(a2Name + "\t");
            output.Append(a3Name + "\t");
            output.Append(GC() + "\t");
            output.Append(txtHitWorth.Text + "\t");
            output.Append(this.txtDamage1.Text + "\t");
            output.Append(this.txtDamage2.Text + "\t");
            output.Append(this.txtDamage3.Text + "\t");

            return output.ToString().Trim();
        }

        private string GC()
        {
            return gc.ToString("0.0");
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
