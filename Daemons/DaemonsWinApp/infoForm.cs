using System;
using System.Collections.Generic;
using System.Linq;
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

        private static Bitmap movement = new System.Drawing.Bitmap(@"pics\move.bmp");
        private static Bitmap hits = new System.Drawing.Bitmap(@"pics\hits.bmp");
        private static Bitmap damage = new System.Drawing.Bitmap(@"pics\damage.bmp");
        private static Bitmap regen = new System.Drawing.Bitmap(@"pics\regen.bmp");
        private static Bitmap soul = new System.Drawing.Bitmap(@"pics\soul.bmp");

        private List<Unit> all, part, move;
        private bool showAll;
        private UseType use;

        private InfoForm()
        {
            InitializeComponent();
            this.MouseWheel += new MouseEventHandler(infoForm_MouseWheel);

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        }

        public bool SetupStuff(ref List<Unit> all, ref List<Unit> part, ref List<Unit> move, UseType use)
        {
            l = null;
            showing = null;

            if (all != null)
                all.Sort(new CurStrComparer());
            if (part != null)
                part.Sort(new CurStrComparer());
            if (move != null)
                move.Sort(new CurStrComparer());

            this.all = all;
            this.part = part;
            this.move = move;
            this.showAll = ( use != UseType.Move );
            this.chbAll.Checked = showAll;
            this.chbStr.Checked = ( use == UseType.Build );
            this.chbStr_CheckedChanged(null, null);
            this.vScrollBar1.Value = 0;
            this.use = use;

            Invalidate();

            this.pnlMove.Visible = ( use != UseType.View );
            this.btnOk.Visible = ( use == UseType.Move );
            this.chbAll.Visible = ( use == UseType.Move );
            this.FormBorderStyle = ( use == UseType.View ? FormBorderStyle.FixedSingle : FormBorderStyle.Fixed3D );

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

            AdjustMax();
            return true;
        }

        private void AdjustSize(List<Unit> units)
        {
            Size setSize;

            int maxWidth = MainForm.instance.Width - ( this.Width - this.ClientSize.Width );
            int maxHeight = MainForm.instance.Height - ( this.Height - this.ClientSize.Height );

            int maxX = maxWidth / size;
            if (( units.Count + maxX - 1 ) / maxX * ySize > maxHeight)
            {
                setSize = MainForm.instance.Size;
            }
            else
            {
                int diff = -1;
                int setWidth = -1, setHeight = -1;
                for (int testX = 1 ; testX <= maxX ; ++testX)
                {
                    int pxWidth = testX * size;
                    int pxHeight = ( ( units.Count + testX - 1 ) / testX * ySize );

                    int newDiff = pxWidth - pxHeight;
                    if (diff == -1 || ( newDiff > 0 && diff < 0 ) || newDiff < diff)
                    {
                        diff = newDiff;
                        setWidth = pxWidth;
                        setHeight = pxHeight;

                        if (units.Count == 1)
                            break;
                    }
                }

                setWidth += 21;
                if (use == UseType.Move && setWidth < 300)
                    setWidth = 300;
                setHeight += 21;
                setSize = new Size(setWidth, setHeight + ( use == UseType.View ? 39 : 78 ));
            }

            if (this.Size != setSize)
                this.Size = setSize;

            int setX = MainForm.instance.Location.X + ( MainForm.instance.Width - this.Width ) / 2;
            int setY = MainForm.instance.Location.Y + ( MainForm.instance.Height - this.Height ) / 2;
            Point setLocation = new Point(setX, setY);

            if (this.Location != setLocation)
                this.Location = setLocation;
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
            int max = GetMax();
            if (max > 0)
            {
                int val = this.vScrollBar1.Value;
                if (val > max)
                    val = max;
                this.vScrollBar1.Value = 0;
                this.vScrollBar1.Maximum = max;
                this.vScrollBar1.Value = val;
                this.vScrollBar1.Show();
            }
            else
            {
                this.vScrollBar1.Hide();
            }
        }

        private int GetMax()
        {
            int needHeight = ( ( (List<Unit>)( showAll ? all : part ) ).Count + XMax - 1 ) / XMax * ySize;
            int height = this.Height;
            if (use == UseType.Move)
                height -= pnlMove.Height;
            if (height < needHeight && height > 0)
                return ( ( use == UseType.View ? 75 : 225 ) + needHeight - height ) / 13;
            else
                return -1;
        }

        private void chbAll_CheckedChanged(object sender, EventArgs e)
        {
            showAll = this.chbAll.Checked;

            AdjustMax();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                this.AdjustSize(showAll ? all : part);

                int x = offset, y = this.vScrollBar1.Value;
                if (!this.vScrollBar1.Visible)
                    y = 0;
                y = offset - y * 13;

                const int incPic = picSize + 1;
                foreach (Unit unit in ( showAll ? all : part ))
                {
                    e.Graphics.DrawImage(unit.GetPic(), x, y);

                    if (use != UseType.View && move.Contains(unit))
                        using (Pen p = new Pen(Color.Black, 3))
                            e.Graphics.DrawRectangle(p, x, y, 90, 90);

                    Color color;
                    double pct = unit.Morale;
                    if (pct < Consts.MoraleCritical)
                    {
                        double turns = Consts.GetMoraleTurns(pct, Consts.MoraleCritical);
                        double max = Consts.GetMoraleTurns(double.Epsilon, Consts.MoraleCritical);
                        int brightness = (int)( 86.5 + 169 * Math.Sqrt(turns / max) );
                        color = Color.FromArgb(brightness, brightness, brightness);
                        pct = 1;
                    }
                    else
                    {
                        if (pct > Consts.MoraleMax)
                            pct = 1;
                        double redPct = Math.Min(2 - pct * 2, 1);
                        double greenPct = Math.Min(pct * 2, 1);
                        color = Color.FromArgb((int)( 255 * redPct ), (int)( 255 * greenPct ), 0);
                    }
                    using (Brush b = new SolidBrush(color))
                    using (Pen p = new Pen(Color.Black, 1))
                    {
                        e.Graphics.DrawRectangle(p, x - 1, y + size - 9, 92, 7);
                        int div = (int)( 91 * pct + .5f );
                        if (pct < Consts.MoraleMax && div == 91)
                            --div;
                        e.Graphics.FillRectangle(Brushes.Black, x, y + size - 8, 91 - div, 6);
                        e.Graphics.FillRectangle(b, x + 91 - div, y + size - 8, div, 6);
                    }

                    x += 6;
                    y += size;

                    using (Font f = new Font("Arial", 13))
                    using (Font b = new Font("Arial", 13, FontStyle.Bold))
                    {
                        int tempY = y;
                        e.Graphics.DrawImage(movement, x, tempY);
                        tempY += incPic;
                        e.Graphics.DrawImage(damage, x, tempY);
                        tempY += incPic;
                        e.Graphics.DrawImage(hits, x, tempY);
                        tempY += incPic;
                        e.Graphics.DrawImage(regen, x, tempY);
                        string rgnStr = unit.Regen.ToString();
                        int incX = incPic + (int)e.Graphics.MeasureString(rgnStr, f).Width;
                        e.Graphics.DrawImage(soul, x + incX, tempY);

                        x += picSize;

                        tempY = y;
                        string str = string.Format("{0} ", unit.Movement);
                        e.Graphics.DrawString(str, unit.Movement > 0 ? b : f, Brushes.Black, new Point(x, tempY));
                        if (unit.ReserveMovement > 0)
                            e.Graphics.DrawString(string.Format("({0})", unit.Movement + unit.ReserveMovement), unit.ReserveMovement > 0 ? b : f, Brushes.Gray, new PointF(x + e.Graphics.MeasureString(str, f).Width, tempY));

                        tempY += incPic;
                        str = "{0}";
                        if (unit.DamageStr != unit.DamageMax.ToString())
                            str += " / {1}";
                        str = string.Format(str, unit.DamageStr, unit.DamageMax);
                        e.Graphics.DrawString(str, unit.RecoverDmg > 0 ? f : b, Brushes.Black, new Point(x, tempY));

                        tempY += incPic;
                        str = "{0}";
                        if (unit.Hits != unit.HitsMax)
                            str += " / {1}";
                        str = string.Format(str, unit.Hits, unit.HitsMax);
                        e.Graphics.DrawString(str, unit.Hits == unit.HitsMax ? b : f, Brushes.Black, new Point(x, tempY));

                        tempY += incPic;
                        e.Graphics.DrawString(string.Format("{0}", rgnStr), f, Brushes.Black, new Point(x, tempY));
                        e.Graphics.DrawString(string.Format("{0}", ( (double)( unit.Souls * ( 1 + unit.HealthPct ) ) ).ToString("0")), f, Brushes.Black, new Point(x + incX, tempY));
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
            catch (Exception E)
            {
                Console.WriteLine(E);
                throw E;
            }
        }

        private void infoForm_MouseUp(object sender, MouseEventArgs e)
        {
            Unit u = GetUnit(e);

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

            Invalidate();
        }

        private Unit GetUnit(MouseEventArgs e)
        {
            if (!this.ClientRectangle.Contains(e.Location))
                return null;

            int y = this.vScrollBar1.Value;
            if (!this.vScrollBar1.Visible)
                y = 0;
            y = ( e.Y - offset + y * 13 ) / ySize;
            int x = ( e.X - offset ) / size + XMax * y;

            if (use == UseType.View)
                if (x >= 0 && x < all.Count)
                    return all[x];
                else
                    return null;

            Unit u = null;
            if (x >= 0)
                if (showAll && x < all.Count)
                    u = all[x];
                else if (x < part.Count)
                    u = part[x];
            return u;
        }

        private void infoForm_KeyDown(object sender, KeyEventArgs e)
        {
            MainForm.shift = e.Shift;
        }

        private void infoForm_KeyUp(object sender, KeyEventArgs e)
        {
            MainForm.shift = e.Shift;
        }

        private Point? l;
        private Unit showing;
        public void infoForm_MouseMove(object sender, MouseEventArgs e)
        {
            Unit u = GetUnit(e);
            if (u != null && ( u.Morale < Consts.MoraleMax || u.Hits < u.HitsMax ))
            {
                if (u != showing)
                {
                    string morale = "100%";
                    if (u.Morale < Consts.MoraleMax)
                    {
                        morale = u.Morale.ToString("0%");
                        if (morale == "100%")
                            morale = "99%";
                    }
                    toolTip1.Show(string.Format("Morale: {3}{5}Recover: {0}{1}{2}{5}Damage: {4}",
                            u.RecoverFull.ToString("0.0"), u.RecoverDmg > 0 ? " / " + u.RecoverDmg.ToString("0.0") : "",
                            u.Morale < Consts.MoraleCritical ? " / " + u.RecoverCritical.ToString("0.0") : "",
                            morale, u.Damage.ToString("0.0"), Environment.NewLine), this, e.X, e.Y, 13000);
                }
                showing = u;
            }
            else
            {
                this.toolTip1.Hide(this);
                showing = null;
            }

            if (use == UseType.View && l.HasValue && Math.Abs(e.Y - l.Value.Y) > Math.Abs(e.X - l.Value.X) && this.vScrollBar1.Visible)
                ScrollForm(Math.Sign(e.Y - l.Value.Y) + ( e.Y - l.Value.Y ) / 9, false);

            l = e.Location;
        }

        private void infoForm_MouseWheel(object sender, MouseEventArgs e)
        {
            ScrollForm(Math.Sign(e.Delta) * -3, true);
        }

        private void ScrollForm(int amount, bool mouseWheel)
        {
            int max = GetMax();
            if (mouseWheel)
                max -= 10;
            if (max < 0)
                max = 0;

            int newValue = this.vScrollBar1.Value + amount;
            if (newValue < 0)
                newValue = 0;
            else if (newValue > max)
                newValue = max;

            if (this.vScrollBar1.Maximum != max || this.vScrollBar1.Value != newValue)
            {
                this.vScrollBar1.Value = 0;
                this.vScrollBar1.Maximum = max;
                this.vScrollBar1.Value = newValue;

                Invalidate();
            }
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            ScrollForm(0, false);
            this.Invalidate();
        }

        private void infoForm_SizeChanged(object sender, EventArgs e)
        {
            AdjustMax();
        }

        public static void ShowDialog(ref List<Unit> all, ref List<Unit> part, ref List<Unit> move, UseType use)
        {
            if (Form.SetupStuff(ref all, ref part, ref move, use))
                if (use == UseType.View)
                {
                    if (all.Count > 0)
                        Form.Show();
                }
                else
                {
                    Form.ShowDialog();
                }
        }

        public static void CloseForm()
        {
            if (Form.use == UseType.View)
                Form.Hide();
            else
                Form.DialogResult = DialogResult.Cancel;
        }

        private void chbStr_CheckedChanged(object sender, EventArgs e)
        {
            IComparer<Unit> c = this.chbStr.Checked ? (IComparer<Unit>)new MaxStrComparer() : (IComparer<Unit>)new CurStrComparer();
            for (List<Unit> l = all ; l != null ; l = l == all ? part : l == part ? move : null)
                l.Sort(c);
            this.Invalidate();
        }

        private class CurStrComparer : IComparer<Unit>
        {
            public int Compare(Unit x, Unit y)
            {
                return Math.Sign(y.Strength - x.Strength);
            }
        }
        private class MaxStrComparer : IComparer<Unit>
        {
            public int Compare(Unit x, Unit y)
            {
                return Math.Sign(y.StrengthMax - x.StrengthMax);
            }
        }
    }

    public enum UseType
    {
        Move,
        Build,
        View,
    }
}