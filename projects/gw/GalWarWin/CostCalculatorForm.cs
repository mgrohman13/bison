using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GalWar;
using MattUtil;

namespace GalWarWin
{
    public partial class CostCalculatorForm : Form
    {
        private static CostCalculatorForm form = new CostCalculatorForm();

        private bool events;

        public CostCalculatorForm()
        {
            InitializeComponent();
            InitRand();
        }

        private void InitRand()
        {
            events = false;

            this.lblOverflow.Visible = false;

            int research = MainForm.Game.CurrentPlayer.GetLastResearched();
            SetValue(this.nudResearch, Game.Random.GaussianCappedInt(research,
                    Consts.ResearchRndm, (int)Math.Min(research, Consts.StartResearch)));

            double str = ShipDesign.GetAttDefStr(research);
            SetValue(this.nudAtt, ShipDesign.MakeStat(str));
            SetValue(this.nudDef, ShipDesign.MakeStat(str));
            SetValue(this.nudHP, ShipDesign.MakeStat(ShipDesign.GetHPStr(ShipDesign.MakeStat(str), ShipDesign.MakeStat(str))));
            SetValue(this.nudSpeed, ShipDesign.MakeStat(ShipDesign.GetSpeedStr(research)));

            bool colony;
            int trans;
            double bombardDamageMult, transStr = ShipDesign.GetTransStr(research);
            ShipDesign.DoColonyTrans(false, false, false, research, ref transStr, out colony, out trans, out bombardDamageMult);

            SetValue(this.nudTrans, trans);
            this.cbCol.Checked = colony;

            SetValue(this.nudDS, GetBombardDamage(bombardDamageMult));

            double totCost = Update(null);
            events = false;

            CalcCost(research, totCost);

            events = true;
        }

        private void SetShip(Ship ship)
        {
            events = false;

            this.lblOverflow.Visible = false;

            double research = MainForm.Game.CurrentPlayer.GetLastResearched();
            SetValue(this.nudResearch, research);
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
                CalcCost(research, totCost);
            }

            events = true;
        }

        private void CalcCost(double research, double totCost)
        {
            int att = (int)this.nudAtt.Value;
            int def = (int)this.nudDef.Value;
            int hp = (int)this.nudHP.Value;
            int speed = (int)this.nudSpeed.Value;
            int trans = (int)this.nudTrans.Value;
            bool colony = (bool)this.cbCol.Checked;

            double upkeepPayoff = GetUpkeepPayoff(att, def, hp, speed, trans, colony, GetBombardDamageMult(att), research);
            double upkeep = Math.Round(totCost / upkeepPayoff * Consts.CostUpkeepPct);
            if (upkeep < 1)
                upkeep = 1;
            SetValue(this.nudProd, totCost - upkeep * upkeepPayoff);
            SetValue(this.nudUpk, upkeep);
        }

        private void SetShipDesign(ShipDesign shipDesign)
        {
            events = false;

            this.lblOverflow.Visible = false;

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
            events = false;
            if (sender != null)
                this.lblOverflow.Visible = false;

            double research = (double)this.nudResearch.Value;
            int att = (int)this.nudAtt.Value;
            int def = (int)this.nudDef.Value;
            int hp = (int)this.nudHP.Value;
            int speed = (int)this.nudSpeed.Value;
            int upk = (int)this.nudUpk.Value;
            int trans = (int)this.nudTrans.Value;
            bool colony = (bool)this.cbCol.Checked;
            double bombardDamageMult = GetBombardDamageMult(att);

            if (bombardDamageMult < 1)
            {
                bombardDamageMult = 1;
                ClearDS();
            }
            this.cbDS.Checked = ( bombardDamageMult > 1 + Consts.FLOAT_ERROR );

            if (sender == nudProd)
            {
                research = CalcResearch(att, def, hp, speed, trans, colony, bombardDamageMult, (double)this.nudProd.Value, upk);
                SetValue(this.nudResearch, research);
            }

            double totCost = ShipDesign.GetTotCost(att, def, hp, speed, trans, colony, bombardDamageMult, research);

            if (sender != null && sender != nudProd)
                SetValue(this.nudProd, GetProd(att, def, hp, speed, trans, colony, bombardDamageMult, research, upk, totCost));

            this.txtStr.Text = GraphsForm.GetArmadaString(ShipDesign.GetStrength(att, def, hp, speed));
            this.txtCost.Text = MainForm.FormatDouble(totCost);
            this.txtValue.Text = GraphsForm.GetArmadaString(ShipDesign.GetValue(att, def, hp, speed, trans, colony, bombardDamageMult, research));

            events = true;
            return totCost;
        }

