using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gravity
{
    class Enemy : Piece
    {
        public Enemy(float x, float y, float size, float density) : base(x, y, size, density, System.Drawing.Color.Black)
        {
        }

        internal override float GetGravity(Type type)
        {
            if (type == typeof(Center))
                return 2;
            if (type == typeof(Enemy))
                return 1;
            if (type == typeof(Player))
                return 1 / 2f;
            if (type == typeof(Target))
                return 1 / 4f;
            throw new Exception();
        }
    }
}
