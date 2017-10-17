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
            return dir * ( 1 - Game.offMapPull ) + trg * Game.offMapPull;
        }

        public virtual void Draw(Graphics graphics, Rectangle drawRectangle, float gameWidth, float gameHeight)
        {
            float xScale = drawRectangle.Width / gameWidth;
            float yScale = drawRectangle.Height / gameHeight;
            float xDraw = drawRectangle.X + ( x - size / 2f ) * xScale + drawRectangle.Width / 2f;
            float yDraw = drawRectangle.Y + ( y - size / 2f ) * yScale + drawRectangle.Height / 2f;
            xScale *= size;
            yScale *= size;

            RectangleF drawPiece = new RectangleF(xDraw, yDraw, xScale, yScale);
            if (drawPiece.IntersectsWith(drawRectangle))
            {
                graphics.FillEllipse(getBrush(color), drawPiece);
            }
            else
            {
                float x1 = xDraw + xScale / 2f, y1 = yDraw + yScale / 2f, x2 = drawRectangle.X + drawRectangle.Width / 2f, y2 = drawRectangle.Y + drawRectangle.Height / 2f;

                float dist = (float)Math.Sqrt(x * x + y * y);
                float l = (float)Math.Pow(dist / ( Game.gameSize / 2f ), 1f);
                float length = ( 1f - l / ( l + 15f ) ) / 2f;
                float t = (float)Math.Sqrt(( x1 - x2 ) * ( x1 - x2 ) + ( y1 - y2 ) * ( y1 - y2 ));
                float x3 = x2 + drawRectangle.Width * length * ( x1 - x2 ) / t;
                float y3 = y2 + drawRectangle.Height * length * ( y1 - y2 ) / t;
                x1 = ( x1 - x2 ) / t * ( Game.gameSize / 2f ) * xScale;
                y1 = ( y1 - y2 ) / t * ( Game.gameSize / 2f ) * yScale;
                x2 = x3;
                y2 = y3;

                float size = (float)( 2 + Math.Pow(this.size / Game.avgSize, 1.5f) );
                using (Pen pen = new Pen(color, size))
                    graphics.DrawLine(pen, x1, y1, x2, y2);
            }
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
