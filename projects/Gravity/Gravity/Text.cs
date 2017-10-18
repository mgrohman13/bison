using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gravity
{
    class Text : Piece
    {
        private const int lasts = 125;

        private string text;
        private int time;
        private bool r, g, b;

        public Text(Game game, float x, float y, string text, float size, bool r, bool g, bool b) : base(game, x, y, size, 0, System.Drawing.Color.Black)
        {
            this.text = text;
            this.r = r;
            this.g = g;
            this.b = b;
            this.time = lasts;
        }
        internal override void Step(float count)
        {
            if (--time > 0)
            {
                float val = 1f - time / 1.75f / lasts;
                int main = Game.rand.Round(255f * val);
                int other = Game.rand.Round(255f * val * val * val);
                this.color = Color.FromArgb(r ? main : other, g ? main : other, b ? main : other);
            }
            else
            {
                game.Remove(this);
            }
        }

        public override void Draw(Graphics graphics, Rectangle drawRectangle)
        {
            if (size > 0)
            {
                RectangleF rect = GetPieceRectangle(drawRectangle);
                using (Font f = new Font("Lucida Console", size, FontStyle.Bold))
                {
                    SizeF drawSize = graphics.MeasureString(text, f);
                    PointF p = new PointF(rect.X + rect.Width / 2f - drawSize.Width / 2f, rect.Y + rect.Height / 2f - drawSize.Height / 2f);
                    if (p.X < drawRectangle.Left)
                        p.X = drawRectangle.Left;
                    else if (p.X + drawSize.Width > drawRectangle.Right)
                        p.X = drawRectangle.Right - drawSize.Width;
                    if (p.Y < drawRectangle.Top)
                        p.Y = drawRectangle.Top;
                    else if (p.Y + drawSize.Height > drawRectangle.Bottom)
                        p.Y = drawRectangle.Bottom - drawSize.Height;
                    p.Y += 1f + size / 10f;
                    graphics.DrawString(text, f, getBrush(color), p.X, p.Y);
                }
            }
        }


        internal override float GetGravity(Type type)
        {
            throw new Exception();
        }
    }
}
