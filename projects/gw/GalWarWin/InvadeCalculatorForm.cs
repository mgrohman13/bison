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

        private InvadeCalculatorForm()
        {
            InitializeComponent();

            SetRand();
        }

        private void SetRand()
        {
            float shipTrans = 0, totPop = 0, totSoldiers = 0, planetQuality = 0, count = 0;

            foreach (Player p in MainForm.Game.GetPlayers())
                foreach (Ship s in p.GetShips())
                {
                    ++count;
                    shipTrans += s.MaxPop;
                    totPop += s.Population;
                    totSoldiers += (float)s.Soldiers;
                }
            if (count > 0)
                shipTrans /= count;

            count = 0;
            HashSet<Planet> planets = MainForm.Game.GetPlanets();
            foreach (Planet p in planets)
            {
                if (p.Colony != null)
                {
                    ++count;
                    totPop += p.Colony.Population;
                    totSoldiers += (float)p.Colony.Soldiers;
                }
                planetQuality += (float)( p.PlanetValue * 2 );
            }
            totPop /= count;
            totSoldiers /= count;
            planetQuality /= planets.Count;

            count = (float)ShipDesign.GetTransStr(MainForm.Game.CurrentPlayer.GetLastResearched());
            if (shipTrans == 0)
                shipTrans = count;
            else
                shipTrans = ( shipTrans + 2f * count ) / 3f;
            totPop = ( 5f * totPop + planetQuality ) / 6f;

            SetRand(this.nudTroops, shipTrans, true);
            SetRand(this.nudPop, totPop, true);
            SetRand(this.nudAttSoldiers, GetSoldiers((float)PopCarrier.GetMoveSoldiers(
                    Game.Random.Round(totPop), totSoldiers, Game.Random.Round(shipTrans)), this.nudTroops), false);
            SetRand(this.nudDefSoldiers, GetSoldiers(totSoldiers, this.nudPop), false);
        }

        private float GetSoldiers(float soldiers, NumericUpDown nud)
        {
            return ( soldiers * 100f / (float)nud.Value );
        }

        private void SetRand(NumericUpDown nud, float avg, bool pop)
        {
            if (!pop)
                avg *= 10;
            decimal value = Game.Random.GaussianCappedInt(avg, .6f);
            if (!pop)
                value /= 10;
            CombatForm.SetValue(nud, value);
        }

        private void SetValues(Ship ship, Colony colony)
        {
            if (ship != null)
            {
                CombatForm.SetValue(this.nudTroops, ship.Population);
                CombatForm.SetValue(this.nudPop, colony.Population);
                CombatForm.SetValue(this.nudAttSoldiers, GetSoldiers(ship));
                CombatForm.SetValue(this.nudDefSoldiers, GetSoldiers(colony));
            }
        }

        private decimal GetSoldiers(PopCarrier popCarrier)
        {
            return Game.Random.Round((float)( popCarrier.GetSoldierPct() * 1000 )) / 10m;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            int troops = (int)this.nudTroops.Value;
            int pop = (int)this.nudPop.Value;
            if (Sliders.SliderForm.ShowForm(new Sliders.Invade(troops, pop,
                    GetSoldiers(troops, this.nudAttSoldiers.Value), GetSoldiers(pop, this.nudDefSoldiers.Value))) > 0)
                this.DialogResult = DialogResult.OK;
        }

        private double GetSoldiers(int pop, decimal soldPct)
        {
            return (double)( pop * soldPct / 100 );
        }

        public static void ShowForm()
        {
            ShowForm(null, null);
        }

        internal static void ShowForm(Ship ship, Colony colony)
        {
            if (form == null)
                form = new InvadeCalculatorForm();

            MainForm.GameForm.SetLocation(form);

            form.SetValues(ship, colony);

            form.ShowDialog();
        }
    }
}
