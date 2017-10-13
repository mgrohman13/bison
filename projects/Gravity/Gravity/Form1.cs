using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MattUtil;
using MattUtil.RealTimeGame;
using Point = System.Drawing.Point;
using BaseForm = MattUtil.RealTimeGame.GameForm;
using BaseGame = MattUtil.RealTimeGame.Game;

namespace Gravity
{
    public partial class Form1 : BaseForm
    {
        public Form1()
        {
            InitializeComponent();

            this.Bounds = Screen.PrimaryScreen.WorkingArea;
            int pad = menuStrip.Height + this.panel1.Height;
            int min = //300;
                    Math.Min(ClientSize.Width, ClientSize.Height - pad);
            this.ClientSize = new Size(min, min + pad);

            game = GetNewGame(true);
        }

        protected override BaseGame GetNewGame(bool scoring)
        {
            Rectangle rectangle = this.ClientRectangle;
            rectangle.Y += menuStrip.Height;
            rectangle.Height -= menuStrip.Height + this.panel1.Height;
            game = new Game(scoring, 1000 / 39f, this.RefreshGame, rectangle);
            //game.Start();
            return game;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (game != null)
            {
                ((Game)game).setTarget(e.X, e.Y);
                if (!game.Started || !game.Running || game.Paused)
                    RefreshGame();
            }
        }

        protected override void RefreshGame()
        {
            base.RefreshGame();
            panel1.Invalidate();

            this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    this.Text = game.Score.ToString("0");
                }
                catch (Exception exception)
                {
                    Console.WriteLine();
                    Console.WriteLine(exception);
                    Console.WriteLine();
                }
            });
        }

        private void Form1_ClientSizeChanged(object sender, EventArgs e)
        {
            if (game != null)
            {
                ((Game)game).setClientRectangle(this.ClientRectangle);
                Invalidate(ClientRectangle, false);
            }
        }

        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            Cursor.Hide();
        }

        private void Form1_MouseLeave(object sender, EventArgs e)
        {
            Cursor.Show();
        }

        private void Form1_Click(object sender, EventArgs e)
        {
            if (game != null)
            {
                if (game.Started)
                    game.Paused = !game.Paused;
                else
                    game.Start();
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (game != null && !game.GameOver())
            {
                Player player = ((Game)game).Player;
                using (Brush brush = new SolidBrush(player.GetShieldColor()))
                    e.Graphics.FillRectangle(brush, 0, 0, player.GetShieldPct() * panel1.Width, panel1.Height);
                using (Pen pen = new Pen(Color.Black, 2))
                    for (float a = .5f; a < 10; a = (float)Math.Floor(a + 1))
                    {
                        float x = Player.GetShieldPct(a) * panel1.Width;
                        e.Graphics.DrawLine(pen, x, 0, x, panel1.Height);
                    }
            }
        }
    }
}
