using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Sheep
{
    class Sheep : Piece
    {
        private float trgX, trgY;

        private static Image sheepImage = LoadImage();
        private static Image LoadImage()
        {
            return Game.LoadImage("sheep_eat.bmp", 36, 172, 51, 51);
        }

        public Sheep(Game game, float x, float y) : base(game, x, y, sheepImage)
        {
            this.trgX = x;
            this.trgY = y;
        }

        public void Step(float sheepX, float sheepY)
        {
            base.Step();

            float dist = (float)Math.Sqrt((this.X - trgX) * (this.X - trgX) + (this.Y - trgY) * (this.Y - trgY));
            float speed = Math.Min(dist, .39f);
            if (speed > 0)
            {
                this.x -= (this.X - trgX) * speed / dist;
                this.y -= (this.Y - trgY) * speed / dist;
            }

            if (Game.rand.Bool(.0026))
            {
                float avgX = (3 * trgX + sheepX) / 4f;
                trgX = avgX + Game.rand.Gaussian(130f / (avgX - trgX));

                float avgY = (3 * trgY + sheepY) / 4f;
                trgY = avgY + Game.rand.Gaussian(130f / (avgY - trgY));
            }

            float pDist = (float)((this.X - game.Player.X) * (this.X - game.Player.X) + (this.Y - game.Player.Y) * (this.Y - game.Player.Y));
            if (Game.rand.Bool(169f / (169f + pDist)))
            {
                trgX += 650f / (this.X - game.Player.X);
                trgY += 650f / (this.Y - game.Player.Y);
            }
        }
    }
}
