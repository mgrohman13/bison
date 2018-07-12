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
    public partial class ColoniesForm : Form
    {
        private static ColoniesForm form = new ColoniesForm();

        private Control[,] controls;

        public ColoniesForm()
        {
            InitializeComponent();
        }

        public static void ShowForm()
        {
            form.LoadData();

            form.Width = form.controls[form.controls.GetLength(0) - 1, 0].Right + 21;
            form.Height = form.controls[0, form.controls.GetLength(1) - 1].Bottom + 91;
            MainForm.GameForm.SetLocation(form);

            form.ShowDialog();
        }

        private const int width = 118, inc = width + 6;
        private void LoadData()
        {
            if (controls != null)
                for (int a = 0 ; a < controls.GetLength(0) ; ++a)
                    for (int b = 0 ; b < controls.GetLength(1) ; ++b)
                    {
                        Controls.Remove(controls[a, b]);
                        controls[a, b] = null;
                    }

            var colonies = MainForm.Game.CurrentPlayer.GetColonies()
                    .OrderBy(colony => colony.Tile.Y).ThenBy(colony => colony.Tile.X);

            controls = new Control[8, colonies.Count()];
            int y = 33, i = 0;
            foreach (Colony colony in colonies)
            {
                int x = 6;
                Loc(x, y, i, colony);

                x += inc;
                Quality(x, y, i, colony);

                x += inc;
                Population(x, y, i, colony);

                x += inc;
                Defense(x, y, i, colony);

                x += inc;
                Income(x, y, i, colony);

                x += inc;
                Building(x, y, i, colony);

                x += inc;
                Production(x, y, i, colony);

                x += inc;
                ProdButton(x, y, i, colony);

                y += 26;
                ++i;
            }
        }

        private void Loc(int x, int y, int i, Colony colony)
        {
            Label location = (Label)( controls[0, i] = NewLabel(x, y, colony.Tile.GetLoction(),
                    click: true, bold: colony.Tile == MainForm.GameForm.selected) );
            location.Click += new EventHandler((sender, e) =>
            {
                MainForm.GameForm.SelectTile(colony.Tile);
                MainForm.GameForm.Center();
                MainForm.GameForm.RefreshAll();
                Close();
            });
        }

        private void Quality(int x, int y, int i, Colony colony)
        {
            Label quality = (Label)( controls[1, i] = NewLabel(x, y, colony.Planet.Quality.ToString(), bold: true) );
            double pct = colony.Population / (double)colony.Planet.Quality;
            if (pct < 1)
                quality.Text += " (" + MainForm.FormatPctWithCheck(pct) + ")";
        }

        private void Population(int x, int y, int i, Colony colony)
        {
            Label pop = (Label)( controls[2, i] = NewLabel(x, y, big: true) );
            MainForm.FormatIncome(pop, colony.GetPopulationGrowth(), true);
            if (pop.Text == "0.0")
                pop.Text = string.Empty;
            pop.Text = colony.Population.ToString() + " " + pop.Text + " (" + MainForm.FormatPct(colony.GetSoldierPct()) + ")";
        }

        private void Defense(int x, int y, int i, Colony colony)
        {
            Label defense = (Label)( controls[3, i] = NewLabel(x, y, click: !colony.MinDefenses, bold: colony.CurBuild is PlanetDefense) );
            if (!colony.MinDefenses)
            {
                defense.Text = string.Format("{0} : {1}   ({2})", colony.Att, colony.Def, colony.HP);
                defense.Click += new EventHandler((sender, e) =>
                {
                    if (PlanetDefenseForm.ShowForm(colony))
                        LoadData();
                });
            }
        }

        private void Income(int x, int y, int i, Colony colony)
        {
            double population = 0, production = 0, gold = 0;
            int research = 0;
            colony.GetTurnIncome(ref population, ref production, ref gold, ref research, false);
            Label inc = (Label)( controls[4, i] = NewLabel(x, y, click: true) );
            MainForm.FormatIncome(inc, production + gold + research, true);
            inc.Text += " (" + colony.GetProductionIncome() + ")";
            inc.Click += new System.EventHandler((object sender, EventArgs e) => LabelsForm.ShowColonyIncome(colony));
        }

        private void Building(int x, int y, int i, Colony colony)
        {
            string buildText;
            Ship repairShip = colony.RepairShip;
            if (repairShip != null)
                buildText = "Repair +" + MainForm.FormatDouble(repairShip.GetHPForProd(colony.GetProductionIncome()));
            else if (colony.CurBuild != null)
                buildText = colony.CurBuild.ToString();
            else
                buildText = "Gold";
            bool ship = ( colony.CurBuild is build );
            Label build = (Label)( controls[5, i] = NewLabel(x, y, buildText, big: true, click: ship, bold: ship && !colony.PauseBuild
                    && ( colony.Production + colony.GetAfterRepairProdInc() * Consts.FLOAT_ERROR_ONE ) >= colony.CurBuild.Cost) );
            if (ship)
                build.Click += new EventHandler((sender, e) => CostCalculatorForm.ShowForm(colony.CurBuild as ShipDesign));
        }

        private void Production(int x, int y, int i, Colony colony)
        {
            controls[6, i] = NewLabel(x, y, MainForm.GetProdText(colony));
        }

        private void ProdButton(int x, int y, int i, Colony colony)
        {
            Button button = (Button)( controls[7, i] = new Button() );
            button.Location = new Point(x, y);
            button.Size = new Size(width, 23);
            button.Text = "Production";
            Controls.Add(button);
            button.Click += new System.EventHandler((object sender, EventArgs e) =>
            {
                Buildable buildable;
                bool pause, ok;
                int prod = colony.Production;
                ok = ProductionForm.ShowForm(colony, out buildable, out pause);
                colony.StartBuilding(MainForm.GameForm, buildable, pause);
                if (ok || prod != colony.Production)
                    LoadData();
            });
        }

        private Label NewLabel(int x, int y, string text = "", bool bold = false, bool big = false, bool click = false)
        {
            Label label = GraphsForm.NewLabel(this.Controls, x, y, text, Color.Transparent, bold);
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Width = width;
            if (click)
                label.BorderStyle = BorderStyle.FixedSingle;
            return label;
        }
    }
}
