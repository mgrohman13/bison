using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gravity
{
    class PowerUp : Piece
    {
        public readonly bool Type;
        private float gravity;

        public PowerUp(Game game, bool type, float x, float y, float size, float density) : base(game, x, y, size, density, type ? Color.Purple : Color.Green)
        {
            this.Type = type;
            this.gravity = Game.rand.Weighted(1 / 8f);
        }

        internal override void Step(float count)
        {
            base.Step(count);

            this.size -= Game.rand.OE(size * .00025f + .01f);
            if (size <= 0)
                game.Remove(this);
        }

        internal override float GetGravity(Type type)
        {
            if (type == typeof(Center))
                return 0;
            return gravity;
        }
    }
}
