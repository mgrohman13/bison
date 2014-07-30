using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading;
using MattUtil;
using Point = MattUtil.Point;
using BaseForm = MattUtil.RealTimeGame.GameForm;
using BaseGame = MattUtil.RealTimeGame.Game;

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
            if (Game.IsReplay && timeScroll != null)
                lock (Game.Replay)
                    timeScroll.Abort();

            lock (this)
            {
                Game.Running = false;
                Game.Dispose();

                bool isReplay = ( this.replay != null );
                this.lblTime.Visible = isReplay;
                this.tbTime.Visible = isReplay;
                this.lblSpeed.Visible = isReplay;
                this.tbSpeed.Visible = isReplay;

                if (isReplay)
                    return new Game(base.RefreshGame, this.center.X, this.center.Y, this.replay);
                else
                    return new Game(base.RefreshGame, this.center.X, this.center.Y, scoring, allowReplay);
            }
        }

        private const float TotalMapSize = SpaceRunner.Game.MapSize * 2f;
        private const int PadSides = 13;

        private readonly Point center;
        private Region clip;

        private ToolStripMenuItem replayParent, replayShow, replaySave, replayLoad, replayEnable;
        private Replay replay;
        private bool allowReplay = true;

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

            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(center.X - Game.MapSize, center.Y - Game.MapSize, TotalMapSize, TotalMapSize);
            clip = new Region(path);
            path.Dispose();

            RefreshGame();
        }

        private Point GetCenter()
        {
            int center = Game.Random.Round(Game.MapSize + PadSides);
            int widthOffset = this.Width - this.ClientSize.Width;
            int heightOffset = this.Height - this.ClientSize.Height + base.menuStrip.Height;
#if TRACE
            Rectangle screenArea = Screen.GetWorkingArea(this);
            center = Game.Random.Round(Math.Min(screenArea.Width - widthOffset, screenArea.Height - heightOffset) / 2f);
#endif
            this.Size = new Size(widthOffset + center * 2, heightOffset + center * 2);
            this.MinimumSize = Size;
            this.MaximumSize = Size;
            return new Point(center, center + base.menuStrip.Height);
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

                int ammo = Game.Ammo, fuel = Game.FuelInt, lives = Game.Lives, score = Game.Round((float)( Game.Score ));
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

                bool enabled = ( ( Game.Replay != null ) && ( Game.IsReplay || Game.GameOver() ) );
                if (this.enabled != enabled)
                {
                    this.enabled = enabled;
                    this.replayShow.Enabled = enabled;
                    this.replaySave.Enabled = enabled;
                }

                if (Game.IsReplay && timeScroll == null && !Game.Paused)
                {
                    int value = Game.TickCount;
                    if (value > this.tbTime.Maximum)
                        value = this.tbTime.Maximum;
                    this.tbTime.Value = value;
                }
#if DEBUG
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.StackTrace);
            }
#endif
        }

        private bool fire = false;

        private void GameForm_MouseMove(object sender, MouseEventArgs e)
        {
            int x = e.X - center.X, y = e.Y - center.Y;
            Game.SetMouseCoordinates(x, y);
            if (fire)
                Game.Fire(x, y);
        }

        private void GameForm_MouseLeave(object sender, EventArgs e)
        {
            if (!Game.IsReplay)
                Game.Paused = true;
        }

        private void GameForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (Game.IsReplay)
            {
                Game.Paused = !Game.Paused;
            }
            else if (e.Button == MouseButtons.Left)
            {
                fire = true;
                GameForm_MouseMove(sender, e);
            }
            else if (e.Button == MouseButtons.Right)
            {
                Game.Turbo = true;
            }
        }

        private void GameForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                fire = false;
            else if (e.Button == MouseButtons.Right)
                Game.Turbo = false;
        }

        private void InitializeReplayMenu()
        {
            this.replayParent = new System.Windows.Forms.ToolStripMenuItem();
            this.replayShow = new System.Windows.Forms.ToolStripMenuItem();
            this.replaySave = new System.Windows.Forms.ToolStripMenuItem();
            this.replayLoad = new System.Windows.Forms.ToolStripMenuItem();
            this.replayEnable = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip.Items.Add(this.replayParent);
            this.replayParent.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { this.replayShow, this.replaySave, this.replayLoad, this.replayEnable });
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
            this.replayEnable.Name = "replayEnable";
            this.replayEnable.Size = new System.Drawing.Size(130, 20);
            RefreshReplayEnable();
            this.replayEnable.Click += new EventHandler(replayEnable_Click);
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
                Game.Replay.Save(save);
        }
        private void replayLoad_Click(object sender, EventArgs e)
        {
            if (QuitPrompt())
            {
                string load = ShowDialog(this.openFileDialog);
                if (load != null)
                    ShowReplay(Replay.Load(load));
            }
        }
        private void replayEnable_Click(object sender, EventArgs e)
        {
            allowReplay = !allowReplay;
            RefreshReplayEnable();
        }
        private void RefreshReplayEnable()
        {
            this.replayEnable.Text = ( allowReplay ? "Disable" : "Enable" );
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
            this.tbTime.TickFrequency = Game.Random.Round(replay.Length / 13f);
            this.tbTime.LargeChange = Game.Random.Round(replay.Length / 5.2f);
            this.tbTime.SmallChange = Game.Random.Round(replay.Length / 9.1f);
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
            Game.SetReplaySpeed(value / 13f);
        }

        private Thread timeScroll = null;
        private void SetReplayPosition()
        {
            if (timeScroll != null)
                lock (Game.Replay)
                    timeScroll.Abort();

            timeScroll = new Thread(ThreadStart);
            timeScroll.IsBackground = true;
            timeScroll.Start();
        }

        private void ThreadStart()
        {
            Thread.Sleep(1000);

            lock (this)
            {
                if (tbTime.IsHandleCreated)
                    base.game = Game.SetReplayPosition(Game, InvokeGetValue(this.tbTime), base.RefreshGame);
                if (tbSpeed.IsHandleCreated)
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
