using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CityWar;

namespace CityWarWinApp
{
    partial class Build : Form
    {
        //63
        int[] Xs = { 12, 108, 153, 198, 279, 375, 441, 509, 574, 630, 836, 1042, 1155 };
        int[] Ws = { 90, 39, 39, 90, 60, 30, 30, 30, 200, 200, 200 };

        Button btnTrade;

        int tabOrder = -1;

        List<Capturable> capts = new List<Capturable>();
        public Build(Game game, List<Capturable> capts, Point l, Size s)
        {
            InitializeComponent();
            this.MouseWheel += new MouseEventHandler(Build_MouseWheel);

            this.capts = capts;
            this.Location = l;
            this.Size = s;

            loadUnits(game);
            RefreshButtons();
        }

        void Build_MouseWheel(object sender, MouseEventArgs e)
        {
            PiecesPanel.UseMouseWheel(sbVer, e, sbVer_Scroll);
        }

        private void loadUnits(Game game)
        {
            bool enabled = true;
            int y = 60 - 29;

            UnitSchema us = Game.UnitTypes.GetSchema();

            List<double> costs = new List<double>(us.Unit.Count);
            Dictionary<double, UnitSchema.UnitRow> costRef = new Dictionary<double, UnitSchema.UnitRow>(costs.Count);

            //HashSet<string> all = new HashSet<string>();
            foreach (UnitSchema.UnitRow r in us.Unit)
            {
                bool can = false;
                foreach (Capturable builder in capts)
                {
                    if (builder.CapableBuild(r.Name))
                    {
                        can = true;
                        break;
                    }
                }
                if (!can)
                    continue;

                //all.Add(r.Name);

                double theCost = (double)(r.Cost) * 1.00001 + (double)(r.People);
                while (costRef.ContainsKey(theCost))
                    theCost += .000001;

                //SortedInsert(costs, theCost);
                costs.Add(theCost);
                costRef.Add(theCost, r);
            }

            costs.Sort();
            foreach (double cost in costs)
                CreateBuildRow(costRef[cost], y += 29);

            bool canPortal = false;
            foreach (Capturable c in capts)
                if (c is Wizard)
                {
                    canPortal = true;
                    break;
                }

            y += 29;

            if (canPortal)
            {
                //double[] totals = new double[5];
                //double[] counts = new double[5];
                //foreach (UnitSchema.UnitRow r in us.Unit)
                //{
                //    int i = -1;
                //    switch (r.CostType)
                //    {
                //    case "W":
                //        i = 0;
                //        break;
                //    case "A":
                //        i = 1;
                //        break;
                //    case "E":
                //        i = 2;
                //        break;
                //    case "D":
                //        i = 3;
                //        break;
                //    case "N":
                //        i = 4;
                //        break;
                //    }
                //    if (i > -1)
                //    {
                //        double div = Math.Sqrt(r.Cost + r.People);
                //        totals[i] += r.Cost / div;
                //        counts[i] += r.People / div;
                //    }
                //}


                Dictionary<CostType, int[]> portalCost = Player.SplitPortalCost(Map.Game, Map.Game.CurrentPlayer.Race);
                List<CostType> keys = new List<CostType>(portalCost.Keys);
                keys.Sort((c1, c2) => portalCost[c1][0] + portalCost[c1][1] - portalCost[c2][0] - portalCost[c2][1]);
                foreach (CostType costType in keys)
                {
                    Color backColor;
                    switch (costType)
                    {
                        case CostType.Water:
                            backColor = this.lblWater.BackColor;
                            break;
                        case CostType.Air:
                            backColor = this.lblAir.BackColor;
                            break;
                        case CostType.Earth:
                            backColor = this.lblEarth.BackColor;
                            break;
                        case CostType.Death:
                            backColor = this.lblDeath.BackColor;
                            break;
                        case CostType.Nature:
                            backColor = this.lblNature.BackColor;
                            break;
                        default:
                            throw new Exception();
                    }

                    int[] cost = portalCost[costType];
                    int wizAmt = cost[0];
                    int otherAmt = cost[1];

                    enabled = (capts[0].Owner.Magic >= wizAmt) && ((int)capts[0].Owner.GetResource(costType.ToString()) >= otherAmt);

                    newBox(Xs[0], y, Ws[0], costType.ToString() + " Portal");
                    newBox(Xs[1], y, Ws[1], wizAmt.ToString(), this.lblWizard.BackColor, HorizontalAlignment.Right);
                    newBox(Xs[2], y, Ws[1], otherAmt.ToString(), backColor, HorizontalAlignment.Right);
                    newButton(Xs[3], y, "Summon", costType.ToString() + " Portal", new EventHandler(build_Click));

                    y += 29;
                }
            }

            enabled = (capts[0].Owner.Magic >= Player.WizardCost);

            newBox(Xs[0], y, Ws[0], "Wizard");
            newBox(Xs[1], y, (Ws[1] * 3) / 2, Player.WizardCost.ToString(), lblWizard.BackColor);
            newButton(Xs[3], y, "Summon", "Wizard", new EventHandler(build_Click));


            ShowBuildList<Wizard>(ref y, "ALWAYS:", new List<string>());
            ShowBuildList<City>(ref y, "ALSO:", costRef.Values.Select(r => r.Name));

            y += 29;

            //ShowUndos(y);

            //y += 29;

            int maxY = this.Height - this.sbHor.Height - 29;
            if (y > maxY)
                y = maxY;

            newButton((this.ClientSize.Width) / 2 - new Button().Width - 6, y,
                "Cancel", null, new EventHandler(close_Click));
            btnTrade = newButton((this.ClientSize.Width) / 2 + 6, y,
               "Trade", null, new EventHandler(trade_Click));

            ShowResources();

            SetupScrollBars();

            btnTrade.Select();
        }

