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

        public void SetColony(Colony colony)
        {
            SetColony(colony, colony.Buildable, 0);
        }
        public void SetBuildable(Buildable buildable)
        {
            SetColony(null, buildable, 0);
        }
        public void SetColony(Colony colony, Buildable buildable, int prodLoss)
        {
            this.colony = colony;
            this.buildable = buildable;
            this.prodLoss = prodLoss;
            RefreshBuildable();
        }

        public void RefreshBuildable()
        {
            RefreshBuildable(0);
        }
        public void RefreshBuildable(double buyProd)
        {
            ShipDesign design = buildable as ShipDesign;
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
            }
            else
            {
                PlanetDefense planetDefense = buildable as PlanetDefense;
                if (planetDefense != null)
                {
                    if (colony != null && colony.Player.IsTurn)
                    {
                        SetVisibility(true);

                        this.label1.Text = "Attack";
                        this.label2.Text = "Defense";
                        this.label3.Text = "HP";
                        this.label4.Text = "Cost / HP";
                        this.label5.Text = "Soldiers";
                        this.label6.Visible = false;
                        this.label7.Text = "Max Att";
                        this.label8.Text = "Max Def";

                        double cost, att, def, hp, soldiers;
                        colony.GetPlanetDefenseInc(buildable, colony.Production - prodLoss + buyProd + colony.GetAfterRepairProdInc(),
                                out att, out def, out hp, out soldiers);
                        cost = ShipDesign.GetPlanetDefenseCost(att, def, MainForm.Game.CurrentPlayer.GetLastResearched());
                        if (colony.Population > 1)
                            soldiers /= colony.Population;

                        this.lblTop.Text = "Planetary Defenses";
                        this.lblInf1.Text = MainForm.FormatUsuallyInt(att);
                        this.lblInf2.Text = MainForm.FormatUsuallyInt(def);
                        this.lblInf3.Text = MainForm.FormatDouble(hp);
                        this.lblInf4.Text = MainForm.FormatDouble(cost);
                        this.lblInf5.Text = MainForm.FormatPct(soldiers);
                        this.lblInf6.Visible = false;
                        this.lblInf7.Text = MainForm.Game.CurrentPlayer.PlanetDefenseAtt.ToString();
                        this.lblInf8.Text = MainForm.Game.CurrentPlayer.PlanetDefenseDef.ToString();
                        this.lblBottom.Visible = false;
                    }
                    else
                    {
                        SetVisibility(false);

                        this.label1.Visible = true;
                        this.label1.Text = "Cost / HP";
                        this.label2.Visible = true;
                        this.label2.Text = "Attack";
                        this.label3.Visible = true;
                        this.label3.Text = "Defense";

                        this.lblInf1.Visible = true;
                        this.lblInf1.Text = MainForm.FormatDouble(MainForm.Game.CurrentPlayer.PlanetDefenseCostPerHP);
                        this.lblInf2.Visible = true;
                        this.lblInf2.Text = MainForm.Game.CurrentPlayer.PlanetDefenseAtt.ToString();
                        this.lblInf3.Visible = true;
                        this.lblInf3.Text = MainForm.Game.CurrentPlayer.PlanetDefenseDef.ToString();
                    }
                }
            }
        }

        private void SetVisibility(bool visible)
        {
            foreach (Control control in this.Controls)
                control.Visible = visible;
        }

        private static string GetBottomText(ShipDesign design)
        {
            return ( design.Colony ? "Colony Ship (" + MainForm.FormatDouble(
                    design.GetColonizationValue(MainForm.Game.MapSize, MainForm.Game.CurrentPlayer.GetLastResearched())) + ")"
                    : ( design.DeathStar ? "Death Star (" + MainForm.FormatInt(design.BombardDamage) + ")" : string.Empty ) );
        }

        private void label_MouseClick(object sender, MouseEventArgs e)
        {
            ShipDesign design = buildable as ShipDesign;
            if (design != null && e.Button == MouseButtons.Right)
                CostCalculatorForm.ShowForm(design);
        }
    }
}
