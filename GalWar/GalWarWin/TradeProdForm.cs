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
    public partial class TradeProdForm : Form
    {
        private static TradeProdForm form = new TradeProdForm();

        private Colony colony;
        private SortedSet<Buildable> buildable;

        private int height;
        private Control[] template;
        private List<BuildableRow> rows;

        private TradeProdForm()
        {
            InitializeComponent();

            height = this.Height;
            template = new Control[] { this.lblName, this.nudProd, this.lblCost, this.nudShips, this.lblInf, this.lblDiff };
            foreach (Control control in template)
                control.Hide();
            rows = new List<BuildableRow>();
        }

        private void SetColony(Colony colony, SortedSet<Buildable> buildable)
        {
            this.colony = colony;
            this.buildable = buildable;

            LoadBuilding();
            RefreshTrade(null);
        }

        private void LoadBuilding()
        {
            foreach (BuildableRow row in rows)
                foreach (Control control in row.GetControls())
                    this.Controls.Remove(control);

            rows.Clear();

            int y = -26;
            foreach (Buildable build in buildable)
            {
                BuildableRow row = new BuildableRow(template, y, nud_ValueChanged, BuildTrade, colony, build);
                rows.Add(row);
                foreach (Control control in row.GetControls())
                    this.Controls.Add(control);
                y += 26;
            }

            this.Height = height + y - 26;
        }

        private void RefreshTrade(object sender)
        {
            if (this.rows.Count == colony.Buildable.Count)
            {
                foreach (BuildableRow row in rows)
                    row.Refresh(sender);

                double gold;
                bool allow = this.colony.GetTradeProduction(BuildTrade(), out gold);
                if (gold > 0)
                    gold = Player.FloorGold(gold);
                else
                    gold = -Player.CeilGold(-gold);

                this.lblCurGold.Text = "/ " + MainForm.FormatDouble(colony.Player.Gold);
                MainForm.FormatIncome(this.lblGoldDiff, gold);

                this.btnOK.Enabled = allow;
            }
        }

        private Dictionary<Buildable, int> BuildTrade()
        {
            Dictionary<Buildable, int> trade = new Dictionary<Buildable, int>();
            foreach (BuildableRow row in rows)
                trade.Add(row.Build, row.GetDiff());
            return trade;
        }

        internal static bool ShowForm(Colony colony, SortedSet<Buildable> buildable)
        {
            MainForm.GameForm.SetLocation(form);
            form.SetColony(colony, buildable);
            bool doTrade = (form.ShowDialog() == DialogResult.OK);

            if (doTrade)
                form.colony.TradeProduction(MainForm.GameForm, form.BuildTrade());

            return doTrade;
        }

        private void lblGoldDiff_Click(object sender, EventArgs e)
        {
            Dictionary<Buildable, int> trade = BuildTrade();
            double gold;
            this.colony.GetTradeProduction(trade, out gold);
            if (!colony.Player.HasGold(-gold))
            {
                BuildableRow row = this.rows.OrderByDescending(r => r.GetDiff()).First();
                Buildable build = row.Build;
                int prod = MattUtil.TBSUtil.FindValue(value =>
                {
                    trade[build] = value - build.Production;
                    this.colony.GetTradeProduction(trade, out gold);
                    return colony.Player.HasGold(-gold);
                }, 0, build.Production + row.GetDiff(), false);
                row.setValue(prod);
            }
        }

        private void lblCurGold_Click(object sender, EventArgs e)
        {
            lblGoldDiff_Click(sender, e);
        }

        private void nud_ValueChanged(object sender, EventArgs e)
        {
            RefreshTrade(sender);
        }

        private class BuildableRow
        {
            private Colony colony;
            private Buildable build;

            private Label lblName, lblCost, lblInf, lblDiff;
            private NumericUpDown nudProd, nudShips;

            private List<Control> controls;
            private Func<Dictionary<Buildable, int>> BuildTrade;

            public Buildable Build
            {
                get
                {
                    return build;
                }
            }

            public BuildableRow(Control[] template, int y, EventHandler ValueChanged, Func<Dictionary<Buildable, int>> BuildTrade, Colony colony, Buildable build)
            {
                this.colony = colony;
                this.build = build;

                this.controls = new List<Control>();
                this.BuildTrade = BuildTrade;

                foreach (Control control in template)
                {
                    Control newControl;
                    if (control is Label)
                    {
                        Label oldLabel = (Label)control;
                        Label newLabel = new Label();

                        newLabel.AutoEllipsis = oldLabel.AutoEllipsis;
                        newLabel.Font = oldLabel.Font;
                        newLabel.TextAlign = oldLabel.TextAlign;

                        newControl = newLabel;
                    }
                    else if (control is NumericUpDown)
                    {
                        NumericUpDown oldNUD = (NumericUpDown)control;
                        NumericUpDown newNUD = new NumericUpDown();

                        newNUD.Maximum = oldNUD.Maximum;
                        newNUD.Value = oldNUD.Value;
                        newNUD.ValueChanged += ValueChanged;

                        newControl = newNUD;
                    }
                    else
                    {
                        throw new Exception();
                    }

                    newControl.Location = new Point(control.Location.X, control.Location.Y + y);
                    newControl.Name = control.Name;
                    newControl.Size = control.Size;

                    if (build is BuildGold)
                        newControl.Hide();
                    else
                        newControl.Show();

                    this.controls.Add(newControl);
                    switch (newControl.Name)
                    {
                        case "lblName":
                            this.lblName = (Label)newControl;
                            break;
                        case "lblCost":
                            this.lblCost = (Label)newControl;
                            break;
                        case "lblInf":
                            this.lblInf = (Label)newControl;
                            break;
                        case "lblDiff":
                            this.lblDiff = (Label)newControl;
                            break;
                        case "nudProd":
                            this.nudProd = (NumericUpDown)newControl;
                            break;
                        case "nudShips":
                            this.nudShips = (NumericUpDown)newControl;
                            break;
                    }
                }
                this.lblName.Click += LblName_Click;
                this.lblDiff.Click += LblDiff_Click;

                this.nudProd.Value = build.Production;

                if (!(build is BuildGold))
                {
                    this.lblCost.TextAlign = this.lblDiff.TextAlign = ContentAlignment.MiddleRight;
                    this.lblInf.Visible = (build is BuildInfrastructure);
                    this.lblCost.Visible = build.Cost.HasValue;
                    this.nudShips.Visible = build.Cost.HasValue;

                    Refresh(null);
                }
            }

            private void LblName_Click(object sender, EventArgs e)
            {
                if (build is StoreProd)
                {
                    Dictionary<Buildable, int> trade = BuildTrade();
                    int prod = MattUtil.TBSUtil.FindValue(value =>
                    {
                        trade[build] = value - build.Production;
                        double gold;
                        this.colony.GetTradeProduction(trade, out gold);
                        return (gold > -Consts.FLOAT_ERROR_ZERO);
                    }, 0, build.Production, false);
                    this.nudProd.Value = prod;
                }
                else
                {
                    this.nudProd.Value = 0;
                }
            }

            private void LblDiff_Click(object sender, EventArgs e)
            {
                this.nudProd.Value = build.Production;
            }

            public void Refresh(object sender)
            {
                int ships = (int)this.nudShips.Value;
                int prod = (int)this.nudProd.Value;

                this.lblName.Text = build.ToString();
                this.lblCost.Text = build.Cost.HasValue ? "/ " + build.Cost.Value.ToString() : string.Empty;

                this.lblInf.Text = string.Empty;
                if (build is BuildInfrastructure)
                {
                    colony.GetUpgMins(out int PD, out int soldier);
                    this.lblInf.Text = string.Format("PD: {0}  Sldr: {1}",
                            MainForm.FormatUsuallyInt(PD), MainForm.FormatUsuallyInt(soldier));
                }

                MainForm.FormatIncome(this.lblDiff, GetDiff());

                if (build.Cost.HasValue)
                {
                    if (sender == this.nudShips)
                    {
                        prod = (ships * build.Cost.Value - colony.GetProductionIncome());
                        if (prod < 0)
                            prod = 0;
                    }
                    else
                    {
                        ships = (prod + colony.GetProductionIncome()) / build.Cost.Value;
                        if (ships < 0)
                            ships = 0;
                        this.nudShips.Value = ships;
                    }
                    this.nudProd.Value = prod;
                }
            }

            public void setValue(int prod)
            {
                this.nudProd.Value = prod;
            }

            public int GetDiff()
            {
                return (int)this.nudProd.Value - this.build.Production;
            }

            public IEnumerable<Control> GetControls()
            {
                return this.controls;
            }
        }
    }
}