        private int ShowBuildList<T>(ref int y, string label, IEnumerable<string> filter) where T : Capturable
        {
            IEnumerable<string> build = capts.OfType<T>().SelectMany(c => c.GetBuildList()).Where(n => !filter.Contains(n)).Distinct().OrderBy(s => Unit.CreateTempUnit(Map.Game, s).BaseTotalCost);
            if (build.Any())
            {
                y += 45;
                newBox(Xs[0], y, Ws[0], label);
                foreach (string u in build)
                {
                    y += 29;
                    newBox(Xs[0], y, Ws[0], u);
                }
            }
            return y;
        }

        //private void SortedInsert(List<double> collection, double value)
        //{
        //    SortedInsert(collection, value, 0, collection.Count);
        //}

        //private void SortedInsert(List<double> collection, double value, int min, int max)
        //{
        //    if (max - min < 2)
        //    {
        //        if (value < collection[min] || (max - min == 0))
        //            collection.Insert(min, value);
        //        else
        //            collection.Insert(max, value);
        //        return;
        //    }

        //    int mid = min + (max - min) / 2;
        //    if (value < collection[mid])
        //        SortedInsert(collection, value, min, mid);
        //    else
        //        SortedInsert(collection, value, mid + 1, max);
        //}

        private void CreateBuildRow(UnitSchema.UnitRow r, int y)
        {
            newBox(Xs[0], y, Ws[0], r.Name);
            newBox(Xs[5], y, Ws[4], r.Hits.ToString());
            newBox(Xs[6], y, Ws[5], r.Armor.ToString());
            newBox(Xs[7], y, Ws[6], r.Regen.ToString());
            newBox(Xs[8], y, Ws[7], r.Move.ToString());

            CostType costType;
            switch (r.CostType)
            {
                case "A":
                    costType = CostType.Air;
                    break;
                case "D":
                    costType = CostType.Death;
                    break;
                case "E":
                    costType = CostType.Earth;
                    break;
                case "N":
                    costType = CostType.Nature;
                    break;
                case "W":
                    costType = CostType.Water;
                    break;
                default:
                    costType = CostType.Production;
                    break;
            }

            UnitType type;
            switch (r.Type)
            {
                case "A":
                    type = UnitType.Air;
                    break;
                case "GWA":
                    type = UnitType.Immobile;
                    break;
                case "GW":
                    type = UnitType.Amphibious;
                    break;
                case "G":
                    type = UnitType.Ground;
                    break;
                case "W":
                    type = UnitType.Water;
                    break;
                default:
                    throw new Exception();
            }

            Color color = getColor(costType);

            newBox(Xs[1], y, Ws[1], r.Cost.ToString(), color, HorizontalAlignment.Right);
            newBox(Xs[2], y, Ws[2], r.People.ToString(), this.lblPpl.BackColor, HorizontalAlignment.Right);

            newBox(Xs[4], y, Ws[3], type.ToString());

            int x = 9;
            foreach (UnitSchema.AttackRow ar in r.GetAttackRows())
            {
                newBox(Xs[x], y, Ws[x - 1], Attack.GetString(ar.Name, ar.Damage, ar.Divide_By, ar.Target_Type, ar.Length));
                ++x;
            }

            newButton(Xs[3], y, (r.CostType != "" ? "Summon" : "Build"),
                r.Name, new EventHandler(build_Click));
        }

