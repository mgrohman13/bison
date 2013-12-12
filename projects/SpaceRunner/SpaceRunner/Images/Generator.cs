using System;
using System.Drawing;

namespace SpaceRunner.Images
{
    internal static class Generator
    {
        internal static void Generate()
        {
            Bullet.InitImages();
            Explosion.InitImages();
            FuelExplosion.InitImages();
            LifeDust.InitImages();
        }

        internal static Bitmap CreateInitialImage(int size)
        {
            return CreateInitialImage(new Bitmap(size, size));
        }

        internal static Bitmap CreateInitialImage(Bitmap image)
        {
            Graphics graphics;
            image = CreateInitialImage(image, out graphics);
            graphics.Dispose();
            return image;
        }

        internal static Bitmap CreateInitialImage(int size, out Graphics graphics)
        {
            return CreateInitialImage(new Bitmap(size, size), out graphics);
        }

        internal static Bitmap CreateInitialImage(Bitmap image, out Graphics graphics)
        {
            graphics = Graphics.FromImage(image);
            graphics.Clear(Color.Magenta);
            return image;
        }

        internal static Bitmap CreateInitialImage(int size, Color dark, Color light, int innerSize, out Graphics graphics)
        {
            Bitmap retVal = CreateInitialImage(size, out graphics);

            graphics.FillEllipse(new SolidBrush(dark), 0, 0, size - 1, size - 1);
            graphics.FillEllipse(new SolidBrush(light), innerSize, innerSize, size - 1 - innerSize * 2, size - 1 - innerSize * 2);

#if DEBUG
            if (size != 13)
                throw new Exception();
#endif
            //this code only supports a size of 13...
            retVal.SetPixel(0, 5, dark);
            retVal.SetPixel(0, 6, dark);
            retVal.SetPixel(0, 7, dark);
            retVal.SetPixel(12, 5, dark);
            retVal.SetPixel(12, 6, dark);
            retVal.SetPixel(12, 7, dark);
            retVal.SetPixel(5, 0, dark);
            retVal.SetPixel(6, 0, dark);
            retVal.SetPixel(7, 0, dark);
            retVal.SetPixel(5, 12, dark);
            retVal.SetPixel(6, 12, dark);
            retVal.SetPixel(7, 12, dark);

            if (innerSize == 3)
                retVal.SetPixel(3, 6, dark);

            return retVal;
        }

        internal static void DrawBlobs(Graphics graphics, Brush light, Brush dot, int num, int min, float minSize, float maxSize, float dotSize, float dotPosition)
        {
            int numBlobs = Game.Random.GaussianOEInt(num, .13f, .13f, min);
            for (int idx = 0 ; idx < numBlobs ; ++idx)
            {
                float size = Game.Random.Range(minSize, maxSize);
                PointF pf = GetPoint(Game.Random.DoubleHalf(6.5f - size));
                graphics.FillRectangle(light, GetRectangle(pf, size));
            }
            PointF p2 = GetPoint(Game.Random.Weighted(6.5f - dotSize / Game.SqrtTwo, dotPosition));
            graphics.FillEllipse(dot, GetRectangle(p2, dotSize));
        }
        private static PointF GetPoint(float dist)
        {
            return Game.GetPoint(Game.GetImageAngle(), dist);
        }
        private static RectangleF GetRectangle(PointF p, float size)
        {
            return new RectangleF(6f + p.X - size / 2f, 6f + p.Y - size / 2f, size, size);
        }
    }
}
