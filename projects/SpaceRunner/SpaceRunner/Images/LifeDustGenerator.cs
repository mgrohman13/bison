using System;
using System.Drawing;

namespace SpaceRunner.Images
{
    class LifeDustGenerator
    {
        internal static Bitmap GenerateLifeDust()
        {
            int darkVal = Game.Random.WeightedInt(2, .65f);
            Color dark = GetDark(darkVal), light = GetLight(darkVal);

            Graphics graphics;
            Bitmap retVal = Generator.CreateInitialImage(13, dark, light, 3, out graphics);

            Generator.DrawBlobs(graphics, new SolidBrush(light), new SolidBrush(GetLight()), 39, 21, 1.5f, 2.5f, 3f);

            int corner = Game.Random.Next(4);
            Rectangle r;
            switch (corner)
            {
            case 0:
                r = new Rectangle(-1, 5, 6, -1);
                break;
            case 1:
                r = new Rectangle(-2, 5, 6, 14);
                break;
            case 2:
                r = new Rectangle(15, 5, 6, 14);
                break;
            case 3:
                r = new Rectangle(15, 7, 6, -1);
                break;
            default:
                throw new Exception();
            }
            graphics.DrawLine(new Pen(Color.Magenta, 3), r.X, r.Y, r.Width, r.Height);
            if (corner == 0)
                retVal.SetPixel(6, 1, dark);

            Generator.DisposeGraphics(graphics);
            return retVal;
        }

        private static Color GetDark(int darkColor)
        {
            switch (darkColor * 3 + Game.Random.Next(3))
            {
            //yellow
            case 0:
                return Color.DarkGoldenrod;
            case 1:
                return Color.SaddleBrown;
            case 2:
                return Color.Sienna;

            //green
            case 3:
                return Color.DarkGreen;
            case 4:
                return Color.DarkOliveGreen;
            case 5:
                return Color.DarkSlateBlue;

            //blue
            case 6:
                return Color.DarkBlue;
            case 7:
                return Color.MediumBlue;
            case 8:
                return Color.Navy;
            }

            throw new Exception();
        }

        private static Color GetLight()
        {
            return GetLight(Game.Random.Next(3));
        }
        private static Color GetLight(int darkColor)
        {
            if (Game.Random.Bool(.13))
                return GetLight();

            int low, high;
            switch (darkColor)
            {
            //yellow: bluegreen,green,blue
            case 0:
                low = 0;
                high = 23;
                break;
            //green: blue,yellow
            case 1:
                low = 18;
                high = 35;
                break;
            //blue: green
            case 2:
                low = 12;
                high = 17;
                break;
            default:
                throw new Exception();
            }

            switch (Game.Random.RangeInt(low, high))
            {
            //bluegreen
            case 0:
            case 1:
            case 2:
            case 3:
                return Color.Aquamarine;
            case 4:
            case 5:
            case 6:
            case 7:
                return Color.DarkTurquoise;
            case 8:
            case 9:
            case 10:
            case 11:
                return Color.Turquoise;

            //green
            case 12:
                return Color.LawnGreen;
            case 13:
                return Color.LightGreen;
            case 14:
                return Color.LimeGreen;
            case 15:
                return Color.MediumSpringGreen;
            case 16:
                return Color.PaleGreen;
            case 17:
                return Color.SpringGreen;

            //blue
            case 18:
                return Color.Aqua;
            case 19:
                return Color.Cyan;
            case 20:
                return Color.DeepSkyBlue;
            case 21:
                return Color.DodgerBlue;
            case 22:
                return Color.LightSkyBlue;
            case 23:
                return Color.SkyBlue;

            //yellow
            case 24:
            case 25:
                return Color.Chartreuse;
            case 26:
            case 27:
                return Color.Gold;
            case 28:
            case 29:
                return Color.GreenYellow;
            case 30:
            case 31:
                return Color.Khaki;
            case 32:
            case 33:
                return Color.Lime;
            case 34:
            case 35:
                return Color.Yellow;
            }

            throw new Exception();
        }
    }
}
