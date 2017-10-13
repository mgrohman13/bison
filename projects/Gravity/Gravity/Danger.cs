using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gravity
{
    class Danger : Enemy
    {
        public Danger(Game game, float x, float y, float size, float density) : base(game, x, y, size, density)
        {
        }

        internal override float GetGravity(Type type)
        {
            float gravity = base.GetGravity(type);
            if (type == typeof(Enemy))
                gravity /= 2f;
            else if (type == typeof(Player))
                gravity *= 4f * game.Difficulty;
            else
                gravity *= 2f;
            return gravity;
        }
    }
}
