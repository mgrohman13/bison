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
            float diff = game.DifficultyMeter;
            if (diff < 0)
                diff = 1f / ( 1f - diff );
            else
                diff++;
            this.size = (float)( 15 / Math.Pow(diff, .1) );
            this.density = (float)( 1f / Math.Pow(game.Difficulty, .25) / Math.Pow(diff, .1) );
        }

    }
}