        private Color getColor(CostType costType)
        {
            Color color;
            switch (costType)
            {
                case CostType.Air:
                    color = lblAir.BackColor;
                    break;
                case CostType.Death:
                    color = this.lblDeath.BackColor;
                    break;
                case CostType.Earth:
                    color = this.lblEarth.BackColor;
                    break;
                case CostType.Nature:
                    color = this.lblNature.BackColor;
                    break;
                case CostType.Production:
                    color = this.lblProd.BackColor;
                    break;
                case CostType.Water:
                    color = this.lblWater.BackColor;
                    break;
                default:
                    throw new Exception();
            }

            return color;
        }

        //private int undoY = 0;
        //List<Button> undoButtons = new List<Button>();

        //private void ShowUndos()
        //{
        //    ShowUndos(undoY);
        //}

        //private void ShowUndos(int y)
        //{
        //    foreach (Button b in undoButtons)
        //        Controls.Remove(b);
        //    undoButtons = new List<Button>();

        //    undoY = y;

        //    int count = 0, incs = new Button().Width + 13;
        //    foreach (Capturable c in capts)
        //        foreach (Unit last in c.getLastBuild())
        //            if (!last.Dead)
        //                ++count;

        //    int xx = (ClientSize.Width - (count * incs - 13)) / 2;

        //    foreach (Capturable c in capts)
        //        foreach (Unit last in c.getLastBuild())
        //        {
        //            if (last.Dead)
        //                continue;

        //            undoButtons.Add(newButton(xx, y, last.Name, last,
        //                new EventHandler(undo_Click), true, Color.Black));
        //            xx += incs;
        //        }
        //}

        void trade_Click(object sender, EventArgs e)
        {
            new Trade(Map.Game.CurrentPlayer).ShowDialog();
            RefreshButtons();
            ShowResources();
        }

        private void lblResource_Click(object sender, EventArgs e)
        {
            trade_Click(sender, e);
        }

        private void RefreshButtons()
        {
            Dictionary<CostType, int[]> portalCost = Player.SplitPortalCost(Map.Game, Map.Game.CurrentPlayer.Race);
            foreach (Control control in Controls)
            {
                if (control is Button && (control.Tag is string) && ((string)control.Tag != ""))
                    if ((string)control.Tag == "Wizard")
                        control.Visible = (capts[0].Owner.Magic >= Player.WizardCost);
                    else if (((string)control.Tag).EndsWith(" Portal"))
                    {
                        CostType poralType = (CostType)Enum.Parse(typeof(CostType), ((string)control.Tag).Split(' ')[0]);
                        int[] cost = portalCost[poralType];
                        int wiz = cost[0];
                        int other = cost[1];

                        control.Visible = (capts[0].Owner.Magic >= wiz) &&
                            ((int)capts[0].Owner.GetResource(poralType.ToString()) >= other);
                    }
                    else
                    {
                        control.Visible = false;
                        foreach (Capturable c in capts)
                            if (c.CanBuild((string)control.Tag))
                            {
                                control.Visible = true;
                                break;
                            }
                    }
            }
        }

        //void undo_Click(object sender, EventArgs e)
        //{
        //    Unit unit = (Unit)((Button)sender).Tag;

        //    foreach (Capturable c in capts)
        //        if (c.getLastBuild().Contains(unit))
        //        {
        //            c.undoBuild(unit);
        //            break;
        //        }

        //    ShowUndos();
        //    RefreshButtons();
        //    ShowResources();
        //}

