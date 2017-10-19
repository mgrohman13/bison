using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Sheep
{
    class Player : Piece
    {
        private static Image playerImage = LoadImage();
        private static Image LoadImage()
        {
            return Game.LoadImage("VX-XP-151-Animal01_zps71801271.bmp", 0, 40, 39, 39);
        }

        public Player(Game game, float x, float y) : base(game, x, y, playerImage)
        {
        }

        public override void Step()
        {
            float dist =(float) Math.Sqrt((this.X - game.Target.X) * (this.X - game.Target.X) + (this.Y - game.Target.Y) * (this.Y - game.Target.Y));
            float speed = Math.Min(dist, 1.3f);
            if (speed > 0)
            {
                this.x -= (this.X - game.Target.X) * speed / dist;
                this.y -= (this.Y - game.Target.Y) * speed / dist;
            }
        }
    }
}
