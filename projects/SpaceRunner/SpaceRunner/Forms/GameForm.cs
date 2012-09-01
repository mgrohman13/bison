using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BaseForm = MattUtil.RealTimeGame.GameForm;
using BaseGame = MattUtil.RealTimeGame.Game;
using System.Threading;

namespace SpaceRunner.Forms
{
    internal partial class GameForm : BaseForm
    {
        private Game Game
        {
            get
            {
                return (Game)base.game;
            }
        }

        protected override BaseGame GetNewGame(bool scoring)
        {
            Game.Dispose();

            bool isReplay = ( this.replay != null );
            this.lblTime.Visible = isReplay;
            this.tbTime.Visible = isReplay;
            this.lblSpeed.Visible = isReplay;
            this.tbSpeed.Visible = isReplay;

            if (isReplay)
                return new Game(base.RefreshGame, this.center.X, this.center.Y, this.replay);
            else
                return new Game(base.RefreshGame, this.center.X, this.center.Y, scoring);
        }

        private const float TotalMapSize = SpaceRunner.Game.MapSize * 2f;
        private const int PadSides = 13;

        private readonly Point center;
        private Region clip;

        private ToolStripMenuItem replayParent, replayShow, replaySave, replayLoad;
        private Replay replay;

        internal GameForm(Game game)
        {
            InitializeReplayMenu();
            InitializeComponent();

            base.game = game;

            center = GetCenter();
            //power up count images
            this.picAmmo.Image = PowerUp.AmmoImage;
            this.picFuel.Image = PowerUp.FuelImage;
            this.picLife.Image = PowerUp.LifeImage;

            GraphicsPath p = new GraphicsPath();
            p.AddEllipse(center.X - Game.MapSize, center.Y - Game.MapSize, TotalMapSize, TotalMapSize);
            clip = new Region(p);
            p.Dispose();

            RefreshGame();
        }

        private Point GetCenter()
        {
            int total = (int)Math.Ceiling(Game.MapSize + PadSides);
#if TRACE
            Rectangle r = Screen.GetWorkingArea(this);
            total = Math.Min(r.Width, r.Height) / 2;
#endif
            this.Size = new Size(this.Width - this.ClientSize.Width + total * 2,
                this.Height - this.ClientSize.Height + total * 2 + base.menuStrip.Height);
            this.MinimumSize = Size;
            this.MaximumSize = Size;
            return new Point(total, total + base.menuStrip.Height);
        }

        bool enabled = true;
        int ammo = -1, fuel = -1, lives = -1, score = -1;
        protected override void OnPaint(PaintEventArgs e)
        {
#if DEBUG
            try
            {
#endif
#if !TRACE
                e.Graphics.Clip = clip;
#endif
                e.Graphics.Clear(Color.Black);

                base.OnPaint(e);

                int ammo = Game.Ammo, fuel = Game.Round(Game.Fuel), lives = Game.Lives, score = Game.Round((float)Game.Score);
                if (this.ammo != ammo)
                {
                    this.ammo = ammo;
                    this.lblAmmo.Text = this.ammo.ToString();
                }
                if (this.fuel != fuel)
                {
                    this.fuel = fuel;
                    this.lblFuel.Text = fuel.ToString();
                }
                if (this.lives != lives)
                {
                    this.lives = lives;
                    this.lblLife.Text = lives.ToString();
                }
                if (this.score != score)
                {
                    this.score = score;
                    this.lblScore.Text = score.ToString("0");
                }

                bool enabled = ( Game.IsReplay || Game.GameOver() );
                if (this.enabled != enabled)
                {
                    this.enabled = enabled;
                    this.replayShow.Enabled = enabled;
                    this.replaySave.Enabled = enabled;
                }

                if (Game.IsReplay && timeScroll == null)
                {
                    this.tbTime.Value = Game.TickCount;
                }
#if DEBUG
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.StackTrace);
            }
#endif
        }

        private void GameForm_MouseMove(object sender, MouseEventArgs e)
        {
            Game.SetMouseCoordinates(e.X - center.X, e.Y - center.Y);
        }

        private void GameForm_MouseLeave(object sender, EventArgs e)
        {
            Game.Paused = true;
        }

