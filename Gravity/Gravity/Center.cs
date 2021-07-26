using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gravity
{
    class Center : Piece
    {
        public Center(Game game, float x, float y, float size, float density) : base(game, x, y, size, density, System.Drawing.Color.Purple)
        {
        }

        internal override float GetGravity(Type type)
        {
            return 0;
        }

        internal override void Step(float count)
        {
        }
    }
}
