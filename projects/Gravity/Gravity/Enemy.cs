using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gravity
{
    class Enemy : Piece
    {
        public Enemy(float x, float y, float size, float density) : base(x, y, size, density, getColor(density))
        {
        }

        private static Color getColor(float density)
        {
            int scale = Game.rand.Round(255f * Math.Pow(1 / (1 + density * density), 2.5));
            return Color.FromArgb(scale, scale, scale);
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
