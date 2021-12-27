using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassLibrary1;

namespace WinFormsApp1
{
    public partial class Trade : Form
    {
        private readonly static Trade form = new();

        public Trade()
        {
            InitializeComponent();
        }

        public static bool ShowTrade()
        {
            form.pnlBurn.Visible = Program.Game.Player.CanBurnMass();
            form.pnlFabricate.Visible = Program.Game.Player.CanFabricateMass();
            form.pnlScrap.Visible = Program.Game.Player.CanScrapResearch();

            form.nudBurn.Value = 0;
            form.nudBurn.Maximum = Program.Game.Player.Mass / Consts.BurnMassForEnergy;
            form.nudFabricate.Value = 0;
            form.nudFabricate.Maximum = Program.Game.Player.Energy / Consts.EnergyForFabricateMass;
            form.nudScrap.Value = 0;
            form.nudScrap.Increment = Consts.MassForScrapResearch;
            form.nudScrap.Maximum = Program.Game.Player.Research.GetProgress(Program.Game.Player.Research.Researching) * Consts.MassForScrapResearch;

            if (form.ShowDialog() == DialogResult.OK)
            {
                Program.Game.Player.Trade((int)form.nudBurn.Value, (int)form.nudFabricate.Value, (int)form.nudScrap.Value / Consts.MassForScrapResearch);
                return true;
            }
            return false;
        }

        private void NUD_ValueChanged(object sender, EventArgs e)
        {
            SetLext(lblBurn, form.nudBurn.Value * Consts.BurnMassForEnergy);
            SetLext(lblFabricate, form.nudFabricate.Value * Consts.EnergyForFabricateMass);
            SetLext(lblScrap, form.nudScrap.Value / Consts.MassForScrapResearch);
        }
        private static void SetLext(Label label, decimal value)
        {
            label.Text = ((int)(-value)).ToString();
        }
    }
}
