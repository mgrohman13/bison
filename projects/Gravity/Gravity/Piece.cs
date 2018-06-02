using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Gravity
{
    abstract class Piece
    {
        private static Dictionary<Color, Brush> brushes = new Dictionary<Color, Brush>();
        private static Dictionary<Color, Pen> pens = new Dictionary<Color, Pen>();

        protected Game game;
        protected float x, y, xDir, yDir, size, density;
        protected Color color;

        protected Piece(Game game, float x, float y, float size, float density, Color color)
        {
            this.game = game;

            this.x = x;
            this.y = y;
            this.xDir = 0;
            this.yDir = 0;
            this.size = size;
            this.density = density;
            this.color = color;
        }

        public float X
        {
            get
            {
                return x;
            }
        }
        public float Y
        {
            get
            {
                return y;
            }
        }
        public float Size
        {
            get
            {
                return size;
            }
        }
        public float Density
        {
            get
            {
                return density;
            }
        }
        public float Mass
        {
            get
            {
                return GetMass(size, density);
            }
        }
        protected static float GetMass(float size, float density)
        {
            return size * size * density;
        }

        internal abstract float GetGravity(Type type);

        internal virtual void Interact(Piece piece)
        {
            if (piece is Player)
                piece.Interact(this);
            else
                Gravity(this, piece);
        }

        protected static void Gravity(Piece p1, Piece p2)
        {
            float xDist = p1.x - p2.x;
            float yDist = p1.y - p2.y;
            float distSqr = xDist * xDist + yDist * yDist;
            float distance = (float)Math.Sqrt(distSqr);
            if (distance > (p1.size + p2.size) / 2f)
            {
                float mult = Game.gravity * p1.Mass * p2.Mass / distSqr / distance;
                xDist *= mult;
                yDist *= mult;

                float g1 = p1.GetGravity(p2.GetType()) / p1.Mass;
                p1.xDir += g1 * -xDist;
                p1.yDir += g1 * -yDist;

                float g2 = p2.GetGravity(p1.GetType()) / p2.Mass;
                p2.xDir += g2 * xDist;
                p2.yDir += g2 * yDist;
            }
        }

        internal virtual void Step(float count)
        {
            x += xDir;
            y += yDir;

            float dist = Game.gameSize / 2f;
            if (x * x + y * y > dist * dist)//* Math.Sqrt(2))
            {
                //Console.WriteLine(Math.Sqrt(x * x + y * y) / dist);
                xDir = adj(xDir, x);
                yDir = adj(yDir, y);
            }
        }

        private float adj(float dir, float pos)
        {
            float trg = -pos * Game.offMapPull;
            return dir * (1 - Game.offMapPull) + trg * Game.offMapPull;
        }

        public virtual void Draw(Graphics graphics, Rectangle drawRectangle)
        {
            float xScale, yScale;
            RectangleF drawPiece = GetPieceRectangle(drawRectangle, out xScale, out yScale);
            if (drawPiece.IntersectsWith(drawRectangle))
            {
                graphics.FillEllipse(getBrush(color), drawPiece);
            }
            else if (size > 0)
            {
                //determine length and direction
                float x1 = getCenter(drawPiece, true), y1 = getCenter(drawPiece, false),
                        x2 = getCenter(drawRectangle, true), y2 = getCenter(drawRectangle, false);
                float dist = (float)Math.Sqrt(x * x + y * y);
                float l = (float)Math.Pow(dist / (Game.gameSize / 2f), 1f);
                float length = (1f - l / (l + 15f)) / 2f;
                float t = (float)Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
                float x3 = x2 + drawRectangle.Width * length * (x1 - x2) / t;
                float y3 = y2 + drawRectangle.Height * length * (y1 - y2) / t;
                x2 = (x1 - x2) / t * Game.gameSize / 2f * xScale + x2;
                y2 = (y1 - y2) / t * Game.gameSize / 2f * yScale + y2;
                x1 = x3;
                y1 = y3;

                //move to edge of screen
                float xTrg;
                if (x1 > x2)
                    xTrg = drawRectangle.Left;
                else
                    xTrg = drawRectangle.Right;
                float yTrg;
                if (y1 > y2)
                    yTrg = drawRectangle.Top;
                else
                    yTrg = drawRectangle.Bottom;
                if (Math.Abs(x2 - x1) > Math.Abs(y2 - y1))
                {
                    float xAdd = xTrg - x2;
                    float yAdd = Math.Sign(yTrg - y2) * Math.Abs(xAdd) * Math.Abs(y2 - y1) / Math.Abs(x2 - x1);
                    x1 += xAdd;
                    x2 += xAdd;
                    y1 += yAdd;
                    y2 += yAdd;
                }
                else
                {
                    float yAdd = yTrg - y2;
                    float xAdd = Math.Sign(xTrg - x2) * Math.Abs(yAdd) * Math.Abs(x2 - x1) / Math.Abs(y2 - y1);
                    y1 += yAdd;
                    y2 += yAdd;
                    x1 += xAdd;
                    x2 += xAdd;
                }

                //determine arrowhead
                const float offset = (float)(Math.PI / 5.0);
                float len = (float)Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2)) / 3f;
                float angle = (float)(Math.Atan2(y1 - y2, x1 - x2) - offset);
                float xA1 = (float)(x2 + len * Math.Cos(angle)), yA1 = (float)(y2 + len * Math.Sin(angle));
                angle += offset * 2f;
                float xA2 = (float)(x2 + len * Math.Cos(angle)), yA2 = (float)(y2 + len * Math.Sin(angle));

                //draw
                float s = (float)(2 + Math.Pow(size / Game.avgSize, 1.75f));
                using (Pen pen = new Pen(color, s))
                {
                    graphics.DrawLine(pen, x1, y1, x2, y2);
                    graphics.DrawLine(pen, xA1, yA1, x2, y2);
                    graphics.DrawLine(pen, xA2, yA2, x2, y2);
                }
            }
        }
        private float getCenter(RectangleF rect, bool x)
        {
            return (x ? rect.X + rect.Width / 2f : rect.Y + rect.Height / 2f);
        }
        protected RectangleF GetPieceRectangle(Rectangle drawRectangle)
        {
            float xScale, yScale;
            return GetPieceRectangle(drawRectangle, out xScale, out yScale);
        }
        protected RectangleF GetPieceRectangle(Rectangle drawRectangle, out float xScale, out float yScale)
        {
            xScale = drawRectangle.Width / Game.gameSize;
            yScale = drawRectangle.Height / Game.gameSize;
            float xDraw = getCenter(drawRectangle, true) + (x - size / 2f) * xScale;
            float yDraw = getCenter(drawRectangle, false) + (y - size / 2f) * yScale;
            return new RectangleF(xDraw, yDraw, xScale * size, yScale * size);
        }

        protected Brush getBrush(Color color)
        {
            Brush brush;
            if (!brushes.TryGetValue(color, out brush))
                brushes.Add(color, brush = new SolidBrush(color));
            return brush;
        }
    }
}
