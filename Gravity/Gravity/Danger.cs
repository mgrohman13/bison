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
            this.mult = Game.rand.GaussianOE(2.5f, .35f, .15f);
            Console.WriteLine("new Danger - " + mult);
        }

        internal override float GetGravity(Type type)
        {
            float mult = this.mult * game.DiffMult(.5f, .1f);
            if (type == typeof(Enemy))
                return 1 / mult;
            if (type == typeof(Player))
                return mult;
            return (float)(Math.Sqrt(mult) * base.GetGravity(type));
        }
    }
}