        void close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        void build_Click(object sender, EventArgs e)
        {
            string unit = ((string)((Button)sender).Tag);

            foreach (Capturable c in capts)
                if (c.CapableBuild(unit))
                {
                    Map.Game.BuildPiece(c, unit);
                    break;
                }

            this.Close();
        }

        private void newBox(int x, int y, int width, string text)
        {
            newBox(x, y, width, text, Color.Silver, HorizontalAlignment.Left);
        }
        private void newBox(int x, int y, int width, string text, Color color)
        {
            newBox(x, y, width, text, color, HorizontalAlignment.Left);
        }
        private void newBox(int x, int y, int width, string text, HorizontalAlignment textAlign)
        {
            newBox(x, y, width, text, Color.Silver, textAlign);
        }
        private void newBox(int x, int y, int width, string text, Color color, HorizontalAlignment textAlign)
        {
            TextBox b = new TextBox();
            b.Font = new Font("Arial", 12f);
            b.Location = new Point(x, y);
            b.Width = width;
            b.ReadOnly = true;
            b.TabStop = false;
            b.BackColor = Color.Silver;
            b.Text = text;
            b.TextAlign = textAlign;
            b.BackColor = color;
            if (b.BackColor == Color.Black || b.BackColor == Color.Blue)
                b.ForeColor = Color.Silver;
            else
                b.ForeColor = Color.Black;
            Controls.Add(b);
        }

        private Button newButton(int x, int y, string text, object tag, EventHandler eh)
        {
            Button b = new Button();
            b.Font = new Font("Arial", 9.75f);
            b.Location = new Point(x, y);
            b.Text = text;
            b.BackColor = Color.Silver;
            b.ForeColor = Color.Black;
            b.TabIndex = ++tabOrder;
            b.Tag = tag;
            b.Click += eh;
            Controls.Add(b);
            return b;
        }

        private void ShowResources()
        {
            Player current = Map.Game.CurrentPlayer;
            this.lblAir.Text = current.Air.ToString();
            this.lblDeath.Text = current.Death.ToString();
            this.lblEarth.Text = current.Earth.ToString();
            this.lblWizard.Text = current.Magic.ToString();
            this.lblNature.Text = current.Nature.ToString();
            this.lblWork.Text = current.Work.ToString();
            this.lblProd.Text = current.Production.ToString();
            this.lblWater.Text = current.Water.ToString();
            this.lblRelic.Text = current.RelicProgress.ToString();
            this.lblPpl.Text = current.Population.ToString();
        }

        private void SetupScrollBars()
        {
            int width = this.ClientSize.Width - this.sbVer.Width;
            int height = this.ClientSize.Height - this.sbHor.Height;

            int maxW = int.MinValue;
            int maxH = int.MinValue;

            foreach (Control c in this.Controls)
                if (!(c is ScrollBar))
                {
                    maxW = Math.Max(maxW, c.Location.X + c.Width);
                    maxH = Math.Max(maxH, c.Location.Y + c.Height);
                }

            this.sbVer.Visible = (maxH > height);
            this.sbHor.Visible = (maxW > width);

            int scrollBarOffset = 33 + sbVer.LargeChange;

            if (sbVer.Visible)
                this.sbVer.Maximum = maxH - height + scrollBarOffset;
            if (sbHor.Visible)
                this.sbHor.Maximum = maxW - width + scrollBarOffset;

            scrollControls(true, 0);
        }

        private void sbVer_Scroll(object sender, ScrollEventArgs e)
        {
            scrollControls(true, e.NewValue - e.OldValue);
        }

        private void sbHor_Scroll(object sender, ScrollEventArgs e)
        {
            scrollControls(false, e.NewValue - e.OldValue);
        }

        private void scrollControls(bool vertical, int diff)
        {
            foreach (Control c in this.Controls)
            {
                bool isScrollingHeader;
                if ((isScrollingHeader = (!vertical && (c is Label) && (c.Tag as string) == "header"))
                    || (c is TextBox) || (c is Button && c.Tag != null))
                {
                    c.Location = new Point(c.Location.X - (vertical ? 0 : diff), c.Location.Y - (vertical ? diff : 0));
                    if (!isScrollingHeader)
                        c.Visible = ((c.Location.Y + c.Height) < btnTrade.Location.Y) &&
                            (c.Location.Y > (label1.Location.Y + label1.Height));
                }
            }
        }
    }
}
