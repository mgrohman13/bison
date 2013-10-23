using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Daemons;

namespace DaemonsWinApp
{
    public partial class InfoForm : Form
    {
        public static InfoForm Form = new InfoForm();

        private const int offset = 10, size = 100, ySize = 200, picSize = 21;

        private static Bitmap hits = new System.Drawing.Bitmap(@"pics\hits.bmp");
        private static Bitmap damage = new System.Drawing.Bitmap(@"pics\damage.bmp");
        private static Bitmap regen = new System.Drawing.Bitmap(@"pics\regen.bmp");
        private static Bitmap soul = new System.Drawing.Bitmap(@"pics\soul.bmp");

        private List<Unit> all, part, move;
        private bool showAll;
        private UseType use;
        private int y;

        private InfoForm()
        {
            InitializeComponent();
            this.MouseWheel += new MouseEventHandler(infoForm_MouseWheel);
        }

        public bool SetupStuff(ref List<Unit> all, ref  List<Unit> part, ref List<Unit> move, UseType use)
        {
            if (all != null)
                all.Sort(Unit.UnitComparison);
            if (part != null)
                part.Sort(Unit.UnitComparison);
            if (move != null)
                move.Sort(Unit.UnitComparison);

            this.all = all;
            this.part = part;
            this.move = move;
            this.showAll = ( use != UseType.Move );
            this.use = use;
            this.y = -1;

            Refresh();

            this.pnlMove.Visible = ( use == UseType.Move );
            this.FormBorderStyle = ( use == UseType.View ? FormBorderStyle.None : FormBorderStyle.Sizable );

            if (use == UseType.Build)
            {
                move.Clear();
                if (all.Count == 1)
                {
                    move.Add(all[0]);
                    return false;
                }
                move.Add(null);
            }

            this.vScrollBar1.Value = this.vScrollBar1.Minimum;
            AdjustMax();
            return true;
        }

        private int XMax
        {
            get
            {
                return this.ClientSize.Width / size;
            }
        }

        private void AdjustMax()
        {
            int max = GetMax(use == UseType.View ? 39 : 169);
            if (max > 0)
            {
                int val = this.vScrollBar1.Value;
                this.vScrollBar1.Value = this.vScrollBar1.Minimum;
                if (val > max)
                    val = max;
                this.vScrollBar1.Maximum = max;
                this.vScrollBar1.Value = val;
                this.vScrollBar1.Show();
            }
            else
            {
                this.vScrollBar1.Hide();
            }
        }

        private int GetMax(int add)
        {
            int needHeight = ( ( (List<Unit>)( showAll ? all : part ) ).Count + XMax - 1 ) / XMax * ySize;
            int height = this.Height;
            if (use == UseType.Move)
                height -= pnlMove.Height;
            if (height < needHeight && height > 0)
                return ( add + needHeight - height ) / 13;
            else
                return -1;
        }

