using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gravity
{
    class Score : Piece
    {
        const int lasts = 125;
        private int score, time;

        public Score(Game game, float x, float y, float size, float score) : base(game, x, y, size, 0, System.Drawing.Color.Green)
        {
            this.score = Game.rand.Round(score);
            this.time = lasts;
        }
        internal override void Step(float count)
        {
            if (--time > 0)
            {
                float val = 1f - time / 1.5f / lasts;
                int other = Game.rand.Round(255f * val * val * val);
                this.color = Color.FromArgb(other, Game.rand.Round(255f * val), other);
            }
            else
            {
                game.Remove(this);
            }
        }

        public override void Draw(Graphics graphics, Rectangle drawRectangle)
        {
            float size = (float)(10f * Math.Pow(score / 100f, .15f));
            if (size > 0)
            {
                RectangleF rect = GetPieceRectangle(drawRectangle);
                using (Font f = new Font("Lucida Console", size, FontStyle.Bold))
                {
                    string str = "+" + score.ToString();
                    SizeF drawSize = graphics.MeasureString(str, f);
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
                    graphics.DrawString(str, f, getBrush(color), p.X, p.Y);
                }
            }
        }


        internal override float GetGravity(Type type)
        {
            throw new Exception();
        }
    }
}
