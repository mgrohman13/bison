using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheep
{
    class Target : Piece
    {
        public Target(float x, float y) : base(x, y)
        {
        }

        public void SetTarget(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
