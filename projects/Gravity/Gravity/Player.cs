using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gravity
{
    class Player : Piece
    {
        public Player(float x, float y, float size, float density) : base(x, y, size, density, System.Drawing.Color.Blue)
        {
        }

        internal override float GetGravity(Type type)
        {
            if (type == typeof(Center))
                return 1;
            if (type == typeof(Enemy))
                return 1 / 4f;
            if (type == typeof(Player))
                return 1;
            if (type == typeof(Target))
                return 8;
            throw new Exception();
        }

        internal override void Step()
        {
            base.Step();

            const float decay = .999f;
            this.xDir *= decay;
            this.yDir *= decay;
        }
    }
}
