using System;
using System.Collections.Generic;
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
        private int prodLoss;

        public BuildableControl()
        {
            InitializeComponent();
        }

        public bool SetColony(Colony colony)
        {
            return SetColony(colony, colony.CurBuild, 0);
        }
        public bool SetBuildable(Buildable buildable)
        {
            return SetColony(null, buildable, 0);
        }
        public bool SetColony(Colony colony, Buildable buildable, int prodLoss)
        {
            this.colony = colony;
            this.buildable = buildable;
            this.prodLoss = prodLoss;
            return RefreshBuildable();
        }

        public bool RefreshBuildable()
        {
            return RefreshBuildable(0);
        }
        public bool RefreshBuildable(double buyProd)
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
                PlanetDefense planetDefense = buildable as PlanetDefense;
                if (planetDefense != null)
                {
                    if (colony != null && colony.Player.IsTurn)
                    {
                        SetVisibility(true);

                        double att, def, hp, newResearch, newProd;
                        colony.GetPlanetDefenseInc(colony.CurBuild.Production - prodLoss + buyProd + colony.GetAfterRepairProdInc(), MainForm.Game.CurrentPlayer.GetCurrentResearch(),
                                out att, out def, out hp, out newResearch, out newProd, false, false);
                        double cost = ShipDesign.GetPlanetDefenseCost(att, def, MainForm.Game.CurrentPlayer.GetCurrentResearch());
                        string costLabel = handleCost(ref cost);
                        //if (colony.Population > 1)
                        //    soldiers /= colony.Population;

                        this.label1.Text = "Attack";
                        this.label2.Text = "Defense";
                        this.label3.Text = "HP";
                        this.label4.Text = costLabel;
                        this.label5.Visible = false;
                        this.label6.Visible = false;
                        this.label7.Text = "Max Att";
                        this.label8.Text = "Max Def";

                        this.lblTop.Text = "Planetary Defenses";
                        this.lblInf1.Text = MainForm.FormatUsuallyInt(att);
                        this.lblInf2.Text = MainForm.FormatUsuallyInt(def);
                        this.lblInf3.Text = MainForm.FormatDouble(hp);
                        this.lblInf4.Text = MainForm.FormatDouble(cost);
                        this.lblInf5.Visible = false;
                        this.lblInf6.Visible = false;
                        this.lblInf7.Text = MainForm.Game.CurrentPlayer.PlanetDefenseAtt.ToString();
                        this.lblInf8.Text = MainForm.Game.CurrentPlayer.PlanetDefenseDef.ToString();
                        this.lblBottom.Visible = false;
                    }
                    else
                    {
                        //double cost = MainForm.Game.CurrentPlayer.PlanetDefenseCostPerHP;
                        //string costLabel = handleCost(ref cost);

                        //this.label1.Visible = true;
                        //this.label1.Text = costLabel;
                        this.label2.Visible = true;
                        this.label2.Text = "Attack";
                        this.label3.Visible = true;
                        this.label3.Text = "Defense";

                        //this.lblInf1.Visible = true;
                        //this.lblInf1.Text = MainForm.FormatDouble(cost);
                        this.lblInf2.Visible = true;
                        this.lblInf2.Text = MainForm.Game.CurrentPlayer.PlanetDefenseAtt.ToString();
                        this.lblInf3.Visible = true;
                        this.lblInf3.Text = MainForm.Game.CurrentPlayer.PlanetDefenseDef.ToString();
                    }

                    return true;
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
                control.Visible = visible;
        }

        private static string GetBottomText(ShipDesign design)
        {
            return ( design.Colony ? "Colony Ship (" + MainForm.FormatDouble(
                    design.GetColonizationValue(MainForm.Game)) + ")"
                    : ( design.DeathStar ? "Death Star (" + MainForm.FormatInt(design.BombardDamage) + ")" : string.Empty ) );
        }

        private ShipDesign GetShipDesign()
        {
            return GetShipDesign(this.buildable);
        }
        public static ShipDesign GetShipDesign(Colony colony)
        {
            return GetShipDesign(( colony != null && colony.Player.IsTurn ) ? colony.CurBuild : null);
        }
        public static ShipDesign GetShipDesign(Buildable buildable)
        {
            return ( buildable is BuildShip ? ( (BuildShip)buildable ).ShipDesign : null );
        }

        private void label_MouseClick(object sender, MouseEventArgs e)
        {
            ShipDesign design = GetShipDesign();
            if (design != null && e.Button == MouseButtons.Right)
                CostCalculatorForm.ShowForm(design);
        }
    }
}
