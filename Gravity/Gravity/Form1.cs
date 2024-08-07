﻿using System;
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
            SetSquare();

            game = GetNewGame(true);
        }

        private void SetSquare()
        {
            int pad = menuStrip.Height + this.panel1.Height + this.panel2.Height;
            int min = Math.Min(ClientSize.Width, ClientSize.Height - pad);
            this.ClientSize = new Size(min, min + pad);
            setClientRectangle();
            Invalidate(this.ClientRectangle);
        }
        private void setClientRectangle()
        {
            if (game != null)
            {
                ((Game)game).setClientRectangle(new Rectangle(0, menuStrip.Height + this.panel2.Height, ClientSize.Width, ClientSize.Height - menuStrip.Height - this.panel1.Height - this.panel2.Height));
            }
        }

        protected override BaseGame GetNewGame(bool scoring)
        {
            game = new Game(scoring, 25, this.RefreshGame);
            SetSquare();
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
            panel2.Invalidate();

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
            setClientRectangle();
            Invalidate(ClientRectangle, false);
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
                SetSquare();
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
                {
                    float[] ticks = new float[] /**/ { .5f, 1, 2, 3, 4, 5, 6, 8, 10, 15, 20, 30 };
                    byte[] labels = new byte[]  /**/ { 001, 1, 1, 1, 1, 1, 1, 1, 01, 01, 01, 01 };
                    for (int a = 0; a < ticks.Length; a++)
                    {
                        float b = ticks[a];
                        float x = Player.GetShieldPct(b) * panel1.Width;
                        e.Graphics.DrawLine(pen, x, 0, x, panel1.Height);
                        if (labels[a] != 0)
                            e.Graphics.DrawString(b.ToString(), this.label1.Font, Brushes.Black, x, 0);
                    }

                }
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            if (game != null && !game.GameOver())
            {
                float w = panel2.Width / 2;
                float meter = ((Gravity.Game)game).DifficultyMeter;
                if (meter < 0)
                {
                    float pct = -Player.GetShieldPct(-meter);
                    e.Graphics.FillRectangle(Brushes.Green, w, 0, -pct * w, panel2.Height);
                }
                else
                {
                    float pct = Player.GetShieldPct(meter);
                    e.Graphics.FillRectangle(Brushes.Red, w * (1 - pct), 0, w * pct, panel2.Height);
                }

                this.label2.Text = "Difficulty - " + ((((Gravity.Game)game).Difficulty - 1f) * 500f).ToString("0");
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            SetSquare();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //Rectangle drawRectangle = new Rectangle(0, menuStrip.Height + this.panel2.Height, ClientSize.Width, ClientSize.Height - menuStrip.Height - this.panel1.Height - this.panel2.Height);
            ////float xScale = drawRectangle.Width / Game.gameSize;
            ////float yScale = drawRectangle.Height / Game.gameSize;
            //float max = getS(0);

            //for (int a = 0; true; a++)
            //{
            //    float dist = (float)(Game.gameSize * Math.Sqrt(2) * a / 256.0) / 2;

            //    float val = getS(dist);
            //    if (val == -1) break;

            //    byte v = (byte)(256 - val / max * 256f);
            //    Console.WriteLine(v);
            //    Pen b = new Pen(Color.FromArgb(v, v, 255), (float)(drawRectangle.Width * Math.Sqrt(2) / 256.0 / 2.0));

            //    float p = (float)(drawRectangle.Width * Math.Sqrt(2) * a / 256.0);

            //    RectangleF r = new RectangleF(-p / 2f + drawRectangle.Width / 2f, menuStrip.Height + this.panel2.Height - p / 2f + drawRectangle.Height / 2f, p, p);
            //    e.Graphics.DrawEllipse(b, r);

            //    if (highlight(max, val, new double[] { .01, .1, .2, .3, .4, .5, .6, .7, .8, .9, .99 }))
            //    {
            //        e.Graphics.DrawEllipse(Pens.Black, r);
            //    }
            //}
        }
        //private static bool highlight(float max, float val, double[] vs)
        //{
        //    return vs.Any(v => Math.Abs(val / max - v) < .0028);
        //}
        //private float getS(float dist)
        //{
        //    const float gameSizeSqr = Game.gameSize * Game.gameSize;
        //    float val = (float)Math.Pow((dist * dist * 4f + gameSizeSqr) / (gameSizeSqr * 3f), .25f) - 1f;
        //    //float cur = (float)Math.Sqrt(shield + 1f);
        //    //float mult = (val < 0 ? game.DiffMult(.5f, .9f, true) / cur : cur / -25f);
        //    if (val >= 0)
        //        return -1;
        //    val = (float)(.004f * Math.Log(Math.Abs(val) + 1f) * 1);
        //    return val;
        //}
    }
}
