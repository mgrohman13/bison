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
                this.lblUpkeep.Text = MainForm.FormatDouble(colony.PlanetDefenseUpkeep);

                if (colony.Buildable is PlanetDefense)
                {
                    this.lblInc.Visible = true;
                    this.lblIncL.Visible = true;

                    double d1 = 0, production = 0, d2 = 0;
                    int i = 0;
                    colony.GetTurnIncome(ref d1, ref production, ref d2, ref i, false);
                    this.lblInc.Text = MainForm.GetBuildingDefense(colony, colony.Buildable, colony.Production + production);
                }
            }
            else if (colony.DefenseAttChange != 0 || colony.DefenseDefChange != 0 || colony.DefenseHPChange != 0)
            {
                this.lblUpkeep.Visible = true;
                this.lblUpkeepL.Visible = true;

                this.lblUpkeepL.Text = "Change";
                this.lblUpkeep.Text = MainForm.GetBuildingDefense(colony, colony.DefenseAttChange, colony.DefenseDefChange, colony.DefenseHPChange);
            }
        }

        public static void ShowForm(Colony colony)
        {
            MainForm.GameForm.SetLocation(form);

            form.SetColony(colony);
            form.ShowDialog();
        }

        private void btnDisband_Click(object sender, EventArgs e)
        {
            int sell = Sliders.SliderForm.ShowForm(new Sliders.SellPlanetDefense(colony));
            if (sell > 0)
            {
                colony.DisbandPlanetDefense(MainForm.GameForm, sell, Sliders.SellPlanetDefense.Gold);
                MainForm.GameForm.RefreshAll();
                SetColony(colony);
            }
        }
    }
}
