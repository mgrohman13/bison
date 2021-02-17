using System;
using System.Collections.Generic;
using System.Text;

namespace game1
{
    class Grass : Terrain
    {
        public Grass(int X, int Y, bool visible)
            : base(ConsoleColor.Gray, X, Y, visible)
        {
        }
    }
}
