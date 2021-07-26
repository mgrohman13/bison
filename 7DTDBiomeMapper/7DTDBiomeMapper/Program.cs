using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace _7DTDBiomeMapper
{
    class Program
    {
        static void Main(string[] args)
        {
            ImageAttributes colorRemapping = new ImageAttributes();
            ColorMap forestToWasteland = new ColorMap();
            forestToWasteland.OldColor = Color.FromArgb(0, 64, 0);
            forestToWasteland.NewColor = Color.FromArgb(255, 168, 0);
            colorRemapping.SetRemapTable(new ColorMap[] { forestToWasteland });

            string path = "C:\\files\\KingGen\\";// Console.ReadLine() + System.IO.Path.DirectorySeparatorChar;
            string fileName = "biomes.png";// Console.ReadLine();
            Image basePic = Image.FromFile(path + fileName);

            Bitmap pic = new Bitmap(basePic.Width, basePic.Height);
            Graphics g = Graphics.FromImage(pic);
            g.DrawImage(basePic, new Rectangle(0, 0, basePic.Width, basePic.Height), 0, 0, basePic.Width, basePic.Height, GraphicsUnit.Pixel, colorRemapping);
            pic.Save(path + "out.png", ImageFormat.Png);
        }
    }
}
