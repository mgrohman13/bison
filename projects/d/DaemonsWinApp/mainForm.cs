using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Daemons;

namespace DaemonsWinApp
{
    public partial class MainForm : Form
    {
        public static MainForm instance;

        public static bool shift = false;

        const int size = 91, offset = 0;

        Game game;
        Font font = new Font("Arial", 13);
        int timestamp;

        Tile selected;

        public MainForm(Game game)
        {
            instance = this;

            this.game = game;

            InitializeComponent();

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

            this.Width += 925 - ClientSize.Width;
            this.Height += 729 - ClientSize.Height;
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;

            RefreshAll();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                for (int x = 0 ; x <= game.Width ; x++)
                    e.Graphics.DrawLine(Pens.Black, new Point(x * size + offset, offset),
                            new Point(x * size + offset, offset + game.Height * size));
                for (int y = 0 ; y <= game.Height ; y++)
                    e.Graphics.DrawLine(Pens.Black, new Point(offset, y * size + offset),
                            new Point(offset + game.Width * size, y * size + offset));

                if (selected != null)
                    using (Pen p = new Pen(Color.Black, 3))
                        e.Graphics.DrawRectangle(p, selected.X * size + offset, selected.Y * size + offset, size, size);

                for (int x = 0 ; x < game.Width ; x++)
                    for (int y = 0 ; y < game.Height ; y++)
                    {
                        Tile t = game.GetTile(x, y);

                        IEnumerable<Unit> units = t.GetUnits(game.GetCurrentPlayer());
                        IEnumerable<Unit> attackers = null;
                        if (units.Any())
                        {
                            attackers = t.GetUnits().Where((u) => !u.Owner.IsTurn());
                        }
                        else
                        {
                            units = t.GetUnits();
                            if (units.Any())
                            {
                                units = units.GroupBy((u) => u.Owner).OrderByDescending((g) => Tile.GetArmyStr(g)).First();
                                attackers = t.GetUnits().Where((u) => ( u.Owner != ( (IGrouping<Player, Unit>)units ).Key ));
                            }
                        }
                        Bitmap picAttacker = Tile.GetBestPic(attackers);
                        Bitmap picUnit = Tile.GetBestPic(units);
                        if (picUnit != null && picAttacker == null)
                        {
                            e.Graphics.DrawImage(picUnit, new Point(1 + x * size + offset, 1 + y * size + offset));
                            e.Graphics.DrawString(units.Count().ToString(), font, Brushes.Black,
                                    new Point(1 + x * size + offset, 1 + y * size + offset + 15));
                            string armyStr = Tile.GetArmyStr(units).ToString("0");
                            e.Graphics.DrawString(armyStr, font, Brushes.Black,
                                    new Point(1 + x * size + offset, 1 + y * size + offset));
                        }
                        else if (picUnit != null && picAttacker != null)
                        {
                            e.Graphics.DrawImage(picAttacker, 1 + x * size + offset,
                                    1 + y * size + offset, size / 2, size / 2);
                            e.Graphics.DrawString(Tile.GetArmyStr(attackers).ToString("0"), font, Brushes.Black,
                                    new Point(1 + x * size + offset + size / 2, 1 + y * size + offset));
                            e.Graphics.DrawString(attackers.Count().ToString(), font, Brushes.Black,
                                    new Point(1 + x * size + offset + size / 2, 1 + y * size + offset + 15));
                            e.Graphics.DrawImage(picUnit, 1 + x * size + offset + size / 2,
                                    1 + y * size + offset + size / 2, size / 2, size / 2);
                            string measureStr = Tile.GetArmyStr(units).ToString("0");
                            int width = (int)e.Graphics.MeasureString(measureStr, font).Width;
                            e.Graphics.DrawString(measureStr, font, Brushes.Black,
                                    new Point(1 + x * size + offset + size / 2 - width,
                                    1 + y * size + offset + size / 2));
                            measureStr = units.Count().ToString();
                            width = (int)e.Graphics.MeasureString(measureStr, font).Width;
                            e.Graphics.DrawString(measureStr, font, Brushes.Black,
                                    new Point(1 + x * size + offset + size / 2 - width,
                                    1 + y * size + offset + size / 2 + 15));
                        }

                        int newX = offset + x * size + 3;
                        foreach (ProductionCenter pc in t.GetProduction())
                        {
                            Brush brush;
                            switch (pc.Type)
                            {
                            case ProductionType.Archer:
                                if (pc.Used)
                                    brush = Brushes.Yellow;
                                else
                                    brush = Brushes.Orange;
                                break;

                            case ProductionType.Infantry:
                                if (pc.Used)
                                    brush = Brushes.Lime;
                                else
                                    brush = Brushes.Green;
                                break;

                            case ProductionType.Knight:
                                if (pc.Used)
                                    brush = Brushes.Cyan;
                                else
                                    brush = Brushes.Blue;
                                break;

                            default:
                                throw new Exception("go home");
                            }
                            e.Graphics.FillEllipse(brush, newX, ( y + 1 ) * size + offset - 16, 13, 13);
                            newX += 16;
                        }
                    }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        void ClearUnitInfo()
        {
            this.lblMorale.Text = "";
            this.lblInf1.Text = "";
            this.lblInf2.Text = "";
            this.lblInf3.Text = "";
            this.lblInf4.Text = "";
            this.lblUnit1.Image = null;
            this.lblUnit2.Image = null;
            this.lblUnit3.Image = null;
            this.lblUnit4.Image = null;
        }

        void RefreshAll()
        {
            Invalidate();
            RefreshArrows();
            RefreshButtons();
            RefreshLog();
            RefreshPlayer();
            RefreshSouls();
            RefreshUnits();
        }

        void RefreshUnits()
        {
            lblAccent.Visible = game.HasWinner();

            ClearUnitInfo();
            if (selected == null)
                return;

            IEnumerable<Unit> units = selected.GetUnits(game.GetCurrentPlayer());
            if (!units.Any())
            {
                units = selected.GetUnits();
                if (!units.Any())
                    return;
            }

            List<Unit>[] types = new List<Unit>[4];
            for (int a = 0 ; a < types.Length ; a++)
                types[a] = new List<Unit>();
            foreach (Unit u in units)
                switch (u.Type)
                {
                case UnitType.Archer:
                    types[1].Add(u);
                    break;

                case UnitType.Daemon:
                    types[3].Add(u);
                    break;

                case UnitType.Indy:
                case UnitType.Infantry:
                    types[0].Add(u);
                    break;

                case UnitType.Knight:
                    types[2].Add(u);
                    break;

                default:
                    throw new Exception("I need sleep.");
                }

            Label[] pics = new[] { this.lblUnit1, this.lblUnit2, this.lblUnit3, this.lblUnit4 };
            Label[] infos = new[] { this.lblInf1, this.lblInf2, this.lblInf3, this.lblInf4 };

            IGrouping<Player, Unit> player = units.GroupBy((u) => u.Owner).OrderByDescending((g) => Tile.GetArmyStr(g)).First();

            this.lblMorale.Text = GetMorale(player).ToString("0%");
            IEnumerable<Unit> attackers = selected.GetUnits().Where((u) => u.Owner != player.Key);
            if (attackers.Any())
                this.lblMorale.Text = GetMorale(attackers).ToString("0% : ") + this.lblMorale.Text;

            IEnumerable<int> active = new int[0];
            if (units.All((u) => u.Owner == player.Key))
                active = player.Key.GetActive(types);
            int num = 0;
            for (int a = 0 ; a < types.Length ; a++)
                if (types[a].Count > 0)
                {
                    infos[num].Text = string.Format("{0} / {1} / {2}", types[a].Count((u) => u.Healed),
                            types[a].Count((u) => ( u.Movement > 0 )), types[a].Count());
                    infos[num].Font = new Font("Microsoft Sans Serif", 11.25f, ( active.Contains(a) ? FontStyle.Bold : FontStyle.Regular ));
                    infos[num].ForeColor = ( active.Contains(a) ? Color.Black : Color.Gray );
                    pics[num++].Image = types[a].GroupBy((u) => u.Owner).OrderByDescending((g) => Tile.GetArmyStr(g)).First().First().GetPic();
                }
        }
        private static double GetMorale(IEnumerable<Unit> units)
        {
            double morale = Tile.GetMorale(units);
            if (morale > Consts.MoraleMax)
                morale = 1;
            return morale;
        }

        void RefreshButtons()
        {
            if (selected != null)
            {
                this.btnBuild.Visible = ( selected.GetProduction(true).Any()
                        && selected.GetUnits(game.GetCurrentPlayer(), true).Any()
                        && selected.Unoccupied(game.GetCurrentPlayer()) );
                this.btnFight.Visible = selected.CanBattle();
            }
        }

        void RefreshPlayer()
        {
            this.lblPlayer.BackColor = game.GetCurrentPlayer().Color;
            this.lblPlayer.ForeColor = game.GetCurrentPlayer().InverseColor;
            this.lblPlayer.Text = game.GetCurrentPlayer().Name;
            this.lblTurn.Text = game.Turn;
        }

        void RefreshSouls()
        {
            this.lblSouls.Text = game.GetCurrentPlayer().Souls.ToString();
        }

        void RefreshArrows()
        {
            this.lblArrows.Text = game.GetCurrentPlayer().Arrows.ToString();
        }

        void btnEndTurn_Click(object sender, EventArgs e)
        {
            DialogResult result = DialogResult.Yes;
            if (game.GetCurrentPlayer().GetUnits().Any(Consts.MoveLeft))
                result = MessageBox.Show("You have unmoved units.  Are you sure?", "", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                game.EndTurn();

                selected = null;
                RefreshAll();

                if (game.GetPlayers().Count == 1)
                    TextForm.ShowDialog(game, false);
            }
        }

        void RefreshLog()
        {
            string log = game.CombatLog;
            if (log.Length > 300)
                log = log.Substring(0, 300);
            this.lblLog.Text = log.Trim();
        }

        void mainForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                InfoForm.CloseForm();
                if (Environment.TickCount - timestamp > 260)
                    return;
            }

