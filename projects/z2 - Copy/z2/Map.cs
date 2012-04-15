using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MattUtil;

namespace z2
{
    public class Map
    {
        private const int NumLevels = 13;
        private const double FreqMult = 1.69, AmpMult = 1.3, Spread = 1;
        private float max;

        private uint[] seeds;
        private float[] rotate;

        private Dictionary<Point, float> final;

        public Map()
        {
            this.seeds = new uint[NumLevels];
            this.rotate = new float[NumLevels];
            this.final = new Dictionary<Point, float>();

            double amp = 1, max = amp;
            for (int level = 0 ; level < NumLevels ; ++level)
            {
                this.seeds[level] = Game.Random.NextUInt();
                this.rotate[level] = Game.Random.DoubleFull((float)Math.PI);
                max += ( amp *= AmpMult );
            }

            this.max = (float)( 2 * max );
        }

        public void Explore(Point p1)
        {
            if (!this.final.ContainsKey(p1))
                this.final[p1] = GetHeight(p1);
        }

        public void DrawAll(Point topLeft, int width, int height)
        {
            for (int y = topLeft.Y ; y < topLeft.Y + height ; ++y)
                for (int x = topLeft.X ; x < topLeft.X + width ; ++x)
                {
                    float h;
                    if (this.final.TryGetValue(new Point(x, y), out h))
                    {
                        h = 1 + h * 15;
                        Console.BackgroundColor = (ConsoleColor)( h );
                    }
                    else
                    {
                        Console.BackgroundColor = (ConsoleColor)( 0 );
                    }
                    Console.Write(' ');
                }
            Console.SetCursorPosition(0, 0);
        }

        private float GetHeight(Point p)
        {
            double amp = 1;
            double freq = 1;
            double height = Game.Random.DoubleHalf(2 * amp);
            for (int level = 0 ; level < NumLevels ; ++level)
            {
                amp *= AmpMult;
                freq *= FreqMult;
                double length = p.X * p.X + p.Y * p.Y;
                double y = Math.Sqrt(length) * Math.Tanh(Math.Atan2(p.Y, p.X) + this.rotate[level]);
                double x = Math.Sqrt(length - y * y);
                height += Noise.GetNoise(this.seeds[level], x / freq, amp);
                height += Noise.GetNoise(this.seeds[level], y / freq, amp);
            }
            height /= this.max;
            if (height > .5)
                height = Math.Pow(height, 1 / Spread);
            else if (height < .5)
                height = Math.Pow(height, Spread);
            return (float)height;
        }
    }
}
