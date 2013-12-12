using System;
using System.Drawing;

namespace SpaceRunner.Images
{
    internal static class BulletGenerator
    {
        internal static Bitmap GenerateBullet()
        {
            Graphics graphics;
            Bitmap retVal = Generator.CreateInitialImage(13, Color.Teal, Color.Aqua, 3, out graphics);

            Generator.DrawBlobs(graphics, Brushes.Aqua, Brushes.White, 13, 6, 1.5f, 4.5f, Game.Random.GaussianCapped(3.9f, .065f), .6f);

            graphics.Dispose();
            return retVal;
        }
    }
}
