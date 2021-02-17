using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Trogdor
{
    public partial class Stats : Form
    {
        const float spacing = 13f;
        float mult, maxTot, maxDec, maxCol;

        public Stats()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (Font font = new Font(FontFamily.GenericSansSerif, 13f))
            {
                float[] ys = new float[5];
                float x, y = 39f;
                ys[0] = y;
                x = DrawStat(e, font, maxTot, spacing, ref y, -1, Game.TotalPlayer, Type.Player);
                ys[1] = y;
                x = Math.Max(x, DrawStat(e, font, maxTot, spacing, ref y, -1, Game.TotalHut, Type.Hut));
                ys[2] = y;
                x = Math.Max(x, DrawStat(e, font, maxTot, spacing, ref y, -1, Game.TotalAlly, Type.Ally));
                ys[3] = y;
                x = Math.Max(x, DrawStat(e, font, maxTot, spacing, ref y, -1, Game.TotalEnemy, Type.Enemy));
                ys[4] = y;

                string str = "Total";
                e.Graphics.DrawString(str, font, Brushes.Black, (x - e.Graphics.MeasureString(str, font).Width + spacing) / 2.0f, 13f);

                float useX = x;
                y = ys[1];
                x = Math.Max(x, DrawStat(e, font, maxCol, useX, ref y, ys[2], Game.CollectHut, Type.Hut));
                y = ys[2];
                x = Math.Max(x, DrawStat(e, font, maxCol, useX, ref y, ys[3], Game.CollectAlly, Type.Ally));
                y = ys[3];
                x = Math.Max(x, DrawStat(e, font, maxCol, useX, ref y, ys[4], Game.CollectEnemy, Type.Enemy));

                str = "Hit";
                e.Graphics.DrawString(str, font, Brushes.Black, useX - spacing + (x - useX - e.Graphics.MeasureString(str, font).Width + spacing) / 2.0f, 13f);

                useX = x;
                y = ys[0];
                x = DrawStat(e, font, maxDec, useX, ref y, ys[1], Game.DecayPlayer, Type.Player);
                y = ys[1];
                x = Math.Max(x, DrawStat(e, font, maxDec, useX, ref y, ys[2], Game.DecayHut, Type.Hut));
                y = ys[2];
                x = Math.Max(x, DrawStat(e, font, maxDec, useX, ref y, ys[3], Game.DecayAlly, Type.Ally));
                y = ys[3];
                x = Math.Max(x, DrawStat(e, font, maxDec, useX, ref y, ys[4], Game.DecayEnemy, Type.Enemy));

                str = "Decay";
                e.Graphics.DrawString(str, font, Brushes.Black, useX - spacing + (x - useX - e.Graphics.MeasureString(str, font).Width + spacing) / 2.0f, 13f);

                if (ClientSize.Width < x)
                    this.Width += (int)x - ClientSize.Width + 1;
                y += this.ClientSize.Height - this.button1.Location.Y + spacing;
                if (ClientSize.Height < y)
                    this.Height += (int)y - ClientSize.Height + 1;
            }
        }

        private float DrawStat(PaintEventArgs e, Font font, float max, float x, ref float y, float nextY, double value, Type type)
        {
            float diameter = GetDiameter(value);
            if (nextY > 0)
                y += (nextY - spacing - y - diameter) / 2f;

            e.Graphics.FillEllipse(Piece.GetBrush(type), x + (max - diameter) / 2f, y, diameter, diameter);
            string str = (value * mult).ToString("f0");
            SizeF strSize = e.Graphics.MeasureString(str, font);
            x += spacing + max;
            e.Graphics.DrawString(str, font, Brushes.Black, x, y + (diameter - strSize.Height) / 2f);
            y += diameter + spacing;
            return (x + spacing + strSize.Width);
        }

        internal void Init()
        {
            InvokeMethod(() => this.Size = new Size(100, 100));

            float maxSize = (float)(210 * Math.Pow(Game.TotalPlayer + Game.TotalHut + Game.TotalAlly + Game.TotalEnemy, .26));
            maxTot = (float)Math.Max(Math.Max(Math.Max(Game.TotalPlayer, Game.TotalHut), Game.TotalAlly), Game.TotalEnemy);
            maxDec = (float)Math.Max(Math.Max(Math.Max(Game.DecayHut, Game.DecayAlly), Game.DecayEnemy), Game.DecayPlayer);
            maxCol = (float)Math.Max(Math.Max(Game.CollectHut, Game.CollectAlly), Game.CollectEnemy);

            mult = maxTot / maxSize;
            Game.TotalPlayer /= mult;
            Game.TotalHut /= mult;
            Game.TotalAlly /= mult;
            Game.TotalEnemy /= mult;
            Game.DecayHut /= mult;
            Game.DecayAlly /= mult;
            Game.DecayEnemy /= mult;
            Game.DecayPlayer /= mult;
            Game.CollectHut /= mult;
            Game.CollectAlly /= mult;
            Game.CollectEnemy /= mult;

            float diameter = GetDiameter(Game.TotalPlayer);
            float length = diameter;

            diameter = GetDiameter(Game.TotalHut);
            length += diameter;

            diameter = GetDiameter(Game.TotalAlly);
            length += diameter;

            diameter = GetDiameter(Game.TotalEnemy);
            length += diameter;

            InvokeMethod(() => this.ClientSize = new Size(300, Game.Random.Round(length) + 100));

            maxTot = GetDiameter(maxTot / mult);
            maxDec = GetDiameter(maxDec / mult);
            maxCol = GetDiameter(maxCol / mult);
        }

        private void InvokeMethod(MethodInvoker Method)
        {
            if (this.InvokeRequired)
                this.Invoke(Method);
            else
                Method();
        }

        private float GetDiameter(double area)
        {
            return (float)(Math.Sqrt(area / Math.PI)) * 2f;
        }
    }
}