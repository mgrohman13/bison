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

        protected float x, y, xDir, yDir, size, density;
        private Color color;

        protected Piece(float x, float y, float size, float density, Color color)
        {
            this.x = x;
            this.y = y;
            this.xDir = 0;
            this.yDir = 0;
            this.size = size;
            this.density = density;
            this.color = color;
        }

        public float Mass
        {
            get
            {
                return size * density;
            }
        }
        internal abstract float GetGravity(Type type);

        internal virtual void Interact(Piece piece)
        {
            Gravity(this, piece);
        }

        protected static void Gravity(Piece p1, Piece p2)
        {
            float xDist = p1.x - p2.x;
            float yDist = p1.y - p2.y;
            float distSqr = xDist * xDist + yDist * yDist;
            float distance = (float)Math.Sqrt(distSqr);
            if (distance > ( p1.size + p2.size ) / 2f)
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

        internal virtual void Step()
        {
            x += xDir;
            y += yDir;

            float dist = Game.gameSize / 2f;
            dist *= dist;
            if (x * x + y * y > dist)
            {
                xDir -= x * Game.offMapPull;
                yDir -= y * Game.offMapPull;
            }
        }

        public virtual void Draw(Graphics graphics, Rectangle drawRectangle, float gameWidth, float gameHeight)
        {
            float xScale = drawRectangle.Width / gameWidth;
            float yScale = drawRectangle.Height / gameHeight;

            float xDraw = ( x - size / 2f ) * xScale + drawRectangle.Width / 2f;
            float yDraw = ( y - size / 2f ) * yScale + drawRectangle.Height / 2f;

            graphics.FillEllipse(getBrush(color), xDraw, yDraw, size * xScale, size * yScale);
        }

        private Brush getBrush(Color color)
        {
            Brush brush;
            if (!brushes.TryGetValue(color, out brush))
                brushes.Add(color, brush = new SolidBrush(color));
            return brush;
        }
    }
}
