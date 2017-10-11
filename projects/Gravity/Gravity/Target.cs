using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gravity
{
    class Target : Piece
    {
        public Target(float x, float y, float size, float density) : base(x, y, size, density, System.Drawing.Color.Teal)
        {
        }

        internal override float GetGravity(Type type)
        {
            return 0;
        }

        internal void setTarget(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        internal override void Step()
        {
        }
    }
}
