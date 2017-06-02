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

        const float AlienShipFriendlyBulletDamageMult = 13f;
        const float BulletDamage = 3.9f;
        const float amt = AlienShipFriendlyBulletDamageMult * BulletDamage;
        const int AlienShipLife = 260;
        const float AlienDamageRandomness = .078f;
        const float AlienDamageOEPct = .26f;
        const float MoraleMax = .25f;// 1 - .0091f;
        const int tot = 100000;

        float[] results;

        public Form1()
        {
            InitializeComponent();
            reset();
        }
        private void Form1_MouseClick(object sender, EventArgs e)
        {
            reset();
            Refresh();
        }

        void reset()
        {
            float avg = 0;
            //bool b = rand.Bool();
            results = new float[tot];
            for (int a = 0 ; a < tot ; ++a)
            {
                //float dmg = rand.GaussianOE(amt, AlienDamageRandomness, AlienDamageOEPct);
                float dmg;
                //if (b)
                dmg = rand.Weighted(rand.Weighted(rand.DoubleHalf(1))) * AlienShipLife;
                //dmg = rand.Weighted(AlienShipLife, MoraleMax);
                //else
                //    dmg = rand.DoubleHalf(rand.DoubleHalf(AlienShipLife));
                //dmg = rand.OEFloat();
                //avg += dmg;
                results[a] = dmg;
            }
            //MessageBox.Show(b + "" + ( avg / tot ));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);

                //draw1(e.Graphics);
                draw2(e.Graphics);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        void draw1(Graphics g)
        {
            float mult = AlienShipLife / ( ClientSize.Width - 1f );
            float max = 1f;
            float[] cache = new float[ClientSize.Width];
            for (int a = 0 ; a < ClientSize.Width ; ++a)
            {
                float val = a * mult;
                float tot = results.Select(r =>
                {
                    float diff = Math.Abs(val - r) + 1f;
                    return 1f / diff / diff;
                }).OrderByDescending(r => r).Skip(Math.Max(1, rand.Round(amt / 666f))).Sum();

                //tot = (float)Math.Sqrt(tot);
                cache[a] = tot;
                max = Math.Max(max, tot);
            }
            mult = ( Height - 1 ) / max;
            PointF[] points = new PointF[ClientSize.Width];
            for (int a = 0 ; a < ClientSize.Width ; ++a)
            {
                int y = ( Height - 1 ) - rand.Round(cache[a] * mult);
                points[a] = new PointF(a, y);
            }
            g.DrawLines(Pens.Black, points);
        }
        void draw2(Graphics g)
        {
            float mVal = results.Max();
            float smooth = (float)trackBar1.Value;
            float mult = mVal / ( ClientSize.Width - 1f );
            float max = 1f;
            float[] cache = new float[ClientSize.Width];
            for (int x = 0 ; x < ClientSize.Width ; ++x)
            {
                float v1 = x * mult;
                float v2 = mult * smooth;
                float count = results.Count(r => Math.Abs(v1 - r) < v2);
                //count *= x / (float)ClientSize.Width;
                cache[x] = count;
                max = Math.Max(max, count);
            }
            //max = 60f;
            mult = ( Height - 1f ) / max;
            PointF[] points = new PointF[ClientSize.Width];
            for (int x = 0 ; x < ClientSize.Width ; ++x)
                points[x] = new PointF(x, rand.Round(Height - 1f - mult * cache[x]));
            g.DrawLines(Pens.Black, points);
            //MessageBox.Show(max.ToString());
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            int width = rand.Round(ClientSize.Width / 6f);
            if (trackBar1.Value > width)
                trackBar1.Value = width;
            trackBar1.Maximum = width;
        }
    }
}
