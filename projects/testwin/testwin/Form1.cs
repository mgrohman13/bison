using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace testwin
{
    public partial class Form1 : Form
    {
        static MattUtil.MTRandom rand;
        static Form1()
        {
            rand = new MattUtil.MTRandom();
            rand.StartTick();
        }

        //const float AlienShipFriendlyBulletDamageMult = 13f;
        //const float BulletDamage = 3.9f;
        //const float amt = AlienShipFriendlyBulletDamageMult * BulletDamage;
        //const int AlienShipLife = 260;
        //const float AlienDamageRandomness = .21f;
        //const float AlienDamageOEPct = .21f;
        //const float MoraleMax = .25f;// 1 - .0091f;
        //const int tot = 100000;

        //public const int PlanetQualityMin = 0;
        //public const int PlanetQualityMax = 390;
        //public const double PlanetQualityOE = 65;
        //public const double AverageQuality = ( PlanetQualityMin + PlanetQualityMax ) / 2.0 + PlanetQualityOE;

        int flag;
        //float[] results;

        public Form1()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            //this.trackBar1.Visible = false;
            trackBar1.Minimum = 0;
            trackBar1.Maximum = 255;
            trackBar1.SmallChange = 1;
            trackBar1.LargeChange = 16;

            trackBar1.Value = rand.Next(256);
            flag = rand.Next(96);

            reset();
        }
        private void Form1_MouseClick(object sender, EventArgs e)
        {
            reset();
            this.Invalidate();
        }

        void reset()
        {
            ++flag;
            //float avg = 0;
            ////bool b = rand.Bool();
            //results = new float[tot];
            //for (int a = 0 ; a < tot ; ++a)
            //{
            //    //float dmg = rand.GaussianOE(amt, AlienDamageRandomness, AlienDamageOEPct);
            //    float dmg = (float)( rand.OE(PlanetQualityOE) + rand.Range(PlanetQualityMin, PlanetQualityMax) );
            //    //float dmg;
            //    //if (b)
            //    //dmg = rand.Weighted(rand.Weighted(rand.DoubleHalf(1))) * AlienShipLife;
            //    //dmg = rand.Weighted(AlienShipLife, MoraleMax);
            //    //else
            //    //    dmg = rand.DoubleHalf(rand.DoubleHalf(AlienShipLife));
            //    //dmg = rand.OEFloat();
            //    //avg += dmg;
            //    results[a] = dmg;
            //}
            ////MessageBox.Show(b + "" + ( avg / tot ));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);

                //draw1(e.Graphics);
                //draw2(e.Graphics);
                draw3(e.Graphics);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        //void draw1(Graphics g)
        //{
        //    float mult = AlienShipLife / ( ClientSize.Width - 1f );
        //    float max = 1f;
        //    float[] cache = new float[ClientSize.Width];
        //    for (int a = 0 ; a < ClientSize.Width ; ++a)
        //    {
        //        float val = a * mult;
        //        float tot = results.Select(r =>
        //        {
        //            float diff = Math.Abs(val - r) + 1f;
        //            return 1f / diff / diff;
        //        }).OrderByDescending(r => r).Skip(Math.Max(1, rand.Round(amt / 666f))).Sum();

        //        //tot = (float)Math.Sqrt(tot);
        //        cache[a] = tot;
        //        max = Math.Max(max, tot);
        //    }
        //    mult = ( Height - 1 ) / max;
        //    PointF[] points = new PointF[ClientSize.Width];
        //    for (int a = 0 ; a < ClientSize.Width ; ++a)
        //    {
        //        int y = ( Height - 1 ) - rand.Round(cache[a] * mult);
        //        points[a] = new PointF(a, y);
        //    }
        //    g.DrawLines(Pens.Black, points);
        //}
        //void draw2(Graphics g)
        //{
        //    float mVal = results.Max();
        //    float smooth = (float)trackBar1.Value;
        //    float mult = mVal / ( ClientSize.Width - 1f );
        //    float max = 1f;
        //    int xAvg = rand.Round(AverageQuality / mult);
        //    g.DrawLine(Pens.Black, xAvg, 0, xAvg, Height);
        //    float[] cache = new float[ClientSize.Width];
        //    for (int x = 0 ; x < ClientSize.Width ; ++x)
        //    {
        //        float v1 = x * mult;
        //        float v2 = mult * smooth;
        //        float count = results.Count(r => Math.Abs(v1 - r) < v2);
        //        //count *= x / (float)ClientSize.Width;
        //        cache[x] = count;
        //        max = Math.Max(max, count);
        //    }
        //    //max = 60f;
        //    mult = ( Height - 1f ) / max;
        //    PointF[] points = new PointF[ClientSize.Width];
        //    for (int x = 0 ; x < ClientSize.Width ; ++x)
        //        points[x] = new PointF(x, rand.Round(Height - 1f - mult * cache[x]));
        //    g.DrawLines(Pens.Black, points);
        //    //MessageBox.Show(max.ToString());
        //}
        private void draw3(Graphics gr)
        {
            if (flag % 96 < 48)
            {
                this.trackBar1.Visible = true;
                this.ClientSize = new Size(256 + 2, 256 + this.trackBar1.Height + 2);

                int f = flag % 3;
                int f2 = 0;// ( flag / 3 ) % 2;
                int f3 = ( flag / 3 ) % 4;
                int o = this.trackBar1.Value;

                Bitmap i = new Bitmap(256, 256);
                for (int x = 0 ; x < 256 ; x++)
                    for (int y = 0 ; y < 256 ; y++)
                    {
                        int r = f == 0 ? o : ( f == 1 ^ f2 == 0 ? x : y );
                        int g = f == 1 ? o : ( f == 2 ^ f2 == 0 ? x : y );
                        int b = f == 2 ? o : ( f == 0 ^ f2 == 0 ? x : y );
                        i.SetPixel(f3 < 2 ? x : 255 - x, f3 % 2 == 0 ? y : 255 - y, Color.FromArgb(r, g, b));
                    }

                gr.DrawImage(i, 0 + 1, this.trackBar1.Height + 1);
            }
            else
            {
                this.trackBar1.Visible = false;

                Bitmap i = new Bitmap(ClientSize.Width, ClientSize.Height);

                for (int xx = 0 ; xx < ClientSize.Width ; ++xx)
                    for (int yy = 0 ; yy < ClientSize.Height ; ++yy)
                    {
                        double x = flag % 4 < 2 ? ( xx + .5 ) / (double)( ClientSize.Width ) : ( xx ) / (double)( ClientSize.Width - 1 );
                        double y = flag % 2 == 0 ? ( yy + .5 ) / (double)( ClientSize.Height ) : ( yy ) / (double)( ClientSize.Height - 1 );
                        //int x = rand.Round(xxx * 4095);
                        //int y = rand.Round(yyy * 4095);

                        int j, k, l;
                        //j = x % 256;
                        //k = y % 256;
                        //l = ( x / 256 ) * 16 + ( y / 256 );
                        y = Math.Sqrt(y);
                        j = rand.Round(x * y * 255);
                        k = rand.Round(( 255 - x * 255 ) * y);
                        l = rand.Round(255 - y * 255);


                        int r, g, b;
                        int f4 = ( flag / 4 ) % 6;
                        switch (f4)
                        {
                        case 0:
                            r = j;
                            g = k;
                            b = l;
                            break;
                        case 1:
                            r = j;
                            b = k;
                            g = l;
                            break;
                        case 2:
                            g = j;
                            r = k;
                            b = l;
                            break;
                        case 3:
                            g = j;
                            b = k;
                            r = l;
                            break;
                        case 4:
                            b = j;
                            r = k;
                            g = l;
                            break;
                        case 5:
                            b = j;
                            g = k;
                            r = l;
                            break;
                        default:
                            throw new Exception();
                        }
                        i.SetPixel(xx, yy, Color.FromArgb(r, b, g));
                    }

                gr.DrawImage(i, 0, 0);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            //int width = rand.Round(ClientSize.Width / 6f);
            //if (trackBar1.Value > width)
            //    trackBar1.Value = width;
            //trackBar1.Maximum = width;
            this.Invalidate();
        }
        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }
    }
}