        private int CalcResearch(int att, int def, int hp, int speed, int trans, bool colony, double bombardDamageMult, double prod, int upk)
        {
            int research = TBSUtil.FindValue(delegate(int r)
            {
                return ( GetProd(att, def, hp, speed, trans, colony, bombardDamageMult, r, upk) < prod );
            }, (int)this.nudResearch.Minimum, (int)this.nudResearch.Maximum, true);

            if (( prod - GetProd(att, def, hp, speed, trans, colony, bombardDamageMult, research, upk) ) >
                    ( GetProd(att, def, hp, speed, trans, colony, bombardDamageMult, research - 1, upk) - prod ))
                --research;

            return research;
        }

        private static double GetProd(int att, int def, int hp, int speed, int trans, bool colony, double bombardDamageMult, int research, int upk)
        {
            return GetProd(att, def, hp, speed, trans, colony, bombardDamageMult, research, upk,
                    ShipDesign.GetTotCost(att, def, hp, speed, trans, colony, bombardDamageMult, research));
        }
        private static double GetProd(int att, int def, int hp, int speed, int trans, bool colony, double bombardDamageMult, double research, int upk, double totCost)
        {
            return ( totCost - upk * GetUpkeepPayoff(att, def, hp, speed, trans, colony, bombardDamageMult, research) );
        }

        private static double GetUpkeepPayoff(int att, int def, int hp, int speed, int trans, bool colony, double bombardDamageMult, double research)
        {
            return Consts.GetUpkeepPayoff(MainForm.Game.MapSize,
                    Consts.GetNonColonyPct(att, def, hp, speed, trans, colony, bombardDamageMult, research),
                    Consts.GetNonTransPct(att, def, hp, speed, trans, colony, bombardDamageMult, research), speed);
        }

        private void MaintainDS(bool deathStar)
        {
            if (events)
            {
                events = false;

                if (deathStar)
                    SetValue(this.nudTrans, 0);
                else
                    ClearDS();

                events = true;
            }
        }
        private void ClearDS()
        {
            events = false;

            SetValue(this.nudDS, GetBombardDamage(1));

            events = true;
        }

        private double GetBombardDamageMult(decimal att)
        {
            return (double)( this.nudDS.Value / att / (decimal)Consts.BombardAttackMult );
        }
        private double GetBombardDamage(double bombardDamageMult)
        {
            return (double)( this.nudAtt.Value * (decimal)bombardDamageMult * (decimal)Consts.BombardAttackMult );
        }

        private void SetValue(NumericUpDown nud, double value)
        {
            if (nud.DecimalPlaces == 0 && (int)value != value)
                throw new Exception();

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

        private void nudAtt_ValueChanged(object sender, EventArgs e)
        {
            if (events && !this.cbDS.Checked)
                ClearDS();
            nud_ValueChanged(sender, e);
        }

        private void nudTrans_ValueChanged(object sender, EventArgs e)
        {
            MaintainDS(false);
            nud_ValueChanged(sender, e);
        }

        private void nudDS_ValueChanged(object sender, EventArgs e)
        {
            MaintainDS(true);
            nud_ValueChanged(sender, e);
        }

        private void cbDS_CheckedChanged(object sender, EventArgs e)
        {
            if (events && this.cbDS.Checked)
                SetValue(this.nudDS, GetBombardDamage(ShipDesign.DeathStarAvg));
            MaintainDS(this.cbDS.Checked);
            cb_CheckedChanged(sender, e);
        }

        private void nud_ValueChanged(object sender, EventArgs e)
        {
            if (events)
                Update(sender);
        }

        private void cb_CheckedChanged(object sender, EventArgs e)
        {
            if (events)
                Update(sender);
        }

        public static void ShowForm()
        {
            ShowForm(null, null);
        }
        public static void ShowForm(Ship selected)
        {
            ShowForm(selected, null);
        }
        public static void ShowForm(ShipDesign selected)
        {
            ShowForm(null, selected);
        }
        private static void ShowForm(Ship ship, ShipDesign shipDesign)
        {
            MainForm.GameForm.SetLocation(form);

            if (ship != null)
                form.SetShip(ship);
            else if (shipDesign != null)
                form.SetShipDesign(shipDesign);

            form.ShowDialog();
        }
    }
}
