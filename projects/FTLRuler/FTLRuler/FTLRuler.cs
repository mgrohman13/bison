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
    public partial class FTLRuler : Form
    {
        private const float diameter = 1520f, full = 64f, nSector = full * .80f, nBeacon = full * .50f;

        private Dictionary<int, float> jumps = new Dictionary<int, float>();
        private float current = full;

        public FTLRuler()
        {
            InitializeComponent();

            this.ResizeRedraw = true;
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.MouseWheel += new MouseEventHandler(FTLRuler_MouseWheel);

            this.TransparencyKey = this.BackColor = Color.Gray;

            this.Height = ( Screen.GetBounds(this).Height * 11 ) / 13;
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Func<Keys, bool> PressedKey = key => ( keyData & key ) == key;
            Action<int, int> MoveForm = (x, y) => this.Location = new Point(this.Location.X + x, this.Location.Y + y);

            int amt = 1;
            if (PressedKey(Keys.Alt))
                amt *= 2;
            if (PressedKey(Keys.Control))
                amt *= 15;
            if (PressedKey(Keys.Shift))
                amt *= 5;

            if (PressedKey(Keys.Down))
                MoveForm(0, amt);
            else if (PressedKey(Keys.Right))
                MoveForm(amt, 0);
            else if (PressedKey(Keys.Up))
                MoveForm(0, -amt);
            else if (PressedKey(Keys.Left))
                MoveForm(-amt, 0);

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void FTLRuler_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString().ToLower() == "m")
            {
                if (jumps.Count == 0)
                    if (current == full)
                        current = nSector;
                    else
                        current = full;
                Reset();
            }
            else
            {
                if (this.Opacity == 1)
                {
                    this.Opacity = .5;
                    this.TransparencyKey = Color.Empty;
                }
                else
                {
                    this.Opacity = 1;
                    this.TransparencyKey = this.BackColor;
                }
            }
        }

        private void FTLRuler_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                if (e.Delta > 0)
                    current = nSector;
                else
                    current = full;
                Reset();
            }
        }

        private void FTLRuler_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right)
            {
                Reset();
            }
            else
            {
                Func<float, float, float, float, float> GetDistSqr = (x1, x2, y1, y2) => ( x1 - x2 ) * ( x1 - x2 ) + ( y1 - y2 ) * ( y1 - y2 );
                const float radiusSqr = diameter * diameter / 4f;
                float centerY = GetY() + diameter / 2f;
                Loop((x, idx) =>
                {
                    if (GetDistSqr(x + diameter / 2f, e.X, centerY, e.Y) < radiusSqr)
                    {
                        float val, newVal;
                        if (!jumps.TryGetValue(idx, out val) || val == current)
                            if (current == full)
                                newVal = nBeacon;
                            else
                                newVal = full;
                        else
                            newVal = current;
                        jumps[idx] = newVal;

                        this.Invalidate();
                        return true;
                    }
                    return false;
                });
            }
        }

        private void Reset()
        {
            jumps.Clear();
            this.Invalidate();
        }

        private float GetY()
        {
            return this.Height / 2f - diameter / 2f;
        }

        private void Loop(Func<float, int, bool> Callback)
        {
            float x = -diameter + current / 2f;
            int idx = 0;
            while (x + diameter - current * 2f < this.Width)
            {
                if (Callback(x, idx))
                    break;
                float val;
                if (jumps.TryGetValue(++idx, out val))
                    x += val;
                else
                    x += current;
            }
        }
    }
}