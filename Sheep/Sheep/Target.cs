using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Sheep
{
    class Target : Piece
    {
        private static Image targetImage = LoadImage();
        private static Image LoadImage()
        {
            return Game.LoadImage("target.bmp");
        }

        public Target(Game game, float x, float y) : base(game, x, y, targetImage)
        {
        }

        public void SetTarget(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public override void Draw(Graphics graphics, float yOffset)
        {
            if (this.x != game.Player.X || this.y != game.Player.Y)
                base.Draw(graphics, yOffset);
        }
    }
}
