using System;
using System.Collections.Generic;
using System.Text;

namespace game1
{
    class Dirt : Terrain
    {
        public Dirt(int X, int Y)
            : this(X, Y, false)
        {
        }
        public Dirt(int X, int Y, bool visible)
            : base(ConsoleColor.White, X, Y, visible)
        {
        }
    }
}