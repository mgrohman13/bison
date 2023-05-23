using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NCWMap
{
    public partial class NewMap : Form
    {
        private const float bottomSpace = 65f;
        private const float padding = 6f;

        public NewMap()
        {
            Program.CreateMap();

            InitializeComponent();
            ResizeRedraw = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                GetScale(out float xs, out float ys);

                if (xs > 1 && ys > 1)
                {
                    int idx = 0;
                    RectangleF[] rects = new RectangleF[18 * 18];

                    using (Font font = new("arial", ys / 2.6f))
                        for (int x = 0; x < 18; ++x)
                            for (int y = 0; y < 18; ++y)
                            {
                                RectangleF r = new(padding + x * xs + (y % 2 == 0 ? xs / 2f : 0), padding + y * ys, xs, ys);
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
                                            for (int a = 0; a < 2; ++a)
                                            {
                                                string str = inf[a];
                                                if (str != null)
                                                {
                                                    float len = e.Graphics.MeasureString(str, font).Width;
                                                    e.Graphics.DrawString(str, font, Brushes.Black, r.X + (xs - len) / 2f, sy);
                                                }
                                                sy += ys / 2f;
                                            }
                                            break;
                                        case 4:
                                            for (int b = 0; b < 2; ++b)
                                            {
                                                for (int a = 0; a < 2; ++a)
                                                {
                                                    string str = inf[b * 2 + a];
                                                    if (str != null)
                                                    {
                                                        float len = e.Graphics.MeasureString(str, font).Width;
                                                        e.Graphics.DrawString(str, font, Brushes.Black,
                                                                r.X + (xs / 2f - len) / 2f + xs / 2f * a, sy);
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
                }

                if (ClientSize.Height > bottomSpace)
                {
                    if (Program.Players != null)
                        using (Font font = new("arial", 8f))
                        {
                            float xInc = ClientSize.Width / (float)Program.Players.Length;
                            float yInc = 10f;

                            float xInf = 0f;
                            for (int a = 0; a < Program.Players.Length; ++a)
                            {
                                float yInf = ClientSize.Height - bottomSpace;
                                Player player = Program.Players[a];

                                DrawString(e, font, xInf, ref yInf, xInc, yInc, player.Name);
                                DrawString(e, font, xInf, ref yInf, xInc, yInc, player.Unit);
                                DrawString(e, font, xInf, ref yInf, xInc, yInc, player.relic.ToString());
                                DrawString(e, font, xInf, ref yInf, xInc, yInc, player, 0);
                                DrawString(e, font, xInf, ref yInf, xInc, yInc, player, 1);
                                DrawString(e, font, xInf, ref yInf, xInc, yInc, player, 2);

                                xInf += xInc;
                            }
                        }
                }

                this.Text = string.Format("{0} - {1}", xs.ToString("0.0"), ys.ToString("0.0"));
            }
            catch (Exception exception)
            {
                Program.Log(exception);
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
            xInf += (xInc - len) / 2f;

            e.Graphics.DrawString(str, font, Brushes.Black, xInf, yInf);

            yInf += yInc;
        }

        private void NewMap_Resize(object sender, EventArgs e)
        {
            GetScale(out _, out float ys);
            ClientSize = new Size((int)Math.Round(ys * 18.5f + padding * 2f), ClientSize.Height);
        }

        private void GetScale(out float xs, out float ys)
        {
            xs = (ClientSize.Width - padding * 2f) / 18.5f;
            ys = (ClientSize.Height - padding * 2f - bottomSpace) / 18f;
        }

        private void ShowMap_Click(object sender, EventArgs e)
        {
            //Program.DoMore();
            if (Program.DoStep())
                MessageBox.Show("done");
            else
                this.Refresh();
        }

        private void NewMap_Load(object sender, EventArgs e)
        {
            NewMap_Resize(sender, e);
        }
    }
}
