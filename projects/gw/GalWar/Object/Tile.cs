using System;
using System.Collections.Generic;

namespace GalWar
{
    [Serializable]
    public class Tile
    {
        #region static

        public static HashSet<Tile> GetNeighbors(Tile tile)
        {
            HashSet<Tile> neighbors = new HashSet<Tile>();
            Tile[,] map = tile.Game.GetMap();
            //loop through the nine regional tiles in the square grid
            for (int x = -2 ; ++x < 2 ; )
                for (int y = -2 ; ++y < 2 ; )
                {
                    int x2 = tile.X + x;
                    int y2 = tile.Y + y;
                    //check this is inside map bounds and truly a neighbor in the hexagonal grid
                    if (x2 >= 0 && y2 >= 0 && x2 < tile.Game.Diameter && y2 < tile.Game.Diameter && IsNeighbor(tile.X, tile.Y, x2, y2))
                    {
                        Tile t = map[x2, y2];
                        //coordinates may be outside the playing area, represented by a null tile
                        if (t != null)
                            neighbors.Add(t);
                    }
                }
            return neighbors;
        }

        public static bool IsNeighbor(Tile tile1, Tile tile2)
        {
            return IsNeighbor(tile1.X, tile1.Y, tile2.X, tile2.Y);
        }

        public static bool IsNeighbor(int x1, int y1, int x2, int y2)
        {
            return ( GetDistance(x1, y1, x2, y2) == 1 );
        }

        public static int GetDistance(Tile tile1, Tile tile2)
        {
            return GetDistance(tile1.X, tile1.Y, tile2.X, tile2.Y);
        }

        public static int GetDistance(int x1, int y1, int x2, int y2)
        {
            int yDist = Math.Abs(y2 - y1);
            int xDist = Math.Abs(x2 - x1) - yDist / 2;
            //determine if the odd y distance will save an extra x move or not
            if (xDist < 1)
                xDist = 0;
            else if (( yDist % 2 != 0 ) && ( ( y2 % 2 != 0 ) == ( x2 < x1 ) ))
                --xDist;
            return yDist + xDist;
        }

        #endregion //static

        #region fields and constructors

        public readonly Game Game;

        private ISpaceObject _spaceObject;

        private readonly byte _x, _y;

        internal Tile(Game game, int x, int y)
        {
            this.Game = game;

            checked
            {
                this._x = (byte)x;
                this._y = (byte)y;
            }

            this._spaceObject = null;
        }

        #endregion //fields and constructors

        #region public

        public int X
        {
            get
            {
                return this._x;
            }
        }
        public int Y
        {
            get
            {
                return this._y;
            }
        }

        public ISpaceObject SpaceObject
        {
            get
            {
                return this._spaceObject;
            }
            internal set
            {
                if (( value == null ) == ( this._spaceObject == null ))
                    throw new Exception();

                this._spaceObject = value;
            }
        }

        public override string ToString()
        {
            return "(" + this.X + "," + this.Y + ")";
        }

        #endregion //public
    }
}
