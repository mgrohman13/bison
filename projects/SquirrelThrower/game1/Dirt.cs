using System;
using System.Collections.Generic;
using System.Text;

namespace game1
{
    class Dirt : Terrain
    {
        public Dirt(int X, int Y)
            : base(ConsoleColor.White, X, Y) { }
    }
}