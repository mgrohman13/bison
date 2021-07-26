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
            float maxDistSqr = ( size * size / 4f );
            for (int x = 0 ; x < size ; ++x)
                for (int y = 0 ; y < size ; ++y)
                {
                    float distSqr = Game.GetDistanceSqr(x, y, center, center) / maxDistSqr;
                    if (distSqr < 1 && Game.Random.Bool((float)Math.Pow(1.0 - distSqr, .78)))
                    {
                        float avgGreen = (float)( Math.Pow(distSqr, .39) * 169.0 );
                        int green = Game.Random.GaussianCappedInt(avgGreen, 1f, Math.Max(0, (int)Math.Ceiling(2.0 * avgGreen - 255.0)));
                        //int green = Game.Random.WeightedInt(255, avgGreen / 255f);

                        float brightness = Game.Random.GaussianCapped(.39f, distSqr);
                        brightness = brightness + ( 1f - brightness ) * ( 1f - distSqr );
                        brightness = Game.Random.GaussianCapped(brightness, distSqr, Math.Max(0f, 2f * brightness - 1f));

                        int red = Game.Random.Round(( green + Game.Random.Weighted(255f - green, .52f + .39f * ( 1f - distSqr )) ) * brightness);
                        green = Game.Random.Round(green * brightness);

                        float darkCutoff = ( red + green ) / 65f;
                        if (darkCutoff >= 1 || Game.Random.Round(darkCutoff) > 0)
                            image.SetPixel(x, y, Color.FromArgb(red, green, 0));
                    }
                }

            return image;
        }
    }
}
