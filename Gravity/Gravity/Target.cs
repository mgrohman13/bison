using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gravity
{
    class Target : Piece
    {
        public Target(Game game, float x, float y, float size, float density) : base(game, x, y, size, density, System.Drawing.Color.Fuchsia)
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

        internal override void Step(float count)
        {
            float diff = game.DiffMult(.5f, .6f);
            this.size = (float)(15 / Math.Sqrt(diff));
            this.density = 1 / diff;
        }
    }
}