            int x = ( e.X - offset ) / size, y = ( e.Y - offset ) / size;

            if (x >= 0 && y >= 0 && x < game.Width && y < game.Height)
            {
                Tile clicked = game.GetTile(x, y);

                if (e.Button == MouseButtons.Left)
                {
                    selected = clicked;
                    RefreshAll();
                }
                else if (e.Button == MouseButtons.Right)
                {
                    if (selected == null)
                        return;

                    if (selected.IsNeighbor(clicked))
                        if (MoveForm.ShowDialog(selected, clicked, shift) == DialogResult.OK)
                        {
                            selected = clicked;
                            RefreshAll();
                        }
                }
            }
        }

        void btnBuild_Click(object sender, EventArgs e)
        {
            if (selected == null)
                return;
            List<Unit> units = selected.GetUnits(game.GetCurrentPlayer(), true).ToList(),
                unUsed = new List<Unit>(), output = new List<Unit>();
            if (units.Count == 0)
                return;
            if (!selected.GetProduction(true).Any())
                return;
            try
            {
                InfoForm.ShowDialog(ref units, ref unUsed, ref output, UseType.Build);
            }
            catch
            {
            }
            if (output.Count > 0 && output[0] != null)
            {
                output[0].Build();
                SelectNext(sender, e);
                RefreshAll();
            }
        }

