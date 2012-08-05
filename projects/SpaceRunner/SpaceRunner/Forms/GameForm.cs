using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SpaceRunner.Forms
{
    public partial class GameForm : MattUtil.RealTimeGame.GameForm
    {
        new public static Game Game
        {
            get
            {
                return (Game)MattUtil.RealTimeGame.GameForm.Game;
            }
        }

        internal void NewGame()
        {
            base.NewGame(this.RefreshGame);
        }

        protected override MattUtil.RealTimeGame.Game StartNewGame(MattUtil.RealTimeGame.GameTicker.EventDelegate RefreshGame, bool scoring)
        {
            Game game = new Game(this.RefreshGame);
            MattUtil.RealTimeGame.GameForm.game = game;
            game.InitGame(this.center.X, this.center.Y, false, scoring);
            RefreshGame();
            return game;
        }

        const float TotalMapSize = SpaceRunner.Game.MapSize * 2f;
        const int PadSides = 13;

        readonly Point center;
        Region clip;

        private static GameForm form;
        public GameForm()
        {
            form = this;
            MattUtil.RealTimeGame.GameForm.game = new Game(this.RefreshGame);
            MattUtil.RealTimeGame.GameForm.game.Running = false;
            InitializeComponent();

            center = GetCenter();
            //power up count images
            this.picAmmo.Image = PowerUp.ammoImage;
            this.picFuel.Image = PowerUp.fuelImage;
            this.picLife.Image = PowerUp.lifeImage;

            GraphicsPath p = new GraphicsPath();
            p.AddEllipse(center.X - Game.MapSize, center.Y - Game.MapSize, TotalMapSize, TotalMapSize);
            clip = new Region(p);
            p.Dispose();
        }

        Point GetCenter()
        {
            int total = (int)Math.Ceiling(Game.MapSize + PadSides);
#if TRACE
            total = (int)Math.Ceiling(Game.RemovalDist + PadSides);
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