        private void chbAll_CheckedChanged(object sender, EventArgs e)
        {
            showAll = this.chbAll.Checked;

            AdjustMax();
            Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            int x = offset, y = offset - this.vScrollBar1.Value * 13;
            const int incPic = picSize + 1;
            foreach (Unit u in ( showAll ? all : part ))
            {
                e.Graphics.DrawImage(u.GetPic(), x, y);

                if (use != UseType.View && move.Contains(u))
                    using (Pen p = new Pen(Color.Black, 3))
                        e.Graphics.DrawRectangle(p, x, y, 90, 90);

                double redPct = Math.Min(2 - u.Morale * 2, 1);
                double greenPct = Math.Min(u.Morale * 2, 1);
                using (Brush b = new SolidBrush(Color.FromArgb((int)( 255 * redPct ), (int)( 255 * greenPct ), 0)))
                {
                    e.Graphics.DrawRectangle(new Pen(Color.Black, 1), x - 1, y + size - 9, 92, 7);
                    int div = (int)( 91 * u.Morale + .5f );
                    e.Graphics.FillRectangle(Brushes.Black, x, y + size - 8, 91 - div, 6);
                    e.Graphics.FillRectangle(b, x + 91 - div, y + size - 8, div, 6);
                }

                using (Font f = new Font("Arial", 13))
                {
                    if (u.Movement > 0)
                        e.Graphics.DrawString(u.Movement.ToString(), f, Brushes.Black, x, y + 72);

                    x += 6;
                    y += size;

                    e.Graphics.DrawImage(damage, x, y);
                    e.Graphics.DrawImage(hits, x, y + incPic);
                    e.Graphics.DrawImage(regen, x, y + incPic * 2);
                    string rgnStr = u.Regen.ToString();
                    int incX = incPic + (int)e.Graphics.MeasureString(rgnStr, f).Width;
                    e.Graphics.DrawImage(soul, x + incX, y + incPic * 2);

                    x += picSize;

                    string str = "{0}";
                    if (u.DamageStr != u.BaseDamage.ToString())
                        str += " / {1}";
                    str = string.Format(str, u.DamageStr, u.BaseDamage);
                    e.Graphics.DrawString(str, f, Brushes.Black, new Point(x, y));

                    str = "{0}";
                    if (u.Hits != u.MaxHits)
                        str += " / {1}";
                    str = string.Format(str, u.Hits, u.MaxHits);
                    e.Graphics.DrawString(str, f, Brushes.Black, new Point(x, y + incPic));

                    e.Graphics.DrawString(string.Format("{0}", rgnStr), f, Brushes.Black, new Point(x, y + incPic * 2));
                    e.Graphics.DrawString(string.Format("{0}", ( (double)( u.Souls * ( 1 + u.HealthPct ) ) ).ToString("0")), f, Brushes.Black, new Point(x + incX, y + incPic * 2));
                }

                x -= picSize + 6;
                y -= size;

                x += size;
                if (x > XMax * size)
                {
                    y += ySize;
                    x = offset;
                }
            }
        }

        private void infoForm_MouseUp(object sender, MouseEventArgs e)
        {
            int x = ( e.X - offset ) / size;
            int y = ( e.Y - offset + this.vScrollBar1.Value * 13 ) / ySize;
            x += XMax * y;

            Unit u = null;
            if (showAll && x < all.Count)
                u = all[x];
            else if (x < part.Count)
                u = part[x];

            if (u != null)
            {
                if (use == UseType.Move)
                {
                    if (move.Contains(u))
                        move.Remove(u);
                    else
                        move.Add(u);
                }
                else if (use == UseType.Build)
                {
                    move[0] = u;
                    CloseForm();
                }
            }

            Refresh();
        }

        private void infoForm_KeyDown(object sender, KeyEventArgs e)
        {
            MainForm.shift = e.Shift;
        }

        private void infoForm_KeyUp(object sender, KeyEventArgs e)
        {
            MainForm.shift = e.Shift;
        }

        public void infoForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.vScrollBar1.Visible)
            {
                if (y >= 0 && use == UseType.View)
                    ScrollForm(e.Y - y, false);
                y = e.Y;
            }
        }

        private void infoForm_MouseWheel(object sender, MouseEventArgs e)
        {
            ScrollForm(Math.Sign(e.Delta) * -3, true);
        }

        private void ScrollForm(int amount, bool mouseWheel)
        {
            int max;
            if (mouseWheel)
            {
                max = GetMax(39);
                this.vScrollBar1.Maximum = max;
            }
            else
            {
                max = this.vScrollBar1.Maximum;
            }
            int newValue = this.vScrollBar1.Value + amount;
            if (newValue < this.vScrollBar1.Minimum)
                newValue = this.vScrollBar1.Minimum;
            else if (newValue > max)
                newValue = max;
            if (this.vScrollBar1.Value != newValue)
            {
                this.vScrollBar1.Value = newValue;
                Invalidate();
            }
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            this.Invalidate();
        }

        private void infoForm_SizeChanged(object sender, EventArgs e)
        {
            AdjustMax();
        }

        public static void ShowDialog(ref List<Unit> all, ref  List<Unit> part, ref List<Unit> move, UseType use)
        {
            if (Form.SetupStuff(ref all, ref part, ref move, use))
                if (use == UseType.View)
                    Form.Show();
                else
                    Form.ShowDialog();
        }

        public static void CloseForm()
        {
            if (Form.use == UseType.View)
                Form.Hide();
            else
                Form.Close();
        }
    }

    public enum UseType
    {
        Move,
        Build,
        View,
    }
}