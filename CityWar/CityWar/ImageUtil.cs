using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace CityWar
{
    internal class ImageUtil
    {
        public static float Zoom = -1;

        public static Bitmap GetPic(Player player, string name, Dictionary<string, Bitmap> pics, Dictionary<string, Bitmap> picsConst, bool constPic)
        {
            Dictionary<string, Bitmap> d = constPic ? picsConst : pics;
            if (!d.ContainsKey(name))
                CreatePic(player, name, pics, picsConst, constPic);
            return d[name];
        }
        private static void CreatePic(Player player, string name, Dictionary<string, Bitmap> pics, Dictionary<string, Bitmap> picsConst, bool constPic)
        {
            string loadName = name.Replace(" (1)", "1").Replace(" (2)", "2").Replace(" (3)", "3");

            Color portalColor = Color.FromArgb(200, 0, 0);
            if (name.EndsWith(" Portal") || name.EndsWith(" PortalUnit"))
            {
                string[] split = name.Split(' ');
                switch (split[0])
                {
                    case "Air":
                        portalColor = Color.Gray;
                        break;
                    case "Death":
                        portalColor = Color.Black;
                        break;
                    case "Earth":
                        portalColor = Color.Gold;
                        break;
                    case "Nature":
                        portalColor = Color.Green;
                        break;
                    case "Water":
                        portalColor = Color.Blue;
                        break;
                }
                loadName = split[1];
            }

            Bitmap pic;
            //dont bother saving a const pic if the name ends with unit; it will never be drawn in a panel
            bool file = (constPic || name.EndsWith("Unit"));
            if (file)
                pic = LoadPicFromFile(player, loadName, portalColor);
            else
                pic = picsConst[name];

            if (constPic)
                picsConst.Add(name, pic);
            else
                pics.Add(name, ResizePic(pic, file));
        }
        private static Bitmap LoadPicFromFile(Player player, string name, Color portalColor)
        {
            Bitmap basePic = Load(name);

            //change the gray to the player color and the red to the poral color
            ImageAttributes colorRemapping = new();
            ColorMap playerMap = new();
            playerMap.OldColor = Color.FromArgb(100, 100, 100);
            playerMap.NewColor = player.Color;
            ColorMap portalMap = new();
            portalMap.OldColor = Color.FromArgb(200, 0, 0);
            portalMap.NewColor = portalColor;
            colorRemapping.SetRemapTable(new ColorMap[] { playerMap, portalMap });

            Bitmap pic = new(100, 100);
            Graphics g = Graphics.FromImage(pic);
            //draw it to a new image to remap the colors
            g.DrawImage(basePic, new Rectangle(0, 0, 100, 100), 0, 0, 100, 100, GraphicsUnit.Pixel, colorRemapping);

            g.Dispose();
            basePic.Dispose();
            colorRemapping.Dispose();

            //return the new image
            return pic;
        }

        private static Bitmap ResizePic(Bitmap pic, bool dispose, float size = 1f)
        {
            Bitmap newPic = new(pic, Game.Random.Round(Zoom * size * 5f / 6f), Game.Random.Round(Zoom * size * 5f / 6f));
            if (dispose)
                pic.Dispose();
            return newPic;
        }

        public static Bitmap LoadPicFromFile(Treasure.TreasureType type)
        {
            return ResizePic(Load("Treasure " + type.ToString()), true, .39f);
        }

        private static Bitmap Load(string name)
        {
            Bitmap basePic;
            try
            {
                basePic = new Bitmap(Game.ResourcePath + "pics\\" + name + ".bmp");
            }
            catch
            {
                basePic = new Bitmap(Game.ResourcePath + "pics\\notFound.bmp");
            }
            //white is transparent
            basePic.MakeTransparent(Color.FromArgb(255, 255, 255));
            return basePic;
        }
    }
}
