using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;

namespace NCWMap
{
    public class Tile
    {
        public readonly int X, Y;
        public bool Water;
        public string[] Inf;

        public Tile(int x, int y)
        {
            this.X = x;
            this.Y = y;
            this.Water = false;
            this.Inf = null;
        }

        public IEnumerable<Tile> GetNeighbors()
        {
            for (int xAdd = -1 ; xAdd <= 1 ; ++xAdd)
                for (int yAdd = -1 ; yAdd <= 1 ; ++yAdd)
                {
                    int x = this.X + xAdd;
                    int y = this.Y + yAdd;
                    if (GetDistance(this.X, this.Y, x, y) == 1)
                    {
                        if (!( x < 0 || x >= Program.Width || y < 0 || y >= Program.Height ))
                            yield return Program.Map[x, y];
                    }
                }
        }
        public static int GetDistance(Tile t1, Tile t2)
        {
            return GetDistance(t1.X, t1.Y, t2.X, t2.Y);
        }
        private static int GetDistance(int x1, int y1, int x2, int y2)
        {
            int yDist = Math.Abs(y2 - y1);
            int xDist = Math.Abs(x2 - x1) - yDist / 2;
            //determine if the odd y distance will save an extra x move or not
            if (xDist < 1)
                xDist = 0;
            else if (( yDist % 2 != 0 ) && ( ( y2 % 2 == 0 ) == ( x2 < x1 ) ))
                --xDist;
            return yDist + xDist;
        }
    }
}
