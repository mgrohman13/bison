using System;
using System.Drawing;
using Point = MattUtil.Point;

namespace SpaceRunner.Images
{
    internal static class FuelExplosionGenerator
    {
        internal static Bitmap GenerateFuelExplosion(int size)
        {
            Bitmap image = Generator.CreateInitialImage(size);

            float center = ( size - 1f ) / 2f;
            for (int x = 0 ; x < size ; ++x)
                for (int y = 0 ; y < size ; ++y)
                {
                    float dist = Game.GetDistanceSqr(x - center, y - center) / ( size * size / 4f );
                    if (dist < 1 && Game.Random.Bool((float)Math.Pow(1.0 - dist, .65)))
                    {
                        float average = (float)( Math.Pow(dist, .39) * 169.0 );
                        int green = Game.Random.GaussianCappedInt(average, 1f, Math.Max(0, (int)Math.Ceiling(2f * average - 255f)));

                        float brightness = .39f + ( 1f - .39f ) * ( 1f - dist );
                        brightness = Game.Random.GaussianCapped(brightness, 1f, Math.Max(0f, 2f * brightness - 1f));

                        int red = Game.Random.Round(( green + Game.Random.Weighted(255f - green, .52f + .39f * ( 1f - dist )) ) * brightness);
                        green = Game.Random.Round(green * brightness);

                        image.SetPixel(x, y, Color.FromArgb(red, green, 0));
                    }
                }

            return image;
        }
    }
}
