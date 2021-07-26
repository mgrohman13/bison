using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Threading;
using MattUtil;
using Point = MattUtil.Point;

namespace ExplosionGenerator
{
    public class Program
    {
        private static MTRandom rand;

        public static void Main(string[] args)
        {
            rand = new MTRandom();
            rand.StartTick();

            foreach (int dn in rand.Iterate(6))
            {
                string dir = @"C:\Development\other\projects\SpaceRunner\pics\explosion\" + ( dn + 1 ) + "\\";
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
                Thread.Sleep(1000);
                Directory.CreateDirectory(dir);
                int i = 0;
                foreach (Bitmap b in GenerateExplosion(39, 13, 16.9))
                    b.Save(dir + ++i + ".bmp");
            }
        }

        private static Bitmap[] GenerateExplosion(int width, int length, double size)
        {
            Bitmap[] result = new Bitmap[length * 2 - 1];

            Bitmap last = new Bitmap(width, width);
            Graphics g = Graphics.FromImage(last);
            g.Clear(Color.Magenta);
            g.Flush();
            g.Dispose();

            double center = ( width - 1.0 ) / 2;

            int idx1 = 0;
            while (++idx1 <= length)
            {
                Bitmap exp = new Bitmap(last);
                last = exp;

                int x = rand.Round(center),
                    y = rand.Round(center);

                byte[] temp = new byte[1];
                rand.NextBytes(temp);
                byte color = temp[0];

                int amt = rand.GaussianCappedInt(size, .13);
                for (int idx2 = 0 ; idx2 < amt ; ++idx2)
                {
                    int oldX = x, oldY = y;
                    x = Mod(x, width, center);
                    y = Mod(y, width, center);

                    if (( x - center ) * ( x - center ) + ( y - center ) * ( y - center ) > width * width / 4.0)
                    {
                        --idx2;
                        x = oldX;
                        y = oldY;
                    }
                    else
                    {
                        color = (byte)( color + rand.GaussianInt(39) );
                        if (rand.Bool() || exp.GetPixel(x, y).ToArgb() == Color.Magenta.ToArgb())
                            exp.SetPixel(x, y, Color.FromArgb(255, color, 0));
                    }
                }

                result[idx1 - 1] = exp;
            }

            idx1 -= 2;
            while (++idx1 < result.Length)
            {
                Bitmap exp = new Bitmap(last);
                last = exp;

                int amt = rand.GaussianCappedInt(width * width / Math.Pow(length, .65), .13);
                foreach (Point p in rand.Iterate(width, width))
                {
                    if (--amt < 0)
                        break;

                    exp.SetPixel(p.X, p.Y, Color.Magenta);
                }

                result[idx1] = exp;
            }

            return result;
        }

        private static int Mod(int v, int width, double center)
        {
            return rand.Round(( 3 * v + center ) / 4 + rand.Gaussian(width * .091));
        }
    }
}
