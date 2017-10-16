using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gravity
{
    class Danger : Enemy
    {
        private float mult;

        public Danger(Game game, float x, float y, float size, float density) : base(game, x, y, size, density)
        {
            Console.WriteLine("new Danger");
            this.mult = Game.rand.GaussianOE(2.5f, .35f, .15f);
        }

        internal override float GetGravity(Type type)
        {
            float mult = this.mult * game.Difficulty;
            float gravity = base.GetGravity(type);
            if (type == typeof(Enemy))
                gravity /= (float)Math.Sqrt(mult);
            else if (type == typeof(Player))
                gravity *= mult;
            else
                gravity *= (float)Math.Sqrt(mult);
            return gravity;
        }
    }
}