        void mainForm_MouseDown(object sender, MouseEventArgs e)
        {
            int x = ( e.X - offset ) / size, y = ( e.Y - offset ) / size;

            if (x >= 0 && y >= 0 && x < game.Width && y < game.Height)
            {
                Tile clicked = game.GetTile(x, y);

                if (e.Button == MouseButtons.Right)
                {
                    timestamp = Environment.TickCount;

                    List<Unit> units = clicked.GetUnits().ToList(), a = null, b = null;
                    InfoForm.ShowDialog(ref units, ref a, ref b, UseType.View);
                }
            }
        }

        void mainForm_KeyDown(object sender, KeyEventArgs e)
        {
            shift = e.Shift;
        }

        void mainForm_KeyUp(object sender, KeyEventArgs e)
        {
            shift = e.Shift;
        }

        void btnNext_Click(object sender, EventArgs e)
        {
            selected = game.GetCurrentPlayer().NextUnit(selected);
            RefreshAll();
        }

        void button1_Click(object sender, EventArgs e)
        {
            if (selected != null)
            {
                foreach (Unit u in selected.GetUnits())
                    u.Heal();

                SelectNext(sender, e);
                RefreshAll();
            }
        }
        private void SelectNext(object sender, EventArgs e)
        {
            if (!game.GetCurrentPlayer().GetActive(new[] { selected.GetUnits(game.GetCurrentPlayer()) }).Any())
                btnNext_Click(sender, e);
        }

        void btnFight_Click(object sender, EventArgs e)
        {
            if (selected != null && selected.FightBattle())
                RefreshAll();
        }

        void lblLog_Click(object sender, EventArgs e)
        {
            TextForm.ShowDialog(game, true);
        }

        void btnPlayers_Click(object sender, EventArgs e)
        {
            new PlayersForm(game).ShowDialog();
        }

        void mainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (InfoForm.Form.Visible)
            {
                //InfoForm.Form.Activate();
                int x = e.X + this.DesktopLocation.X - InfoForm.Form.DesktopLocation.X;
                int y = e.Y + this.DesktopLocation.Y - InfoForm.Form.DesktopLocation.Y;
                InfoForm.Form.infoForm_MouseMove(sender, new MouseEventArgs(e.Button, e.Clicks, x, y, e.Delta));
            }
        }

        void button3_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                game.SaveGame(saveFileDialog1.FileName);
        }

        void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                game = Game.LoadGame(openFileDialog1.FileName);

                RefreshAll();
            }
        }

        void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.game.AutoSave();

            if (this.game.CombatLog.Length > 0)
            {
                string logFile = "../../../combat.txt";
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(logFile, true))
                {
                    sw.WriteLine();
                    sw.WriteLine();
                    sw.WriteLine(this.game.CombatLog);
                    sw.Flush();
                }
            }
        }
    }
}