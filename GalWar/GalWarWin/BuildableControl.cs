using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin
{
    public partial class BuildableControl : UserControl
    {
        private Colony colony;
        private Buildable buildable;
        private ShipDesign shipDesign;
        private bool? addProd;

        public BuildableControl()
        {
            InitializeComponent();
        }

        public bool SetColony(Colony colony)
        {
            return SetColony(colony, null, null, null);
        }
        public bool SetBuildable(Buildable buildable)
        {
            return SetColony(null, buildable, null, null);
        }
        public bool SetShipDesign(ShipDesign shipDesign)
        {
            return SetColony(null, null, shipDesign, null);
        }
        public bool SetColony(Colony colony, Buildable buildable, ShipDesign shipDesign, bool? addProd)
        {
            this.colony = colony;
            this.buildable = buildable ?? (colony != null ? colony.CurBuild : null);
            this.shipDesign = shipDesign ?? (this.buildable is BuildShip ? ((BuildShip)this.buildable).ShipDesign : null);
            this.addProd = addProd;
            return RefreshBuildable();
        }

        public bool RefreshBuildable()
        {
            SetVisibility(false);

            ShipDesign design = GetShipDesign();
            if (design != null)
            {
                SetVisibility(true);

                this.label1.Text = "Production";
                this.label2.Text = "Research";
                this.label3.Text = "Attack";
                this.label4.Text = "Defense";
                this.label5.Text = "HP";
                this.label6.Text = "Speed";
                this.label7.Text = "Upkeep";
                this.label8.Text = "Transport";

                this.lblTop.Text = design.ToString();
                this.lblInf1.Text = design.Cost.ToString();
                this.lblInf2.Text = design.Research.ToString();
                this.lblInf3.Text = design.Att.ToString();
                this.lblInf4.Text = design.Def.ToString();
                this.lblInf5.Text = design.HP.ToString();
                this.lblInf6.Text = design.Speed.ToString();
                this.lblInf7.Text = design.Upkeep.ToString();
                this.lblInf8.Text = design.Trans.ToString();
                this.lblBottom.Text = GetBottomText(design);

                return true;
            }
            else
            {
                colony.GetInfrastructure(this.addProd, out Dictionary<Ship, double> rs,
                        out double att, out double def, out double hp, out double soldiers);
                bool pd = (att > Consts.FLOAT_ERROR_ZERO || def > Consts.FLOAT_ERROR_ZERO || hp > Consts.FLOAT_ERROR_ZERO);
                double repair = rs.Values.Sum();
                bool any = (repair > Consts.FLOAT_ERROR_ZERO) || (pd) || (soldiers > Consts.FLOAT_ERROR_ZERO);
                if (any || buildable is BuildInfrastructure)
                {
                    if (colony != null && colony.Player.IsTurn)
                    {
                        SetVisibility(false);

                        int prod = colony.GetInfrastructureIncome(this.addProd);
                        colony.GetUpgMins(out int PD, out int soldier);

                        this.lblTop.Text = "Infrastructure";

                        this.label1.Text = "Production";
                        this.label2.Text = "PD Prod";
                        this.label3.Text = "Sldr Prod";
                        this.label4.Text = "Repair";
                        this.label5.Text = "Attack";
                        this.label6.Text = "Defense";
                        this.label7.Text = "HP";
                        this.label8.Text = "Soldiers";

                        this.lblInf1.Text = string.Format("{0} {1} ({2})",
                                colony.GetInfrastructureProd(this.addProd) - prod,
                                MainForm.FormatIncome(prod, false), MainForm.FormatInt(colony.GetInfrastructureIncome(false)));
                        this.lblInf2.Text = MainForm.FormatUsuallyInt(PD);
                        this.lblInf3.Text = colony.Population > 0 ? MainForm.FormatUsuallyInt(soldier) : "-";
                        this.lblInf4.Text = MainForm.FormatDouble(repair);
                        this.lblInf5.Text = MainForm.FormatIncome(att);
                        this.lblInf6.Text = MainForm.FormatIncome(def);
                        this.lblInf7.Text = MainForm.FormatIncome(hp);
                        this.lblInf8.Text = MainForm.GetBuildingSoldiers(colony, soldiers);

                        this.lblTop.Show();
                        this.label1.Show();
                        this.label2.Show();
                        this.label3.Show();
                        this.lblInf1.Show();
                        this.lblInf2.Show();
                        this.lblInf3.Show();
                        if (repair > Consts.FLOAT_ERROR_ZERO)
                        {
                            this.label4.Show();
                            this.lblInf4.Show();
                        }
                        if (pd)
                        {
                            this.label5.Show();
                            this.label6.Show();
                            this.label7.Show();
                            this.lblInf5.Show();
                            this.lblInf6.Show();
                            this.lblInf7.Show();
                        }
                        if (soldiers > Consts.FLOAT_ERROR_ZERO)
                        {
                            this.label8.Show();
                            this.lblInf8.Show();
                        }

                        return any;
                    }
                    else
                    {
                        this.lblTop.Show();
                        this.lblTop.Text = "huh???";
                        return true;
                        ////double cost = MainForm.Game.CurrentPlayer.PlanetDefenseCostPerHP;
                        ////string costLabel = handleCost(ref cost);

                        ////this.label1.Visible = true;
                        ////this.label1.Text = costLabel;
                        //this.label2.Visible = true;
                        //this.label2.Text = "Attack";
                        //this.label3.Visible = true;
                        //this.label3.Text = "Defense";

                        ////this.lblInf1.Visible = true;
                        ////this.lblInf1.Text = MainForm.FormatDouble(cost);
                        //this.lblInf2.Visible = true;
                        //this.lblInf2.Text = MainForm.Game.CurrentPlayer.PlanetDefenseAtt.ToString();
                        //this.lblInf3.Visible = true;
                        //this.lblInf3.Text = MainForm.Game.CurrentPlayer.PlanetDefenseDef.ToString();
                    }
                }
            }

            return false;
        }

        private string handleCost(ref double cost)
        {
            string costLabel = "Cost / HP";
            if (cost < .2)
            {
                costLabel = "HP / Cost";
                cost = 1 / cost;
            }
            return costLabel;
        }

        private void SetVisibility(bool visible)
        {
            foreach (Control control in this.Controls)
                if (visible)
                    control.Show();
                else
                    control.Hide();
        }

        private static string GetBottomText(ShipDesign design)
        {
            return (design.Colony ? "Colony Ship (" + MainForm.FormatDouble(
                    design.GetColonizationValue(MainForm.Game)) + ")"
                    : (design.DeathStar ? "Death Star (" + MainForm.FormatInt(design.BombardDamage) + ")" : string.Empty));
        }

        private ShipDesign GetShipDesign()
        {
            return this.shipDesign;
        }

        private void label_MouseClick(object sender, MouseEventArgs e)
        {
            ShipDesign design = GetShipDesign();
            if (design != null && e.Button == MouseButtons.Right)
                CostCalculatorForm.ShowForm(design);
        }
    }
}
