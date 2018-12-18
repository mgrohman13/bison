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
    public partial class PlanetDefenseForm : Form
    {
        private static PlanetDefenseForm form = new PlanetDefenseForm();

        private Colony colony;
        private bool sold = false;

        private PlanetDefenseForm()
        {
            InitializeComponent();
        }

        private void SetColony(Colony colony)
        {
            this.colony = colony;

            this.btnDisband.Visible = false;
            this.lblUpkeep.Visible = false;
            this.lblUpkeepL.Visible = false;
            this.lblInc.Visible = false;
            this.lblIncL.Visible = false;

            if (colony.MinDefenses)
            {
                this.lblAtt.Text = "0";
                this.lblDef.Text = "0";
            }
            else
            {
                this.lblAtt.Text = colony.Att.ToString();
                this.lblDef.Text = colony.Def.ToString();
            }
            this.lblHP.Text = colony.HP.ToString();

            if (colony.Player.IsTurn)
            {
                this.btnDisband.Visible = true;
                this.lblUpkeep.Visible = true;
                this.lblUpkeepL.Visible = true;

                this.lblUpkeepL.Text = "Upkeep";
                this.lblUpkeep.Text = MainForm.FormatDouble(colony.PDUpkeep);

                if (colony.CurBuild is PlanetDefense)
                {
                    this.lblInc.Visible = true;
                    this.lblIncL.Visible = true;

                    double d1 = 0, production = 0, d2 = 0;
                    int i = 0;
                    colony.GetTurnIncome(ref d1, ref production, ref d2, ref i, false);
                    this.lblInc.Text = MainForm.GetBuildingDefense(colony, colony.CurBuild, colony.CurBuild.Production + production);
                }
            }
            else
            {
                this.lblUpkeep.Visible = true;
                this.lblUpkeepL.Visible = true;

                this.lblUpkeepL.Text = "Cost";
                this.lblUpkeep.Text = MainForm.FormatDouble(colony.PDCostAvgResearch);

                if (colony.DefenseAttChange != 0 || colony.DefenseDefChange != 0 || colony.DefenseHPChange != 0)
                {
                    this.lblInc.Visible = true;
                    this.lblIncL.Visible = true;

                    this.lblIncL.Text = "Change";
                    this.lblInc.Text = MainForm.GetBuildingDefense(colony, colony.DefenseAttChange, colony.DefenseDefChange, colony.DefenseHPChange);
                }
            }
        }

        public static bool ShowForm(Colony colony)
        {
            MainForm.GameForm.SetLocation(form);

            form.SetColony(colony);
            form.sold = false;
            form.ShowDialog();
            return form.sold;
        }

        private void btnDisband_Click(object sender, EventArgs e)
        {
            Sliders.SellPlanetDefense slider = new Sliders.SellPlanetDefense(colony);
            int sell = Sliders.SliderForm.ShowForm(slider);
            if (sell > -1)
            {
                colony.DisbandPlanetDefense(MainForm.GameForm, sell, slider.Gold);
                MainForm.GameForm.RefreshAll();
                SetColony(colony);
                sold = true;
            }
        }
    }
}
