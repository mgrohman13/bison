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
        public BuildableControl()
        {
            InitializeComponent();
        }

        public void SetBuildable(Buildable buildable)
        {
            foreach (Control control in this.Controls)
                control.Visible = false;

            ShipDesign design = buildable as ShipDesign;
            if (design != null)
            {
                foreach (Control control in this.Controls)
                    control.Visible = true;

                this.label1.Text = "Production";
                this.label8.Text = "Research";
                this.label2.Text = "Attack";

                this.lblAtt.Text = design.Att.ToString();
                this.lblColony.Text = GetColonyText(design);
                this.lblCost.Text = design.Cost.ToString();
                this.lblDef.Text = design.Def.ToString();
                this.lblHP.Text = design.HP.ToString();
                this.lblName.Text = design.ToString();
                this.lblResearch.Text = design.Research.ToString();
                this.lblSpeed.Text = design.Speed.ToString();
                this.lblTrans.Text = design.Trans.ToString();
                this.lblUpkeep.Text = design.Upkeep.ToString();
            }
            else
            {
                PlanetDefense planetDefense = buildable as PlanetDefense;
                if (planetDefense != null)
                {
                    this.lblName.Visible = true;
                    this.label1.Visible = true;
                    this.lblCost.Visible = true;
                    this.label8.Visible = true;
                    this.lblResearch.Visible = true;
                    this.label2.Visible = true;
                    this.lblAtt.Visible = true;

                    this.label1.Text = "Cost";
                    this.label8.Text = "Attack";
                    this.label2.Text = "Defense";

                    this.lblName.Text = "Planetary Defenses";
                    this.lblCost.Text = MainForm.FormatDouble(planetDefense.HPCost);
                    this.lblResearch.Text = planetDefense.Att.ToString();
                    this.lblAtt.Text = planetDefense.Def.ToString();
                }
            }
        }

        private static string GetColonyText(ShipDesign design)
        {
            return ( design.Colony ? "Colony Ship (" + MainForm.FormatDouble(GetColonyValue(design)) + ")" : ( design.DeathStar ?
                    "Death Star (" + MainForm.FormatDouble(design.BombardDamage) + ")" : string.Empty ) );
        }

        private static double GetColonyValue(ShipDesign design)
        {
            return ShipDesign.GetColonizationValue(design.Speed, design.Cost, design.HP, design.HP);
        }
    }
}
