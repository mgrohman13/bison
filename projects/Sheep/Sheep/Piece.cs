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
        protected float x, y;

        protected Piece(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public virtual void Draw(Graphics graphics, float yOffset)
        {
            using (Font f = new Font("Arial", 13f))
            {
                string str = this.GetType().ToString().Substring(6);
                SizeF size = graphics.MeasureString(str, f);
                graphics.DrawString(str, f, Brushes.Black, x - size.Width / 2f, y + yOffset - size.Height / 2f);
            }
        }


        public virtual void Step()
        {
        }
    }
}
