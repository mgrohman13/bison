﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NCWMap
{
    public partial class ShowMap : Form
    {
        public ShowMap()
        {
            InitializeComponent();
            ResizeRedraw = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            const float bottomSpace = 52f;

            const float padding = 6f;
            float xs = ( ClientSize.Width - padding * 2f ) / (float)( Program.Width + .5f );
            float ys = ( ClientSize.Height - padding * 2f - bottomSpace ) / (float)( Program.Height );

            int idx = 0;
            RectangleF[] rects = new RectangleF[Program.Width * Program.Height];

            using (Font font = new Font("arial", ys / 2.6f))
                for (int x = 0 ; x < Program.Width ; ++x)
                    for (int y = 0 ; y < Program.Height ; ++y)
                    {
                        RectangleF r = new RectangleF(padding + x * xs + ( y % 2 == 0 ? xs / 2f : 0 ), padding + y * ys, xs, ys);
                        rects[idx++] = r;
                        if (Program.Map[x, y].Water)
                            e.Graphics.DrawLine(Pens.Black, r.X, r.Y + ys / 2f, r.X + xs, r.Y + ys / 2f);

                        string[] inf = Program.Map[x, y].Inf;
                        if (inf != null)
                        {
                            float sy = r.Y;
                            switch (inf.Length)
                            {
                            case 2:
                                for (int a = 0 ; a < 2 ; ++a)
                                {
                                    string str = inf[a];
                                    if (str != null)
                                    {
                                        float len = e.Graphics.MeasureString(str, font).Width;
                                        e.Graphics.DrawString(str, font, Brushes.Black, r.X + ( xs - len ) / 2f, sy);
                                    }
                                    sy += ys / 2f;
                                }
                                break;
                            case 4:
                                for (int b = 0 ; b < 2 ; ++b)
                                {
                                    for (int a = 0 ; a < 2 ; ++a)
                                    {
                                        string str = inf[b * 2 + a];
                                        if (str != null)
                                        {
                                            float len = e.Graphics.MeasureString(str, font).Width;
                                            e.Graphics.DrawString(str, font, Brushes.Black,
                                                    r.X + ( xs / 2f - len ) / 2f + xs / 2f * a, sy);
                                        }
                                    }
                                    sy += ys / 2f;
                                }
                                break;
                            default:
                                throw new Exception();
                            }
                        }
                    }

            e.Graphics.DrawRectangles(Pens.Black, rects);

            this.Text = string.Format("{0} - {1}", xs.ToString("0.0"), ys.ToString("0.0"));

            using (Font font = new Font("arial", 8f))
            {
                float xInc = ClientSize.Width / (float)Program.Players.Length;
                float yInc = 10f;

                float xInf = 0f;
                for (int a = 0 ; a < Program.Players.Length ; ++a)
                {
                    float yInf = ClientSize.Height - bottomSpace;
                    Player player = Program.Players[a];

                    DrawString(e, font, xInf, ref yInf, xInc, yInc, player.Name);
                    DrawString(e, font, xInf, ref yInf, xInc, yInc, player.Unit);
                    DrawString(e, font, xInf, ref yInf, xInc, yInc, player, 0);
                    DrawString(e, font, xInf, ref yInf, xInc, yInc, player, 1);
                    DrawString(e, font, xInf, ref yInf, xInc, yInc, player, 2);

                    xInf += xInc;
                }
            }
        }
        private void DrawString(PaintEventArgs e, Font font, float xInf, ref float yInf, float xInc, float yInc, Player p, int r)
        {
            string str = String.Format("{0}.{1}", p.Resources[r, 0], p.Resources[r, 1]);
            DrawString(e, font, xInf, ref yInf, xInc, yInc, str);
        }
        private void DrawString(PaintEventArgs e, Font font, float xInf, ref float yInf, float xInc, float yInc, string str)
        {
            float len = e.Graphics.MeasureString(str, font).Width;
            xInf += ( xInc - len ) / 2f;

            e.Graphics.DrawString(str, font, Brushes.Black, xInf, yInf);

            yInf += yInc;
        }

        private void ShowMap_Click(object sender, EventArgs e)
        {
            Program.DoMore();
            this.Refresh();
        }
    }
}
