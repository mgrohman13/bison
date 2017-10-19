using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Sheep
{
    class Piece
    {
        protected Game game;
        protected float x, y;
        protected Image image;

        protected Piece(Game game, float x, float y, Image image)
        {
            this.game = game;
            this.x = x;
            this.y = y;
            this.image = image;
        }
        public float X { get { return x; } }
        public float Y { get { return y; } }

        public virtual void Draw(Graphics graphics, float yOffset)
        {
            //using (Font f = new Font("Arial", 13f))
            //{
            //string str = this.GetType().ToString().Substring(6);
            //SizeF size = graphics.MeasureString(str, f);
            //graphics.DrawString(str, f, Brushes.Black, x - size.Width / 2f, y + yOffset - size.Height / 2f);
            //}

            graphics.DrawImage(image, x - image.Width / 2f, y + yOffset - image.Height / 2f);
        }


        public virtual void Step()
        {
        }
    }
}
