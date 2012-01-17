using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin
{
    public partial class InvadeCalculatorForm : Form
    {
        private static InvadeCalculatorForm form;

        private MainForm gameForm;

        private InvadeCalculatorForm()
        {
            InitializeComponent();

            double shipTrans = 0, totPop = 0, totSoldiers = 0, planetQuality = 0, count = 0;

            foreach (Player p in MainForm.Game.GetPlayers())
                foreach (Ship s in p.GetShips())
                {
                    ++count;
                    shipTrans += s.MaxPop;
                    totPop += s.Population;
                    totSoldiers += s.TotalSoldiers;
                }
            if (count > 0)
                shipTrans /= count;

            count = 0;
            ReadOnlyCollection<Planet> planets = MainForm.Game.GetPlanets();
            foreach (Planet p in planets)
            {
                if (p.Colony != null)
                {
                    ++count;
                    totPop += p.Colony.Population;
                    totSoldiers += p.Colony.TotalSoldiers;
                }
                planetQuality += p.PlanetValue * 2;
            }
            totPop /= count;
            totSoldiers /= count;
            planetQuality /= planets.Count;

            count = ShipDesign.GetTransStr(MainForm.Game.ExpResearch);
            if (shipTrans == 0)
                shipTrans = count;
            else
                shipTrans = ( shipTrans + 2.0 * count ) / 3.0;
            totPop = ( 5.0 * totPop + planetQuality ) / 6.0;

            SetRand(this.nudTroops, shipTrans, true);
            SetRand(this.nudPop, totPop, true);
            SetRand(this.nudAttSoldiers, PopCarrier.GetMoveSoldiers(Game.Random.Round(totPop), totSoldiers, Game.Random.Round(shipTrans))
                    * 100.0 / (double)this.nudTroops.Value, false);
            SetRand(this.nudDefSoldiers, totSoldiers * 100.0 / (double)this.nudPop.Value, false);
        }

        private void SetRand(NumericUpDown nud, double avg, bool pop)
        {
            if (!pop)
                avg *= 10;
            decimal value = Game.Random.GaussianCappedInt(avg, .6);
            if (!pop)
                value /= 10;
            if (value < nud.Minimum)
                value = nud.Minimum;
            else if (value > nud.Maximum)
                value = nud.Maximum;
            nud.Value = value;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            int troops = (int)this.nudTroops.Value;
            int pop = (int)this.nudPop.Value;
            if (Sliders.SliderForm.ShowDialog(gameForm, new Sliders.Invade(troops, troops, pop,
                    GetSoldiers(troops, this.nudAttSoldiers.Value), GetSoldiers(pop, this.nudDefSoldiers.Value))) > 0)
                this.DialogResult = DialogResult.OK;
        }

        private double GetSoldiers(int pop, decimal soldPct)
        {
            return (double)( pop * soldPct / 100 );
        }

        public static void ShowDialog(MainForm gameForm)
        {
            if (form == null)
                form = new InvadeCalculatorForm();

            form.gameForm = gameForm;
            gameForm.SetLocation(form);
            form.ShowDialog();
        }
    }
}