        private void GameForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Game.Fire = true;
            else if (e.Button == MouseButtons.Right)
                Game.Turbo = true;
        }

        private void GameForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Game.Fire = false;
            else if (e.Button == MouseButtons.Right)
                Game.Turbo = false;
        }

        private void InitializeReplayMenu()
        {
            this.replayParent = new System.Windows.Forms.ToolStripMenuItem();
            this.replayShow = new System.Windows.Forms.ToolStripMenuItem();
            this.replaySave = new System.Windows.Forms.ToolStripMenuItem();
            this.replayLoad = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip.Items.Add(this.replayParent);
            this.replayParent.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { this.replayShow, this.replaySave, this.replayLoad });
            this.replayParent.Name = "replayParent";
            this.replayParent.Size = new System.Drawing.Size(130, 20);
            this.replayParent.Text = "Replay";
            this.replayShow.Name = "replayShow";
            this.replayShow.Size = new System.Drawing.Size(130, 20);
            this.replayShow.Text = "Replay";
            this.replayShow.Click += new EventHandler(replayShow_Click);
            this.replaySave.Name = "replaySave";
            this.replaySave.Size = new System.Drawing.Size(130, 20);
            this.replaySave.Text = "Save";
            this.replaySave.Click += new EventHandler(replaySave_Click);
            this.replayLoad.Name = "replayLoad";
            this.replayLoad.Size = new System.Drawing.Size(130, 20);
            this.replayLoad.Text = "Load";
            this.replayLoad.Click += new EventHandler(replayLoad_Click);
        }

        private void replayShow_Click(object sender, EventArgs e)
        {
            if (QuitPrompt())
                ShowReplay(Game.Replay);
        }
        private void replaySave_Click(object sender, EventArgs e)
        {
            string save = ShowDialog(this.saveFileDialog);
            if (save != null)
                MattUtil.TBSUtil.SaveGame(Game.Replay, save);
        }
        private void replayLoad_Click(object sender, EventArgs e)
        {
            if (QuitPrompt())
            {
                string load = ShowDialog(this.openFileDialog);
                if (load != null)
                    ShowReplay(MattUtil.TBSUtil.LoadGame<Replay>(load));
            }
        }

        private static string ShowDialog(FileDialog fileDialog)
        {
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                return fileDialog.FileName;
            return null;
        }

        private void ShowReplay(Replay replay)
        {
            this.replay = replay;
            NewGame(false);
            this.replay = null;

            SetReplaySpeed();

            this.tbTime.Maximum = replay.Length;
            this.tbTime.TickFrequency = Game.Round(replay.Length / 13f);
            this.tbTime.LargeChange = Game.Round(replay.Length / 5.2f);
            this.tbTime.SmallChange = Game.Round(replay.Length / 9.1f);
        }

        private void tbSpeed_Scroll(object sender, EventArgs e)
        {
            SetReplaySpeed();
        }
        private void tbTime_Scroll(object sender, EventArgs e)
        {
            SetReplayPosition();
        }

        private void lblSpeed_Click(object sender, EventArgs e)
        {
            this.tbSpeed.Value = 13;
            SetReplaySpeed();
        }
        private void lblTime_Click(object sender, EventArgs e)
        {
            this.tbTime.Value = 0;
            SetReplayPosition();
        }

        private void SetReplaySpeed()
        {
            SetReplaySpeed(this.tbSpeed.Value);
        }
        private void SetReplaySpeed(int value)
        {
            Game.SetReplaySpeed(value / 13.0);
        }

        private Thread timeScroll = null;
        private void SetReplayPosition()
        {
            lock (this.tbTime)
            {
                if (timeScroll != null)
                    timeScroll.Abort();

                timeScroll = new Thread(ThreadStart);
                timeScroll.IsBackground = true;
                timeScroll.Start();
            }
        }

        private void ThreadStart()
        {
            Thread.Sleep(1000);

            lock (this.tbTime)
            {
                base.game = Game.SetReplayPosition(Game, InvokeGetValue(this.tbTime), base.RefreshGame);
                SetReplaySpeed(InvokeGetValue(this.tbSpeed));
                timeScroll = null;
            }
        }
        private static int InvokeGetValue(TrackBar tb)
        {
            int value = -1;
            tb.Invoke(new MethodInvoker(delegate()
            {
                value = tb.Value;
            }));
#if DEBUG
            if (value == -1)
                throw new Exception();
#endif
            return value;
        }
    }
}
