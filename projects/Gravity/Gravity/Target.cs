using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gravity
{
    class Target : Piece
    {
        public Target(Game game, float x, float y, float size, float density) : base(game, x, y, size, density, System.Drawing.Color.Red)
        {
        }

        internal override float GetGravity(Type type)
        {
            return 0;
        }

        internal void setTarget(float x, float y)
        {
            this.x = x;
            this.y = y - size / 2f;
        }

        internal override void Step(float count)
        {
            this.density = (float)(1.1f / Math.Pow(game.Difficulty, .125f));
        }
    }
}
