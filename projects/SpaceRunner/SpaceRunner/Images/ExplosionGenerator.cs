using System;
using System.Drawing;
using Point = MattUtil.Point;

namespace SpaceRunner.Images
{
    internal static class ExplosionGenerator
    {
        internal static Bitmap[] GenerateExplosion(int width, int length, float size)
        {
            Bitmap[] result = new Bitmap[length * 2 - 1];

            Bitmap last = new Bitmap(width, width);
            Generator.CreateInitialImage(last);

            float center = ( width - 1f ) / 2f;

            int idx1 = 0;
            while (++idx1 <= length)
            {
                Bitmap exp = new Bitmap(last);
                last = exp;

                int x = Game.Random.Round(center),
                    y = Game.Random.Round(center);

                byte[] temp = new byte[1];
                Game.Random.NextBytes(temp);
                byte color = temp[0];

                int amt = Game.Random.GaussianCappedInt(size, .13f);
                for (int idx2 = 0 ; idx2 < amt ; ++idx2)
                {
                    int oldX = x, oldY = y;
                    x = Mod(x, width, center);
                    y = Mod(y, width, center);

                    if (Game.GetDistanceSqr(x - center, y - center) > width * width / 4f)
                    {
                        --idx2;
                        x = oldX;
                        y = oldY;
                    }
                    else
                    {
                        color = (byte)( color + Game.Random.GaussianInt(39) );
                        if (Game.Random.Bool() || exp.GetPixel(x, y).ToArgb() == Color.Magenta.ToArgb())
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

                int amt = Game.Random.GaussianCappedInt(width * width / Math.Pow(length, .65), .13);
                foreach (Point p in Game.Random.Iterate(width, width))
                {
                    if (--amt < 0)
                        break;

                    exp.SetPixel(p.X, p.Y, Color.Magenta);
                }

                result[idx1] = exp;
            }

            return result;
        }

        private static int Mod(int v, int width, float center)
        {
            return Game.Random.Round(( 3f * v + center ) / 4f + Game.Random.Gaussian(width * .091f));
        }
    }
}
