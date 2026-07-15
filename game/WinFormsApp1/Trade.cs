using ClassLibrary1;
using ClassLibrary1.Pieces;
using System;
using System.Windows.Forms;
using static ClassLibrary1.Map.Map;

namespace WinFormsApp1
{
    public partial class Trade : Form
    {
        private readonly static Trade form = new();

        public Trade()
        {
            InitializeComponent();
        }

        public static bool ShowTrade(Tile selected, bool replace, Piece replacePiece, bool build,
            bool upgrade, int upgEnergy, int upgMass)
        {
            form.pnlBurn.Visible = Program.Game.Player.CanBurnMass();
            form.pnlFabricate.Visible = Program.Game.Player.CanFabricateMass();
            form.pnlScrap.Visible = Program.Game.Player.CanScrapResearch();

            form.nudBurn.Value = 0;
            form.nudBurn.Maximum = Math.Max(0, Program.Game.Player.Mass / Consts.BurnMassPerEnergy);
            form.nudFabricate.Value = 0;
            form.nudFabricate.Maximum = Math.Max(0, Program.Game.Player.Energy / Consts.EnergyPerFabricateMass);
            form.nudScrap.Value = 0;
            form.nudScrap.Increment = Consts.MassForScrapResearch;
            form.nudScrap.Maximum = Program.Game.Player.Research.GetProgress(Program.Game.Player.Research.Researching) * Consts.MassForScrapResearch;

            SetTrade(selected, replace, replacePiece, build, upgrade, upgEnergy, upgMass);

            if (form.ShowDialog() == DialogResult.OK)
            {
                Program.Game.Player.Trade((int)form.nudBurn.Value, (int)form.nudFabricate.Value, GetResearch());
                return true;
            }
            return false;
        }
        private static void SetTrade(Tile selected, bool replace, Piece replacePiece, bool build,
            bool upgrade, int upgEnergy, int upgMass)
        {
            int energy = 0, mass = 0;
            if (replace)
            {
                Program.BuildForm.ReplaceCost(selected, replacePiece, out energy, out mass);
            }
            else if (build)
            {
                Program.BuildForm.BuilderCost(selected, out energy, out mass);
            }
            else if (upgrade)
            {
                energy = upgEnergy;
                mass = upgMass;
            }

            energy -= Program.Game.Player.Energy;
            mass -= Program.Game.Player.Mass;

            SetTrade(energy, mass);
        }
        private static void SetTrade(int energy, int mass)
        {
            if (energy > 0 && Program.Game.Player.CanBurnMass())
            {
                SetTrade(form.nudBurn, energy, 1);
            }
            if (mass > 0)
            {
                if (Program.Game.Player.CanScrapResearch())
                {
                    SetTrade(form.nudScrap, mass, Consts.MassForScrapResearch);
                    mass -= (int)form.nudScrap.Value;
                }
                if (mass > 0 && energy <= 0 && Program.Game.Player.CanFabricateMass())
                {
                    SetTrade(form.nudFabricate, mass, 1);
                }
            }
        }
        private static void SetTrade(NumericUpDown nud, int needed, double per)
        {
            int value = (int)(Math.Ceiling(needed / per) * per);
            nud.Value = Math.Min(nud.Maximum, value);
        }

        private void NUD_ValueChanged(object sender, EventArgs e)
        {
            SetLext(lblBurn, form.nudBurn.Value * Consts.BurnMassPerEnergy);
            SetLext(lblFabricate, form.nudFabricate.Value * Consts.EnergyPerFabricateMass);
            SetLext(lblScrap, GetResearch());
        }
        private static int GetResearch() => (int)Math.Ceiling(form.nudScrap.Value / Consts.MassForScrapResearch);
        private static void SetLext(Label label, decimal value)
        {
            label.Text = ((int)(-value)).ToString();
        }
    }
}
