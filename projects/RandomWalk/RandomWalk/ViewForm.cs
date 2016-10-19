using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MattUtil;

namespace RandomWalk
{
    public partial class ViewForm : Form
    {
        const double avg = 5.2;
        double minX = -13, minY = -13, maxX = 13, maxY = 13;
        private List<Walk> walks;

        public ViewForm()
        {
            ControlStyles flag = ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint;
            if (Environment.OSVersion.Version.Major != 5 || Environment.OSVersion.Version.Minor != 1)
                flag |= ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint;
            this.SetStyle(flag, true);
            this.BackColor = Color.Transparent;

            this.InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.TransparencyKey = this.BackColor;

            this.Bounds = Screen.AllScreens.Aggregate(new Rectangle(0, 0, 0, 0), (rect, screen) => Rectangle.Union(rect, screen.Bounds));

            Thread t1 = new Thread(() =>
            {
                while (true)
                {
                    int sleep = Walk.rand.OEInt(13000);
                    Console.WriteLine("wait: " + sleep);
                    Thread.Sleep(sleep);
                    lock (walks)
                        if (this.walks.Any(w => w.Active) && Walk.rand.Bool(this.walks.Count / ( this.walks.Count + avg )))
                        {
                            Console.WriteLine("deactivate");
                            int idx = Walk.rand.Next(this.walks.Count);
                            this.walks[idx].Deactivate();
                        }
                        else if (Walk.rand.Bool(avg / ( this.walks.Count + avg )))
                        {
                            Console.WriteLine("add");
                            this.walks.Add(RandWalk());
                        }
                }
            });
            t1.IsBackground = true;
            t1.Start();

            Thread t2 = new Thread(() =>
            {
                int i = 0;
                while (true)
                {
                    Thread.Sleep(39);
                    double minX = -13, minY = -13, maxX = 13, maxY = 13;
                    lock (walks)
                        foreach (PointD point in walks.SelectMany(walk => walk.Points))
                        {
                            minX = Math.Min(minX, point.X);
                            minY = Math.Min(minY, point.Y);
                            maxX = Math.Max(maxX, point.X);
                            maxY = Math.Max(maxY, point.Y);
                        }
                    bool mod = false;
                    Func<double, double, double> Mod = new Func<double, double, double>((v1, v2) =>
                    {
                        const double factor = .0169;
                        double newVal = ( v1 * ( 1 - factor ) ) + ( v2 * factor );
                        if (Math.Abs(v2 - newVal) < .039)
                            newVal = v2;
                        if (v1 != newVal)
                            mod = true;
                        return newVal;
                    });
                    this.minX = Mod(this.minX, minX);
                    this.minY = Mod(this.minY, minY);
                    this.maxX = Mod(this.maxX, maxX);
                    this.maxY = Mod(this.maxY, maxY);
                    if (mod)
                        Invalidate();
                    else
                        Console.WriteLine(++i);
                }
            });
            t2.IsBackground = true;
            t2.Start();

            this.walks = new List<Walk>();
            this.Reset();
        }

        private void Reset()
        {
            lock (walks)
            {
                foreach (Walk walk in walks)
                    walk.Stop();

                walks.Clear();
                int num = Walk.rand.GaussianOEInt(avg, .169, .21, 1);
                for (int a = 0 ; a < num ; ++a)
                    walks.Add(RandWalk());
            }
        }

        private Walk RandWalk()
        {
            Walk walk = new Walk(Invalidate, RandomColor(), 1 + Walk.rand.GaussianOEInt(2.1, .39, .39), Walk.rand.Bool(),
                    Walk.rand.OE(.52), Walk.rand.OE(), Walk.rand.OE(780), Walk.rand.Weighted(.26), Walk.rand.Weighted(.13), Walk.rand.GaussianOE(169, .26, .13));
            walk.Start();
            return walk;
        }

        private static Color RandomColor()
        {
            Func<int> Gen = () => Walk.rand.Next(256);
            return Color.FromArgb(Gen(), Gen(), Gen());
        }

        protected override CreateParams CreateParams
        {
            get
            {
                // Activate double buffering at the form level.  All child controls will be double buffered as well.
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;   // WS_EX_COMPOSITED
                return cp;
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                Func<double, double, double, double> Scale = (s, x, m) => s / ( x - m );
                double scaleX = Scale(ClientSize.Width, maxX, minX);
                double scaleY = Scale(ClientSize.Height, maxY, minY);

                lock (walks)
                    foreach (Walk walk in walks.ToArray())
                        using (Pen pen = new Pen(walk.Color, walk.Size))
                        {
                            Func<double, double, double, float> GetP = (p, m, s) => (float)( ( p - m ) * s );
                            PointF[] points = walk.Points.Select(point =>
                                    new PointF(GetP(point.X, minX, scaleX), GetP(point.Y, minY, scaleY))).ToArray();
                            if (points.Length > 1)
                            {
                                e.Graphics.DrawCurve(pen, points, (float)walk.Tension);
                            }
                            else if (points.Length == 0)
                            {
                                Console.WriteLine("remove");
                                this.walks.Remove(walk);
                            }
                        }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.StackTrace);
                Console.WriteLine();
            }
        }

        private void ViewForm_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void ViewForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            Close();
        }
    }
}
