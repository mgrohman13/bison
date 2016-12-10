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
            results = new float[tot];
            for (int a = 0 ; a < tot ; ++a)
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

                float mult = AlienShipLife / ( Width - 1f );
                float max = 1f;
                float[] cache = new float[Width];
                for (int a = 0 ; a < Width ; ++a)
                {
                    float val = a * mult;
                    float tot = 0f;
                    foreach (float r in results)
                    {
                        float diff = Math.Abs(val - r) + 1f;
                        tot += 1f / diff / diff;
                    }
                    tot = (float)Math.Sqrt(tot);
                    cache[a] = tot;
                    max = Math.Max(max, tot);
                }
                mult = ( Height - 1 ) / max;
                PointF[] points = new PointF[Width];
                for (int a = 0 ; a < Width ; ++a)
                {
                    int y = ( Height - 1 ) - rand.Round(cache[a] * mult);
                    points[a] = new PointF(a, y);
                }
                e.Graphics.DrawLines(Pens.Black, points);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
