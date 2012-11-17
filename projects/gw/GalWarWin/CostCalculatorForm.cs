using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin
{
    public partial class CostCalculatorForm : Form
    {
        private static CostCalculatorForm form = new CostCalculatorForm();

        private bool events = true, deathStar = false;

        public CostCalculatorForm()
        {
            InitializeComponent();

            Update(null);
        }

        private void SetShip(Ship ship)
        {
            events = false;

            SetValue(this.nudResearch, MainForm.Game.CurrentPlayer.GetLastResearched());
            SetValue(this.nudAtt, ship.Att);
            SetValue(this.nudDef, ship.Def);
            SetValue(this.nudHP, ship.MaxHP);
            SetValue(this.nudSpeed, ship.MaxSpeed);
            SetValue(this.nudTrans, ship.MaxPop);
            this.cbCol.Checked = ship.Colony;
            SetValue(this.nudDS, ship.BombardDamage);

            double totCost = Update(null);
            events = false;

            if (ship.Player.IsTurn)
            {
                SetValue(this.nudProd, ship.GetProdForHP(ship.MaxHP) / Consts.RepairCostMult);
                SetValue(this.nudUpk, ship.Upkeep);
            }
            else
            {
                SetValue(this.nudProd, totCost * ( 1 - Consts.CostUpkeepPct ));
                SetValue(this.nudUpk, totCost * Consts.CostUpkeepPct /
                        GetUpkeepPayoff(ship.Att, ship.Def, ship.MaxHP, ship.MaxSpeed, ship.MaxPop, ship.Colony,
                        GetBombardDamageMult(ship.Att), MainForm.Game.CurrentPlayer.GetLastResearched()));
            }

            events = true;
        }

        private void SetShipDesign(ShipDesign shipDesign)
        {
            events = false;

            SetValue(this.nudProd, shipDesign.Cost);
            SetValue(this.nudResearch, shipDesign.Research);
            SetValue(this.nudAtt, shipDesign.Att);
            SetValue(this.nudDef, shipDesign.Def);
            SetValue(this.nudHP, shipDesign.HP);
            SetValue(this.nudSpeed, shipDesign.Speed);
            SetValue(this.nudUpk, shipDesign.Upkeep);
            SetValue(this.nudTrans, shipDesign.Trans);
            this.cbCol.Checked = shipDesign.Colony;
            SetValue(this.nudDS, shipDesign.BombardDamage);

            Update(null);
        }

        private double Update(object sender)
        {
            this.lblOverflow.Visible = false;
            events = false;

            double research = (double)this.nudResearch.Value;
            int att = (int)this.nudAtt.Value;
            int def = (int)this.nudDef.Value;
            int hp = (int)this.nudHP.Value;
            int speed = (int)this.nudSpeed.Value;
            int upk = (int)this.nudUpk.Value;
            int trans = (int)this.nudTrans.Value;
            bool colony = (bool)this.cbCol.Checked;
            float bombardDamageMult = GetBombardDamageMult(att);
            deathStar = ( Math.Abs(bombardDamageMult - 1) > Consts.FLOAT_ERROR );

            if (sender == nudProd)
            {
                research = CalcResearch(att, def, hp, speed, trans, colony, bombardDamageMult, (double)this.nudProd.Value, upk);
                SetValue(this.nudResearch, research);
            }

            double totCost = ShipDesign.GetTotCost(att, def, hp, speed, trans, colony, bombardDamageMult, research);

            if (sender != null && sender != nudProd)
                SetValue(this.nudProd, GetProd(att, def, hp, speed, trans, colony, bombardDamageMult, research, upk, totCost));

            this.txtStr.Text = MainForm.FormatDouble(ShipDesign.GetStrength(att, def, hp, speed));
            this.txtCost.Text = MainForm.FormatDouble(totCost);
            this.txtValue.Text = MainForm.FormatDouble(ShipDesign.GetValue(att, def, hp, speed, trans, colony, bombardDamageMult, research));

            events = true;
            return totCost;
        }

        private double CalcResearch(int att, int def, int hp, int speed, int trans, bool colony, float bombardDamageMult, double prod, int upk)
        {
            int research = MattUtil.TBSUtil.FindValue(delegate(int r)
            {
                return ( GetProd(att, def, hp, speed, trans, colony, bombardDamageMult, r, upk) < prod );
            }, (int)this.nudResearch.Minimum, (int)this.nudResearch.Maximum, true);

            if (( prod - GetProd(att, def, hp, speed, trans, colony, bombardDamageMult, research, upk) ) >
                    ( GetProd(att, def, hp, speed, trans, colony, bombardDamageMult, research - 1, upk) - prod ))
                --research;

            return research;
        }

        private float GetBombardDamageMult(int att)
        {
            return (float)( this.nudDS.Value / att / (decimal)Consts.BombardAttackMult );
        }

        private static double GetProd(int att, int def, int hp, int speed, int trans, bool colony, float bombardDamageMult, int research, int upk)
        {
            return GetProd(att, def, hp, speed, trans, colony, bombardDamageMult, research, upk,
                    ShipDesign.GetTotCost(att, def, hp, speed, trans, colony, bombardDamageMult, research));
        }
        private static double GetProd(int att, int def, int hp, int speed, int trans, bool colony, float bombardDamageMult, double research, int upk, double totCost)
        {
            return ( totCost - upk * GetUpkeepPayoff(att, def, hp, speed, trans, colony, bombardDamageMult, research) );
        }

        private static double GetUpkeepPayoff(int att, int def, int hp, int speed, int trans, bool colony, float bombardDamageMult, double research)
        {
            return Consts.GetUpkeepPayoff(MainForm.Game.MapSize,
                    Consts.GetNonColonyPct(att, def, hp, speed, trans, colony, bombardDamageMult, research),
                    Consts.GetNonTransPct(att, def, hp, speed, trans, colony, bombardDamageMult, research), speed);
        }

        private void SetValue(NumericUpDown nud, double value)
        {
            decimal setValue = (decimal)value;
            if (setValue < nud.Minimum)
            {
                this.lblOverflow.Visible = true;
                setValue = nud.Minimum;
            }
            else if (setValue > nud.Maximum)
            {
                this.lblOverflow.Visible = true;
                setValue = nud.Maximum;
            }
            nud.Value = setValue;
        }

        private void nud_ValueChanged(object sender, EventArgs e)
        {
            if (events)
            {
                if (!deathStar && sender == this.nudAtt)
                    SetValue(this.nudDS, (double)( this.nudAtt.Value * (decimal)Consts.BombardAttackMult ));
                Update(sender);
            }
        }

        private void cb_CheckedChanged(object sender, EventArgs e)
        {
            if (events)
                Update(sender);
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        public static void ShowForm(MainForm gameForm)
        {
            ShowForm(gameForm, null, null);
        }
        public static void ShowForm(MainForm gameForm, Ship selected)
        {
            ShowForm(gameForm, selected, null);
        }
        public static void ShowForm(MainForm gameForm, ShipDesign selected)
        {
            ShowForm(gameForm, null, selected);
        }
        private static void ShowForm(MainForm gameForm, Ship ship, ShipDesign shipDesign)
        {
            gameForm.SetLocation(form);

            if (ship != null)
                form.SetShip(ship);
            else if (shipDesign != null)
                form.SetShipDesign(shipDesign);

            form.Show();
        }
    }
}
