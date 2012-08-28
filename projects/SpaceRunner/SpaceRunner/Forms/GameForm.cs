using System;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BaseForm = MattUtil.RealTimeGame.GameForm;
using BaseGame = MattUtil.RealTimeGame.Game;

namespace SpaceRunner.Forms
{
    internal partial class GameForm : BaseForm
    {
        new internal static Game Game
        {
            get
            {
                return (Game)BaseForm.Game;
            }
        }

        internal void NewGame()
        {
            base.NewGame(this.RefreshGame);
        }

        protected override BaseGame StartNewGame(MattUtil.RealTimeGame.GameTicker.EventDelegate RefreshGame, bool scoring)
        {
            return StartNewGame(RefreshGame, scoring, null);
        }

        private BaseGame StartNewGame(MattUtil.RealTimeGame.GameTicker.EventDelegate RefreshGame, bool scoring, Replay replay)
        {
            Game game = new Game(this.RefreshGame);
            if (BaseForm.game != null)
                ( (IDisposable)BaseForm.game ).Dispose();
            BaseForm.game = game;
            if (replay == null)
                game.InitGame(this.center.X, this.center.Y, scoring);
            else
                game.InitReplay(this.center.X, this.center.Y, replay);
            RefreshGame();
            return game;
        }

        const float TotalMapSize = SpaceRunner.Game.MapSize * 2f;
        const int PadSides = 13;

        readonly Point center;
        Region clip;

        private ToolStripMenuItem replay;

        private static GameForm form;
        internal GameForm()
        {
            this.replay = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip.Items.Add(this.replay);
            this.replay.Name = "newGameToolStripMenuItem";
            this.replay.Size = new System.Drawing.Size(130, 20);
            this.replay.Click += new EventHandler(replay_Click);

            form = this;
            BaseForm.game = new Game(this.RefreshGame);
            BaseForm.game.Running = false;
            InitializeComponent();

            center = GetCenter();
            //power up count images
            this.picAmmo.Image = PowerUp.AmmoImage;
            this.picFuel.Image = PowerUp.FuelImage;
            this.picLife.Image = PowerUp.LifeImage;

            GraphicsPath p = new GraphicsPath();
            p.AddEllipse(center.X - Game.MapSize, center.Y - Game.MapSize, TotalMapSize, TotalMapSize);
            clip = new Region(p);
            p.Dispose();
        }

        void replay_Click(object sender, EventArgs e)
        {
            Replay replay = null;

            if (!Game.Started)
            {
                if (QuitPrompt())
                {
                    string load = ShowDialog(this.openFileDialog);
                    if (load != null && File.Exists(load))
                        replay = MattUtil.TBSUtil.LoadGame<Replay>(load);
                }
            }
            else if (Game.IsReplay)
            {
                string save = ShowDialog(this.saveFileDialog);
                if (save != null)
                    MattUtil.TBSUtil.SaveGame(Game.Replay, save);
            }
            else
            {
                if (QuitPrompt())
                    replay = Game.Replay;
            }

            if (replay != null)
                StartNewGame(this.RefreshGame, false, replay);
        }

        private static string ShowDialog(FileDialog fileDialog)
        {
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                return fileDialog.FileName;
            return null;
        }

        Point GetCenter()
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

        protected override void OnPaint(PaintEventArgs e)
        {
#if !TRACE
            e.Graphics.Clip = clip;
#endif
            e.Graphics.Clear(Color.Black);

            base.OnPaint(e);

            //show power up counts
            this.lblAmmo.Text = Game.Ammo.ToString();
            this.lblFuel.Text = Game.Fuel.ToString("0");
            this.lblLife.Text = Game.Lives.ToString();
            this.lblScore.Text = Game.Score.ToString("0");

            if (!Game.Started)
                this.replay.Text = "Load Replay";
            else if (Game.IsReplay)
                this.replay.Text = "Save Replay";
            else
                this.replay.Text = "Replay";
        }

        //private bool setMouse = true;
        void GameForm_MouseMove(object sender, MouseEventArgs e)
        {
            //if (setMouse)
            //{
            //    setMouse = false;
            Game.SetMouseCoordinates(e.X - center.X, e.Y - center.Y);
            //    setMouse = true;
            //}
        }

        void GameForm_MouseLeave(object sender, EventArgs e)
        {
            Game.Paused = true;
        }

        void GameForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Game.Fire = true;
            else if (e.Button == MouseButtons.Right)
                Game.Turbo = true;
        }

        void GameForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Game.Fire = false;
            else if (e.Button == MouseButtons.Right)
                Game.Turbo = false;
        }
    }
}
