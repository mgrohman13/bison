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

        public bool SetupStuff(ref List<Unit> all, ref  List<Unit> part, ref List<Unit> move, UseType use)
        {
            l = null;
            showing = null;

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
                return ( ( use == UseType.View ? 39 : 182 ) + needHeight - height ) / 13;
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
                foreach (Unit u in ( showAll ? all : part ))
                {
                    e.Graphics.DrawImage(u.GetPic(), x, y);

                    if (use != UseType.View && move.Contains(u))
                        using (Pen p = new Pen(Color.Black, 3))
                            e.Graphics.DrawRectangle(p, x, y, 90, 90);

                    Color color;
                    double pct;
                    if (u.Morale < Consts.MoraleCritical)
                    {
                        double turns = Consts.GetMoraleTurns(u.Morale, Consts.MoraleCritical);
                        double max = Consts.GetMoraleTurns(double.Epsilon, Consts.MoraleCritical);
                        int brightness = (int)( 127 + 128 * Math.Pow(turns / max, .6) );
                        color = Color.FromArgb(brightness, brightness, brightness);
                        pct = 1;
                    }
                    else
                    {
                        double redPct = Math.Min(2 - u.Morale * 2, 1);
                        double greenPct = Math.Min(u.Morale * 2, 1);
                        color = Color.FromArgb((int)( 255 * redPct ), (int)( 255 * greenPct ), 0);
                        pct = u.Morale;
                    }
                    using (Brush b = new SolidBrush(color))
                    using (Pen p = new Pen(Color.Black, 1))
                    {
                        e.Graphics.DrawRectangle(p, x - 1, y + size - 9, 92, 7);
                        int div = (int)( 91 * pct + .5f );
                        if (pct < .995 && div == 91)
                            --div;
                        else if (pct > .995 && div != 91)
                            div = 91;
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
                        string rgnStr = u.Regen.ToString();
                        int incX = incPic + (int)e.Graphics.MeasureString(rgnStr, f).Width;
                        e.Graphics.DrawImage(soul, x + incX, tempY);

                        x += picSize;

                        tempY = y;
                        string str = string.Format("{0} ", u.Movement);
                        e.Graphics.DrawString(str, u.Movement > 0 ? b : f, Brushes.Black, new Point(x, tempY));
                        if (u.ReserveMovement > 0)
                            e.Graphics.DrawString(string.Format("({0})", u.Movement + u.ReserveMovement), u.ReserveMovement > 0 ? b : f, Brushes.Gray, new PointF(x + e.Graphics.MeasureString(str, f).Width, tempY));

                        tempY += incPic;
                        str = "{0}";
                        if (u.DamageStr != u.DamageMax.ToString())
                            str += " / {1}";
                        str = string.Format(str, u.DamageStr, u.DamageMax);
                        e.Graphics.DrawString(str, u.RecoverDmg > 0 ? f : b, Brushes.Black, new Point(x, tempY));

                        tempY += incPic;
                        str = "{0}";
                        if (u.Hits != u.HitsMax)
                            str += " / {1}";
                        str = string.Format(str, u.Hits, u.HitsMax);
                        e.Graphics.DrawString(str, u.Hits == u.HitsMax ? b : f, Brushes.Black, new Point(x, tempY));

                        tempY += incPic;
                        e.Graphics.DrawString(string.Format("{0}", rgnStr), f, Brushes.Black, new Point(x, tempY));
                        e.Graphics.DrawString(string.Format("{0}", ( (double)( u.Souls * ( 1 + u.HealthPct ) ) ).ToString("0")), f, Brushes.Black, new Point(x + incX, tempY));
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
            string morale = null, damage = null;
            if (u != null)
            {
                morale = u.Morale.ToString("0%");
                damage = u.Damage.ToString("0.0");
            }
            if (u != null && ( morale != "100%" || damage != u.DamageMax.ToString("0.0") ))
            {
                if (u != showing)
                {
                    string recover = u.RecoverFull.ToString("0.0"),
                            r2 = u.RecoverDmg.ToString("0.0"), r3 = u.RecoverCritical.ToString("0.0");
                    toolTip1.Show(string.Format("Morale: {0}{3}Recover: {1}{4}{5}{3}Max Damage: {2}",
                            morale, recover, damage, Environment.NewLine,
                            r2 == "0.0" ? "" : " / " + r2, r3 == "0.0" ? "" : " / " + r3),
                            this, e.X, e.Y, 13000);
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