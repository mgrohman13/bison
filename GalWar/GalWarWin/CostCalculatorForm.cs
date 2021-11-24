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

        private bool events = true;

        public CostCalculatorForm()
        {
            InitializeComponent();
            InitRand();
        }

        private void StopEvents(Action Action)
        {
            bool previous = this.events;
            this.events = false;
            Action();
            this.events = previous;
        }

        private void InitRand()
        {
            StopEvents(() =>
            {
                this.lblOverflow.Visible = false;

                int research = Game.Random.GaussianOEInt(MainForm.Game.AvgResearch,
                        Consts.ResearchRndm, Consts.ResearchRndm, (int)Math.Min(MainForm.Game.AvgResearch, Consts.StartResearch));
                SetValue(this.nudResearch, research);

                double str = ShipDesign.GetAttDefStr(research);
                SetValue(this.nudAtt, ShipDesign.MakeStat(str));
                SetValue(this.nudDef, ShipDesign.MakeStat(str));
                SetValue(this.nudHP, ShipDesign.MakeStat(ShipDesign.GetHPStr(ShipDesign.MakeStat(str), ShipDesign.MakeStat(str))));
                SetValue(this.nudSpeed, ShipDesign.MakeStat(ShipDesign.GetSpeedStr(research)));

                bool colony;
                int trans;
                double bombardDamageMult, transStr = ShipDesign.GetTransStr(research);
                ShipDesign.DoColonyTransDS(false, false, false, research, ref transStr, out colony, out trans, out bombardDamageMult);

                SetValue(this.nudTrans, trans);
                this.cbCol.Checked = colony;

                if (bombardDamageMult > 0)
                    SetValue(this.nudDS, Game.Random.Round(bombardDamageMult * GetBombardDamage()));
                else
                    ClearDS();

                double totCost = Update(null);

                CalcCost(research, totCost);
            });
        }

        private void SetShip(Ship ship)
        {
            StopEvents(() =>
            {
                do
                {
                    this.lblOverflow.Visible = false;

                    SetValue(this.nudAtt, ship.Att);
                    SetValue(this.nudDef, ship.Def);
                    SetValue(this.nudHP, ship.MaxHP);
                    SetValue(this.nudSpeed, ship.MaxSpeed);
                    SetValue(this.nudTrans, ship.MaxPop);
                    this.cbCol.Checked = ship.Colony;
                    this.cbDS.Checked = ship.DeathStar;
                    SetValue(this.nudDS, ship.BombardDamage);

                    if (ship.Player.IsTurn)
                    {
                        double cost = ((IShipStats)ship).Cost;
                        SetValue(this.nudProd, cost);
                        SetValue(this.nudUpk, ship.BaseUpkeep);
                        SetValue(this.nudResearch, CalcResearch(ship.Att, ship.Def, ship.MaxHP, ship.MaxSpeed, ship.MaxPop, ship.Colony, ship.BombardDamage, cost, ship.BaseUpkeep));

                        Update(null);
                    }
                    else
                    {
                        int research = (int)Math.Round(GraphsForm.GetLastResearched(MainForm.Game.GetResearch(), ship.Player));
                        SetValue(this.nudResearch, research);

                        double totCost = Update(null);

                        CalcCost(research, totCost);
                    }
                } while (this.lblOverflow.Visible);
            });
        }

        private void CalcCost(int research, double totCost)
        {
            int att = (int)this.nudAtt.Value;
            int def = (int)this.nudDef.Value;
            int hp = (int)this.nudHP.Value;
            int speed = (int)this.nudSpeed.Value;
            int trans = (int)this.nudTrans.Value;
            bool colony = (bool)this.cbCol.Checked;
            double BombardDamage = (double)this.nudDS.Value;

            double upkeepPayoff = GetUpkeepPayoff(att, def, hp, speed, trans, colony, BombardDamage, MainForm.Game);
            double upkeep = Math.Round(totCost / upkeepPayoff * Consts.CostUpkeepPct);
            if (upkeep < 1)
                upkeep = 1;
            SetValue(this.nudProd, totCost - upkeep * upkeepPayoff);
            SetValue(this.nudUpk, upkeep);

            SetColony();
        }

        private void SetShipDesign(ShipDesign shipDesign)
        {
            StopEvents(() =>
            {
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
                this.cbDS.Checked = shipDesign.DeathStar;
                SetValue(this.nudDS, shipDesign.BombardDamage);

                Update(null);
                this.Text = shipDesign.ToString();
            });
        }

        private double Update(object sender)
        {
            double totCost = double.NaN;
            StopEvents(() =>
            {
                this.Text = string.Empty;

                if (sender != null)
                    this.lblOverflow.Visible = false;

                int research = (int)this.nudResearch.Value;
                int att = (int)this.nudAtt.Value;
                int def = (int)this.nudDef.Value;
                int hp = (int)this.nudHP.Value;
                int speed = (int)this.nudSpeed.Value;
                int upk = (int)this.nudUpk.Value;
                int trans = (int)this.nudTrans.Value;
                bool colony = (bool)this.cbCol.Checked;
                double bombardDamage = (double)this.nudDS.Value;

                this.cbDS.Checked = (bombardDamage > GetBombardDamage() * Consts.FLOAT_ERROR_ONE);
                if (!cbDS.Checked)
                {
                    bombardDamage = GetBombardDamage();
                    ClearDS();
                }

                if (sender == nudProd)
                {
                    research = CalcResearch(att, def, hp, speed, trans, colony, bombardDamage, (double)this.nudProd.Value, upk);
                    SetValue(this.nudResearch, research);
                }

                totCost = ShipDesign.GetTotCost(att, def, hp, speed, trans, colony, bombardDamage, research);

                if (sender != null && sender != nudProd)
                    SetValue(this.nudProd, GetProd(att, def, hp, speed, trans, colony, bombardDamage, research, upk, totCost));

                this.txtStr.Text = GraphsForm.GetArmadaString(ShipDesign.GetStrength(att, def, hp, speed));
                this.txtCost.Text = MainForm.FormatDouble(totCost);
                this.txtValue.Text = GraphsForm.GetArmadaString(ShipDesign.GetValue(att, def, hp, speed, trans, colony, bombardDamage, MainForm.Game));

                SetColony();
            });
            return totCost;
        }

        private int CalcResearch(int att, int def, int hp, int speed, int trans, bool colony, double bombardDamage, double prod, int upk)
        {
            int research = TBSUtil.FindValue(delegate (int r)
            {
                return (GetProd(att, def, hp, speed, trans, colony, bombardDamage, r, upk) < prod);
            }, (int)this.nudResearch.Minimum, (int)this.nudResearch.Maximum, true);

            if ((prod - GetProd(att, def, hp, speed, trans, colony, bombardDamage, research, upk)) >
                    (GetProd(att, def, hp, speed, trans, colony, bombardDamage, research - 1, upk) - prod))
                --research;

            return research;
        }

        private static double GetProd(int att, int def, int hp, int speed, int trans, bool colony, double bombardDamage, int research, int upk)
        {
            return GetProd(att, def, hp, speed, trans, colony, bombardDamage, research, upk,
                    ShipDesign.GetTotCost(att, def, hp, speed, trans, colony, bombardDamage, research));
        }
        private static double GetProd(int att, int def, int hp, int speed, int trans, bool colony, double bombardDamage, int research, int upk, double totCost)
        {
            return (totCost - upk * GetUpkeepPayoff(att, def, hp, speed, trans, colony, bombardDamage, MainForm.Game));
        }

        private static double GetUpkeepPayoff(int att, int def, int hp, int speed, int trans, bool colony, double bombardDamage, Game game)
        {
            return Consts.GetUpkeepPayoff(MainForm.Game.MapSize,
                    Consts.GetNonColonyPct(att, def, hp, speed, trans, colony, bombardDamage, game, true),
                    Consts.GetNonTransPct(att, def, hp, speed, trans, colony, bombardDamage, game, true), speed);
        }

        private void MaintainDS(bool deathStar)
        {
            if (events)
            {
                StopEvents(() =>
                {
                    if (deathStar)
                    {
                        SetValue(this.nudTrans, 0);
                        CheckDSMin();
                    }
                    else
                    {
                        ClearDS();
                    }
                });
            }
        }

        private void CheckDSMin()
        {
            StopEvents(() =>
            {
                int min = ShipDesign.GetDeathStarMin((int)this.nudAtt.Value);
                int bombardDamage = (int)this.nudDS.Value;
                if (bombardDamage < min)
                    SetValue(this.nudDS, min);
            });
        }

        private void ClearDS()
        {
            SetValue(this.nudDS, GetBombardDamage());
        }

        private double GetBombardDamage()
        {
            return Consts.GetBombardDamage((double)this.nudAtt.Value);
        }

        private void SetColony()
        {
            bool colony = (bool)this.cbCol.Checked;
            double cost = (double)this.nudProd.Value;
            int att = (int)this.nudAtt.Value;
            int def = (int)this.nudDef.Value;
            int hp = (int)this.nudHP.Value;
            int speed = (int)this.nudSpeed.Value;
            int trans = (int)this.nudTrans.Value;
            double bombardDamage = (double)this.nudDS.Value;

            this.txtColonyValue.Text = (colony
                    ? MainForm.FormatDouble(cost * Consts.GetNonColonyPct(att, def, hp, speed, trans, colony, bombardDamage, MainForm.Game, false))
                    : string.Empty);
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
            if (this.cbDS.Checked)
            {
                CheckDSMin();
            }
            else if (events)
            {
                StopEvents(() =>
                {
                    ClearDS();
                });
            }
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
            if (this.cbDS.Checked)
            {
                bool oldEvents = events;
                events = false;
                this.cbCol.Checked = false;
                events = oldEvents;
            }

            this.nudDS.DecimalPlaces = (this.cbDS.Checked ? 0 : 1);
            if (events && this.cbDS.Checked)
                SetValue(this.nudDS, Game.Random.Round(GetBombardDamage() * ShipDesign.DeathStarAvg));
            MaintainDS(this.cbDS.Checked);
            cb_CheckedChanged(sender, e);
        }

        private void cbCol_CheckedChanged(object sender, EventArgs e)
        {
            if (this.cbCol.Checked)
                this.cbDS.Checked = false;

            SetColony();
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

        public static int CalcResearch(Ship ship)
        {
            form.SetShip(ship);
            return (int)form.nudResearch.Value;
        }

        public static double CalcColonizationValue(Ship ship)
        {
            form.SetShip(ship);
            return double.Parse(form.txtColonyValue.Text);
        }

        private void label16_Click(object sender, EventArgs e)
        {
            double cost = (double)this.nudProd.Value;
            int hp = (int)this.nudHP.Value;
            Sliders.SliderForm.ShowForm(new Sliders.GoldRepair(hp, Consts.RepairCostMult * cost / (double)hp));
        }
    }
}
