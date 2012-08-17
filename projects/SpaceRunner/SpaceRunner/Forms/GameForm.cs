using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BaseForm = MattUtil.RealTimeGame.GameForm;

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

        protected override MattUtil.RealTimeGame.Game StartNewGame(MattUtil.RealTimeGame.GameTicker.EventDelegate RefreshGame, bool scoring)
        {
            Game game = new Game(this.RefreshGame);
            if (BaseForm.game != null)
                ( (IDisposable)BaseForm.game ).Dispose();
            BaseForm.game = game;
            game.InitGame(this.center.X, this.center.Y, false, scoring);
            RefreshGame();
            return game;
        }

        const float TotalMapSize = SpaceRunner.Game.MapSize * 2f;
        const int PadSides = 13;

        readonly Point center;
        Region clip;

        private static GameForm form;
        internal GameForm()
        {
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

        //void fireworksToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (QuitPrompt())
        //        Game.StartNewFireworks(RefreshGame, center.X, center.Y);
        //}

        protected override bool AskQuit()
        {
            return ( base.AskQuit() && !Game.Fireworks );
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
        }

        void GameForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Game.Fireworks)
                Game.SetMouseCoordinates(e.X - center.X, e.Y - center.Y);
        }

        void GameForm_MouseLeave(object sender, EventArgs e)
        {
            if (!Game.Fireworks)
                Game.Paused = true;
        }

        void GameForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (!Game.Fireworks)
            {
                if (e.Button == MouseButtons.Left)
                    Game.Fire = true;
                else if (e.Button == MouseButtons.Right)
                    Game.Turbo = true;
            }
        }

        void GameForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (!Game.Fireworks)
            {
                if (e.Button == MouseButtons.Left)
                    Game.Fire = false;
                else if (e.Button == MouseButtons.Right)
                    Game.Turbo = false;
            }
        }
    }
}
