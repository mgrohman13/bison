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
            int min = Math.Min(ClientSize.Width, ClientSize.Height);
            this.ClientSize = new Size(min, min);
            //Text = ClientSize.ToString();

            game = GetNewGame(false);
        }

        protected override BaseGame GetNewGame(bool scoring)
        {
            game = new Game(1000 / 39f, this.RefreshGame, this.ClientRectangle);
            game.Start();
            return game;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (game != null)
            {
                ((Game)game).setTarget(e.X, e.Y);
                if (!game.Started)
                    game.Start();
            }
        }

        protected override void RefreshGame()
        {
            base.RefreshGame();
            this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    this.Text = game.Score.ToString("0.0");
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

                //Text = ClientSize.ToString();
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
                //{
                //    if (!game.Started)
                //        game.Start();
                game.Paused = !game.Paused;
            //}
        }
    }
}
