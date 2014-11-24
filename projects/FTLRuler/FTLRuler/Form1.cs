using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FTLRuler
{
    public partial class Form1 : Form
    {
        private const float diameter = 2048f, _100 = 64f, _70 = _100 * .70f, _50 = _100 * .50f;

        private Dictionary<int, float> jumps = new Dictionary<int, float>();

        public Form1()
        {
            InitializeComponent();

            this.ResizeRedraw = true;
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.MouseWheel += new MouseEventHandler(Form1_MouseWheel);

            Rectangle bounds = Screen.GetBounds(this);
            bounds.Inflate(bounds.Width / -13, bounds.Height / -13);
            this.Bounds = bounds;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            float y = GetY();
            Loop((x, idx) =>
            {
                e.Graphics.DrawEllipse(Pens.White, x, y, diameter, diameter);
                return false;
            });
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                float val = ( e.Delta < 0 ? _50 : _70 );
                for (int idx = 0 ; idx < this.Width / val ; ++idx)
                    jumps[idx] = val;
            }
            this.Invalidate();
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right)
            {
                jumps.Clear();
            }
            else
            {
                Func<float, float, float, float, float> GetDistSqr = (c1, v1, c2, v2) => ( c1 - v1 ) * ( c1 - v1 ) + ( c2 - v2 ) * ( c2 - v2 );
                float centerY = GetY() + diameter / 2f;
                const float radiusSqr = diameter * diameter / 4f;
                Loop((x, idx) =>
                {
                    if (GetDistSqr(x + diameter / 2f, e.X, centerY, e.Y) < radiusSqr)
                    {
                        for (int i = 0 ; i < ( e.Button == MouseButtons.Right ? 2 : 1 ) ; ++i)
                        {
                            float val, newVal;
                            jumps.TryGetValue(idx, out val);
                            if (val == _70)
                                newVal = _100;
                            else if (val == _50)
                                newVal = _70;
                            else
                                newVal = _50;
                            jumps[idx] = newVal;
                        }
                        return true;
                    }
                    return false;
                });
            }
            this.Invalidate();
        }

        private float GetY()
        {
            return this.Height / 2f - diameter / 2f;
        }

        private void Loop(Func<float, int, bool> Callback)
        {
            float x = -diameter + _100 / 2f;
            int idx = -1;
            while (x + diameter - _100 * 2f < this.Width)
            {
                if (Callback(x, idx))
                    break;
                float val;
                if (jumps.TryGetValue(++idx, out val))
                    x += val;
                else
                    x += _100;
            }
        }
    }
}
