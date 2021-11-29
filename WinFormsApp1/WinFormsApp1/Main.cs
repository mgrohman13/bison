using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            Refresh();
        }

        public Map MapMain => mapMain;
        public Map MapMini => mapMini;
        public Info Info => infoMain;

        public override void Refresh()
        {
            Info.Refresh();
            base.Refresh();

            Program.Game.Player.GetIncome(out double energyInc, out double energyUpk, out double massInc, out double massUpk, out double researchInc, out double researchUpk);
            energyInc -= energyUpk;
            massInc -= massUpk;
            researchInc -= researchUpk;
            this.lblEnergy.Text = Format(Program.Game.Player.Energy);
            FormatInc(lblEnergyInc, energyInc);
            this.lblMass.Text = Format(Program.Game.Player.Mass);
            FormatInc(lblMassInc, massInc);
            this.lblResearch.Text = Format(Program.Game.Player.Research);
            FormatInc(lblResearchInc, researchInc);
        }
        private static void FormatInc(Label label, double inc)
        {
            label.ForeColor = inc >= 0 ? Color.Black : Color.Red;
            label.Text = string.Format("{0}{1}", inc >= 0 ? "+" : "-", Format(inc));
        }
        private static string Format(double value)
        {
            return value.ToString("0.0");
        }

        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Program.EndTurn();
            else if (e.KeyCode == Keys.Q)
                Program.Next(false);
            else if (e.KeyCode == Keys.E)
                Program.Next(true);
        }
    }
}
