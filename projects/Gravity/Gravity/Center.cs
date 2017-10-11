using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gravity
{
    class Center : Piece
    {
        public Center(float x, float y, float size, float density) : base(x, y, size, density, System.Drawing.Color.Red)
        {
        }

        internal override float GetGravity(Type type)
        {
            return 0;
        }

        internal override void Step()
        {
        }
    }
}
