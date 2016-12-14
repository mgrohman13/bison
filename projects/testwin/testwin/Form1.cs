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
        const float AlienShipLife = 260f;
        const float AlienDamageRandomness = .078f;
        const float AlienDamageOEPct = .26f;
        const int tot = 10000;

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
            results = new float[tot];
            for (int a = 0; a < tot; ++a)
            {
                float dmg = rand.GaussianOE(amt, AlienDamageRandomness, AlienDamageOEPct);
                results[a] = dmg;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);

                draw1(e.Graphics);
                draw2(e.Graphics);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        void draw1(Graphics g)
        {
            float mult = AlienShipLife / (Width - 1f);
            float max = 1f;
            float[] cache = new float[Width];
            for (int a = 0; a < Width; ++a)
            {
                float val = a * mult;
                float tot =
                results.Select(r =>
                {
                    float diff = Math.Abs(val - r) + 1f;
                    return 1f / diff / diff;
                }).OrderByDescending(r => r).Skip(Math.Max(1, rand.Round(amt / 666f))).Sum();

                //tot = (float)Math.Sqrt(tot);
                cache[a] = tot;
                max = Math.Max(max, tot);
            }
            mult = (Height - 1) / max;
            PointF[] points = new PointF[Width];
            for (int a = 0; a < Width; ++a)
            {
                int y = (Height - 1) - rand.Round(cache[a] * mult);
                points[a] = new PointF(a, y);
            }
            g.DrawLines(Pens.Black, points);
        }
        void draw2(Graphics g)
        {
            float smooth = (float)trackBar1.Value;
            float mult = AlienShipLife / (Width - 1f);
            float max = 1f;
            float[] cache = new float[Width];
            for (int a = 0; a < Width; ++a)
            {
                float val = a * mult;
                float tot = 0f;
                foreach (float r in results)
                {
                    float diff = Math.Abs(val - r);
                    if (diff < mult * smooth)
                        ++tot;
                }
                cache[a] = tot;
                max = Math.Max(max, tot);
            }
            mult = (Height - 1) / max;
            PointF[] points = new PointF[Width];
            for (int a = 0; a < Width; ++a)
            {
                int y = (Height - 1) - rand.Round(cache[a] * mult);
                points[a] = new PointF(a, y);
            }
            g.DrawLines(Pens.Black, points);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            int width = rand.Round(Width / 6f);
            if (trackBar1.Value > width)
                trackBar1.Value = width;
            trackBar1.Maximum = width;
        }
    }
}
