using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MattUtil;

namespace RandomWalk
{
    public partial class ViewForm : Form
    {
        private List<Walk> walks;

        public ViewForm()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, true);
            this.BackColor = Color.Transparent;

            this.InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.TransparencyKey = this.BackColor;
            this.Bounds = Screen.GetBounds(this);

            this.walks = new List<Walk>();
            this.Reset();
        }

        private void Reset()
        {
            foreach (Walk walk in walks)
                walk.Stop();

            walks.Clear();
            int num = Walk.rand.Round(1.3 + Walk.rand.GaussianOE(2.6, .169, .21));
            for (int a = 0 ; a < num ; ++a)
                walks.Add(new Walk(Invalidate, RandomColor(), 1 + Walk.rand.GaussianOEInt(2.1, .39, .39),
                        Walk.rand.Bool(), Walk.rand.OE(), Walk.rand.OE(650), Walk.rand.Weighted(.26), Walk.rand.Weighted(.13)));

            foreach (Walk walk in walks)
                walk.Start();
        }

        private static Color RandomColor()
        {
            return Color.FromArgb(Walk.rand.Next(256), Walk.rand.Next(256), Walk.rand.Next(256));
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
                double minX = -13, minY = -13, maxX = 13, maxY = 13;
                foreach (PointD point in walks.SelectMany(walk => walk.Points))
                {
                    minX = Math.Min(minX, point.X);
                    minY = Math.Min(minY, point.Y);
                    maxX = Math.Max(maxX, point.X);
                    maxY = Math.Max(maxY, point.Y);
                }

                double scaleX = ClientSize.Width / ( maxX - minX );
                double scaleY = ClientSize.Height / ( maxY - minY );

                foreach (Walk walk in walks)//Walk.rand.Iterate(walks))
                    using (Pen pen = new Pen(walk.Color, walk.Size))
                    {
                        PointF[] points = walk.Points.Select(point =>
                                new PointF((float)( ( point.X - minX ) * scaleX ), (float)( ( point.Y - minY ) * scaleY ))).ToArray();
                        if (points.Length > 1)
                            e.Graphics.DrawLines(pen, points);
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
