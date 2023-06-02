using System;
using System.Collections.Generic;

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
            for (int xAdd = -1; xAdd <= 1; ++xAdd)
                for (int yAdd = -1; yAdd <= 1; ++yAdd)
                {
                    int x = X + xAdd;
                    int y = Y + yAdd;
                    if (x >= 0 && x < 18 && y >= 0 && y < 18 && GetDistance(X, Y, x, y) == 1)
                        yield return Program.Map[x, y];
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
            else if ((yDist % 2 != 0) && ((y2 % 2 == 0) == (x2 < x1)))
                --xDist;
            return yDist + xDist;
        }

        internal void Outpost(Player player)
        {
            if (Inf != null)
                throw new Exception();
            Inf = new string[] { $"F {player.Name.Split(' ')[1]}", null };
        }
    }
}
