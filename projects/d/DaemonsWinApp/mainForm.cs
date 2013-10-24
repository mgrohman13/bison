using System;
using System.Collections.Generic;
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
        private const int size = 91, offSet = 0;
        private Tile _selected;
        public static bool shift = false;
        private Font font = new Font("Arial", 13);
        private Game game;

        private Tile Selected
        {
            get
            {
                return _selected;
            }

            set
            {
                _selected = value;
                RefreshUnits();
            }
        }

        public MainForm(Game game)
        {
            this.game = game;

            InitializeComponent();

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

            RefreshPlayer();
            RefreshLog();

            this.Width += 925 - ClientSize.Width;
            this.Height += 729 - ClientSize.Height;
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            for (int x = 0 ; x <= game.GetWidth() ; x++)
                e.Graphics.DrawLine(Pens.Black, new Point(x * size + offSet, offSet),
                    new Point(x * size + offSet, offSet + game.GetHeight() * size));
            for (int y = 0 ; y <= game.GetHeight() ; y++)
                e.Graphics.DrawLine(Pens.Black, new Point(offSet, y * size + offSet),
                    new Point(offSet + game.GetWidth() * size, y * size + offSet));

            if (Selected != null)
                e.Graphics.DrawRectangle(new Pen(Color.Black, 3),
                    Selected.X * size + offSet, Selected.Y * size + offSet, size, size);

            for (int x = 0 ; x < game.GetWidth() ; x++)
                for (int y = 0 ; y < game.GetHeight() ; y++)
                {
                    Tile t = game.GetTile(x, y);

                    Bitmap picUnit = t.GetBestUnit();
                    Bitmap picAttacker = t.GetBestAttacker();
                    if (picUnit != null && picAttacker == null)
                    {
                        e.Graphics.DrawImage(picUnit, new Point(1 + x * size + offSet, 1 + y * size + offSet));
                        e.Graphics.DrawString(t.NumUnits.ToString(), font, Brushes.Black,
                            new Point(1 + x * size + offSet, 1 + y * size + offSet + 15));
                        string armyStr = t.GetArmyStr().ToString("0");
                        //float width = e.Graphics.MeasureString(armyStr, font).Width;
                        e.Graphics.DrawString(armyStr, font, Brushes.Black,
                            new Point(1 + x * size + offSet, 1 + y * size + offSet));
                        //new Point((x + 1) * size + offSet - ((int)width) - 1, 1 + y * size + offSet));
                    }
                    else if (picUnit != null && picAttacker != null)
                    {
                        e.Graphics.DrawImage(picAttacker, 1 + x * size + offSet,
                            1 + y * size + offSet, size / 2, size / 2);
                        e.Graphics.DrawString(t.GetAttackerStr().ToString("0"), font, Brushes.Black,
                            new Point(1 + x * size + offSet + size / 2, 1 + y * size + offSet));
                        e.Graphics.DrawString(t.NumAttackers.ToString(), font, Brushes.Black,
                            new Point(1 + x * size + offSet + size / 2, 1 + y * size + offSet + 15));
                        e.Graphics.DrawImage(picUnit, 1 + x * size + offSet + size / 2,
                            1 + y * size + offSet + size / 2, size / 2, size / 2);
                        string measureStr = t.GetArmyStr().ToString("0");
                        int width = (int)e.Graphics.MeasureString(measureStr, font).Width;
                        e.Graphics.DrawString(measureStr, font, Brushes.Black,
                            new Point(1 + x * size + offSet + size / 2 - width,
                            1 + y * size + offSet + size / 2));
                        measureStr = t.NumUnits.ToString();
                        width = (int)e.Graphics.MeasureString(measureStr, font).Width;
                        e.Graphics.DrawString(measureStr, font, Brushes.Black,
                           new Point(1 + x * size + offSet + size / 2 - width,
                           1 + y * size + offSet + size / 2 + 15));
                    }

                    int newX = offSet + x * size + 3;
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
                        e.Graphics.FillEllipse(brush, newX, ( y + 1 ) * size + offSet - 16, 13, 13);
                        newX += 16;
                    }
                }
        }

        private void ClearUnitInfo()
        {
            this.lblInf1.Text = "";
            this.lblInf2.Text = "";
            this.lblInf3.Text = "";
            this.lblInf4.Text = "";
            this.lblUnit1.Image = null;
            this.lblUnit2.Image = null;
            this.lblUnit3.Image = null;
            this.lblUnit4.Image = null;
        }

        private void RefreshUnits()
        {
            lblAccent.Visible = game.HasWinner();

            RefreshButtons();

            ClearUnitInfo();

            if (Selected == null)
                return;

            Unit[] units = Selected.GetUnits();

            if (units.Length == 0)
                return;

            List<Unit>[] types = new List<Unit>[4];

            int[] counts = new int[types.Length];

            for (int a = 0 ; a < types.Length ; a++)
            {
                types[a] = new List<Unit>();
                counts[a] = 0;
            }

            foreach (Unit u in units)
            {
                switch (u.Type)
                {
                case UnitType.Archer:
                    types[1].Add(u);
                    if (u.Healed && u.Movement > 0)
                        counts[1]++;
                    break;

                case UnitType.Daemon:
                    types[3].Add(u);
                    if (u.Healed && u.Movement > 0)
                        counts[3]++;
                    break;

                case UnitType.Indy:
                case UnitType.Infantry:
                    types[0].Add(u);
                    if (u.Healed && u.Movement > 0)
                        counts[0]++;
                    break;

                case UnitType.Knight:
                    types[2].Add(u);
                    if (u.Healed && u.Movement > 0)
                        counts[2]++;
                    break;

                default:
                    throw new Exception("I need sleep.");
                }
            }

            Label[] pics = new Label[4];
            pics[0] = this.lblUnit1;
            pics[1] = this.lblUnit2;
            pics[2] = this.lblUnit3;
            pics[3] = this.lblUnit4;

            Label[] infos = new Label[4];
            infos[0] = this.lblInf1;
            infos[1] = this.lblInf2;
            infos[2] = this.lblInf3;
            infos[3] = this.lblInf4;

            int num = 0;

            for (int a = 0 ; a < types.Length ; a++)
                if (types[a].Count > 0)
                {
                    infos[num].Text = string.Format("{0}({1})", types[a].Count, counts[a]);
                    pics[num++].Image = types[a][0].GetPic();
                }
        }

        private void RefreshButtons()
        {
            if (Selected != null)
            {
                this.btnBuild.Visible = ( ( Selected.GetProduction(true).Count > 0 )
                        && ( Selected.GetUnits(game.GetCurrentPlayer(), true).Count > 0 )
                        && Selected.NumAttackers == 0 );

                this.btnFight.Visible = Selected.NumAttackers > 0;
            }
        }

        private void RefreshPlayer()
        {
            this.lblPlayer.BackColor = game.GetCurrentPlayer().Color;
            this.lblPlayer.ForeColor = game.GetCurrentPlayer().InverseColor;
            this.lblPlayer.Text = game.GetCurrentPlayer().Name;
            this.lblTurn.Text = game.Turn;
            RefreshSouls();
            RefreshArrows();
        }

        private void RefreshSouls()
        {
            this.lblSouls.Text = game.GetCurrentPlayer().Souls.ToString();
        }

        private void RefreshArrows()
        {
            this.lblArrows.Text = game.GetCurrentPlayer().Arrows.ToString();
        }

        private void btnEndTurn_Click(object sender, EventArgs e)
        {
            DialogResult result = DialogResult.Yes;
            Tile tile = game.GetCurrentPlayer().NextUnit(Selected);
            if (tile != null && tile.GetUnits(game.GetCurrentPlayer(), true, true).Count > 0)
                result = MessageBox.Show("You have unmoved units.  Are you sure?", "", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                game.EndTurn();
                if (game.GetPlayers().Length == 1)
                {
                    TextForm.ShowDialog(game, false);
                }
                else
                {
                    Selected = null;
                    RefreshPlayer();
                    Refresh();
                    RefreshLog();
                }
            }
        }

        private void RefreshLog()
        {
            string log = game.CombatLog;
            if (log.Length > 300)
                log = log.Substring(0, 300);
            this.lblLog.Text = log.Trim();
        }

        private void mainForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                InfoForm.CloseForm();

            int x = ( e.X - offSet ) / size, y = ( e.Y - offSet ) / size;

            if (x >= 0 && y >= 0 && x < game.GetWidth() && y < game.GetHeight())
            {
                Tile clicked = game.GetTile(x, y);

                if (e.Button == MouseButtons.Left)
                    Selected = clicked;
                else if (e.Button == MouseButtons.Right)
                {
                    if (Selected == null)
                        return;

                    if (Selected.IsNeighbor(clicked))
                        if (MoveForm.ShowDialog(Selected, clicked, shift, shift ? game.GetCurrentPlayer().Arrows : int.MaxValue) == DialogResult.OK)
                        {
                            Selected = clicked;

                            RefreshLog();
                            RefreshArrows();
                        }
                }
            }

            Refresh();
            RefreshUnits();
            RefreshSouls();
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            if (Selected == null)
                return;
            List<Unit> units = Selected.GetUnits(game.GetCurrentPlayer(), true),
                unUsed = new List<Unit>(), output = new List<Unit>();
            if (units.Count == 0)
                return;
            if (Selected.GetProduction(true).Count == 0)
                return;
            try
            {
                InfoForm.ShowDialog(ref units, ref unUsed, ref output, UseType.Build);
            }
            catch
            {
            }
            if (output.Count > 0 && output[0] != null)
                output[0].Build();

            Refresh();
            RefreshUnits();
        }

        private void mainForm_MouseDown(object sender, MouseEventArgs e)
        {
            int x = ( e.X - offSet ) / size, y = ( e.Y - offSet ) / size;

            if (x >= 0 && y >= 0 && x < game.GetWidth() && y < game.GetHeight())
            {
                Tile clicked = game.GetTile(x, y);

                if (e.Button == MouseButtons.Right)
                {
                    List<Unit> units = new List<Unit>(clicked.GetAllUnits()), a = null, b = null;
                    InfoForm.ShowDialog(ref units, ref a, ref b, UseType.View);
                }
            }
        }

        private void mainForm_KeyDown(object sender, KeyEventArgs e)
        {
            shift = e.Shift;
        }

        private void mainForm_KeyUp(object sender, KeyEventArgs e)
        {
            shift = e.Shift;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            Selected = game.GetCurrentPlayer().NextUnit(Selected);
            Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Selected != null)
            {
                foreach (Unit u in Selected.GetAttackers())
                    u.Heal();
                foreach (Unit u in Selected.GetUnits())
                    u.Heal();
                RefreshArrows();
            }
            btnNext_Click(sender, e);
        }

        private void btnFight_Click(object sender, EventArgs e)
        {
            if (Selected != null && Selected.FightBattle())
            {
                Refresh();
                RefreshUnits();
                RefreshSouls();
                RefreshLog();
                RefreshPlayer();
            }
        }

        private void lblLog_Click(object sender, EventArgs e)
        {
            TextForm.ShowDialog(game, true);
        }

        private void btnPlayers_Click(object sender, EventArgs e)
        {
            new PlayersForm(game).ShowDialog();
        }

        private void mainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (InfoForm.Form.Visible)
                InfoForm.Form.infoForm_MouseMove(sender, e);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                game.SaveGame(saveFileDialog1.FileName);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                game = Game.LoadGame(openFileDialog1.FileName);

                Refresh();
                RefreshArrows();
                RefreshButtons();
                RefreshLog();
                RefreshPlayer();
                RefreshSouls();
                RefreshUnits();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.game.AutoSave();
        }
    }
}