using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Sheep
{
    class Wolf : Piece
    {
        private static Image wolfImage = LoadImage();
        private static Image LoadImage()
        {
            return Game.LoadImage("LOS-Wolf02.bmp", 0, 40, 39, 39);
        }

        public Wolf(Game game, float x, float y) : base(game, x, y, wolfImage)
        {
        }
    }
}
