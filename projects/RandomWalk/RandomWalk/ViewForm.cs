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
            InitializeComponent();
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            this.walks = new List<Walk>();
            int num = Walk.rand.Round(1.3 + Walk.rand.GaussianOE(2.6, .169, .21));
            for (int a = 0 ; a < num ; ++a)
                walks.Add(new Walk(this.Invalidate, Color.FromArgb(Walk.rand.Next(256), Walk.rand.Next(256), Walk.rand.Next(256)),
                        1 + Walk.rand.GaussianOEInt(1.3, .13, .13), Walk.rand.Bool(), Walk.rand.OE(), Walk.rand.OEInt(650), Walk.rand.Weighted(.21), Walk.rand.Weighted(.091)));

            foreach (Walk walk in walks)
                walk.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);

                double minX = -1, minY = 1, maxX = -1, maxY = 1;
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
    }
}
