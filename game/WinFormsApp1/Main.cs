using ClassLibrary1;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LogEntry = ClassLibrary1.Log.LogEntry;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace WinFormsApp1
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            this.MouseWheel += Main_MouseWheel;
            this.Bounds = Screen.FromControl(this).WorkingArea;
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            Program.RefreshChanged();
            Invalidate();
            mapMain.Invalidate();

            //Debug.WriteLine(ClientSize);
        }

        public Map MapMain => mapMain;
        public Map MapMini => mapMini;
        public Info Info => infoMain;

        public override void Refresh()
        {
            Program.Game.Player.GetIncome(out double energyInc, out double massInc, out int researchInc, out double researchAvg);
            //energyInc -= energyUpk;
            //massInc -= massUpk; 
            Format(this.lblEnergy, Program.Game.Player.Energy);
            FormatInc(lblEnergyInc, energyInc);
            Format(this.lblMass, Program.Game.Player.Mass);
            FormatInc(lblMassInc, massInc);
            this.lblResearch.Text = string.Format("{0} / {1}", Program.Game.Player.Research.GetProgress(Program.Game.Player.Research.Researching),
                Program.Game.Player.Research.GetCost(Program.Game.Player.Research.Researching));
            FormatInc(lblResearchInc, researchInc, "0");
            lblResearching.Text = $"({researchAvg:0.0})      {Program.Game.Player.Research.Researching}";
            base.Refresh();
        }
        private static void FormatInc(Label label, double inc, string format = "0.0") =>
            Format(label, Math.Sign(inc), string.Format("{0}{1}", inc >= 0 ? "+" : "", inc.ToString(format)));
        private static void Format(Label label, int value, string text = null)
        {
            label.ForeColor = value >= 0 ? Color.Black : Color.Red;
            label.Text = text ?? value.ToString();
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            if (e.KeyCode == Keys.Escape)
                Program.EndTurn();
            else if (e.KeyCode == Keys.Space)
                Program.Hold();
            else if (e.KeyCode == Keys.Q)
                Program.Next(false);
            else if (e.KeyCode == Keys.E)
                Program.Next(true);
            else if (e.KeyCode == Keys.B)
                Info.BtnBuild_Click(sender, e);
            else if (e.KeyCode == Keys.T)
                Info.BtnViewAtt_Click(sender, e);
            else if (e.KeyCode == Keys.Z)
                Program.Sleep();
            else
                e.SuppressKeyPress = false;

            mapMain.Map_KeyDown(sender, e);
        }
        private void Main_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (e.KeyChar == ' ')
            //    e.su
            //    e.Handled = true;
        }

        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            mapMain.Map_KeyUp(sender, e);
        }

        private void Main_MouseWheel(object sender, MouseEventArgs e)
        {
            mapMain.Map_MouseWheel(sender, e);
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.SaveGame();
        }

        private LogEntry lastLog = null;
        internal void UpdateProgress(Tile center, double progress)
        {
            bool visible = progress >= 0 && progress <= 1;

            mapMain.ToggleEnemyTurn(visible);

            Debug.WriteLine($"UpdateProgress: {progress}");
            if (visible)
            {
                progressBar1.Value = Game.Rand.Round(progress * progressBar1.Maximum);

                var map = mapMain.Bounds;
                double w = map.Width;// / 2.0;
                double h = progressBar1.Height;
                double x = map.X;// + (map.Width - w) / 2.0;
                double y = map.Y + (map.Height - h);// / 2.0;
                progressBar1.Bounds = new((int)x, (int)y, (int)w, (int)h);

                if (center != null)
                    mapMain.Center(center);
                Application.DoEvents();
                this.Refresh();

                var curLog = Program.Game.Log.Data(null).FirstOrDefault();
                if (lastLog != curLog)
                {
                    lastLog = curLog;
                    Info.RefreshLog();
                }

                DisableButtons(this, true);
            }
            else
            {
                DisableButtons(this, false);
            }
            progressBar1.Visible = visible;
        }
        private static void DisableButtons(Control control, bool disabled)
        {
            foreach (Control c in control.Controls)
            {
                if (c is Button)
                    c.Enabled = !disabled;
                DisableButtons(c, disabled);
            }
        }

        private void LblResearching_Click(object sender, EventArgs e)
        {
            if (ResearchForm.ShowForm())
                Program.RefreshChanged();
        }
        private void LblResources_Click(object sender, EventArgs e)
        {
            DataTable data = new()
            {
                Columns = { { "Category", typeof(string) }, { "Count", typeof(string) }, { "Energy +", typeof(string) }, { "Energy -", typeof(string) },
                    { "Mass +", typeof(string) }, { "Mass -", typeof(string) },  { "Research +", typeof(string) },  { "Research -", typeof(string) }, },
            };

            Dictionary<Type, double[]> income = Program.Game.Player.GetIncomeDetails();
            foreach (var pair in income.OrderByDescending(p => p.Value.Sum()))
                data.Rows.Add(pair.Key.Name, pair.Value[0], pair.Value[1].ToString("0.0"), pair.Value[2].ToString("0.0"),
                    pair.Value[3].ToString("0.0"), pair.Value[4].ToString("0.0"), pair.Value[5].ToString("0.0"), pair.Value[6].ToString("0.0"));

            DgvForm.ShowData("Resources per Turn", data);
        }
    }
}
