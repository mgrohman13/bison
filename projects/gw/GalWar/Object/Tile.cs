using System;
using System.Collections.Generic;
using MattUtil;

namespace GalWar
{
    public class Tile
    {
        #region static

        public static HashSet<Tile> GetNeighbors(Tile tile)
        {
            HashSet<Tile> neighbors = new HashSet<Tile>();
            //loop through the nine regional tiles in the square grid
            for (int x = -2 ; ++x < 2 ; )
                for (int y = -2 ; ++y < 2 ; )
                {
                    int x2 = tile.X + x;
                    int y2 = tile.Y + y;
                    //check this is truly a neighbor in the hexagonal grid
                    if (IsNeighbor(tile, x2, y2))
                        neighbors.Add(tile.Game.GetTile(x2, y2));
                }
            //add teleport destination if there is one
            Tile teleporter = tile.Teleporter;
            if (teleporter != null)
                neighbors.Add(teleporter);
            return neighbors;
        }

        public static bool IsNeighbor(Tile tile1, Tile tile2)
        {
            return IsNeighbor(tile1, tile2.X, tile2.Y);
        }

        public static int GetDistance(Tile tile1, Tile tile2)
        {
            return GetDistance(tile1, tile2.X, tile2.Y);
        }

        private static bool IsNeighbor(Tile tile, int x, int y)
        {
            return ( GetDistance(tile, x, y) == 1 );
        }
        private static int GetDistance(Tile tile, int x, int y)
        {
            return GetDistance(tile.X, tile.Y, x, y, tile.Game.GetTeleporters());
        }
        private static int GetDistance(int x1, int y1, int x2, int y2, List<Tuple<Point, Point>> teleporters)
        {
            int dist = GetRawDistance(x1, y1, x2, y2);
            if (dist > 1)
                foreach (Tuple<Point, Point> teleporter in teleporters)
                {
                    List<Tuple<Point, Point>> subset = new List<Tuple<Point, Point>>(teleporters);
                    subset.Remove(teleporter);
                    dist = Math.Min(dist, Math.Min(GetTeleporterDistance(x1, y1, x2, y2, teleporter, subset),
                            GetTeleporterDistance(x2, y2, x1, y1, teleporter, subset)));
                }
            return dist;
        }
        private static int GetTeleporterDistance(int x1, int y1, int x2, int y2, Tuple<Point, Point> teleporter, List<Tuple<Point, Point>> teleporters)
        {
            return 1 + GetDistance(x1, y1, teleporter.Item1.X, teleporter.Item1.Y, teleporters)
                    + GetDistance(x2, y2, teleporter.Item2.X, teleporter.Item2.Y, teleporters);
        }
        private static int GetRawDistance(int x1, int y1, int x2, int y2)
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

        private readonly short _x, _y;

        private SpaceObject _spaceObject;

        internal Tile(Game game, Point point)
        {
            checked
            {
                this.Game = game;

                this._x = (short)point.X;
                this._y = (short)point.Y;

                this._spaceObject = null;
            }
        }

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

        public SpaceObject SpaceObject
        {
            get
            {
                return this._spaceObject;
            }
            internal set
            {
                checked
                {
                    if (( value == null ) == ( this.SpaceObject == null ) || value is Colony)
                        throw new Exception();

                    this._spaceObject = value;

                    this.Game.SetSpaceObject(X, Y, value);
                }
            }
        }

        #endregion //fields and constructors

        #region public

        public Point Point
        {
            get
            {
                return new Point(this.X, this.Y);
            }
        }

        public Tile Teleporter
        {
            get
            {
                int index;
                return GetTeleporter(out index);
            }
        }
        public Tile GetTeleporter(out int number)
        {
            Tuple<Point, Point> teleporter = Game.GetTeleporter(this.Point, out number);
            if (teleporter == null)
                return null;
            if (teleporter.Item1 == this.Point)
                return Game.GetTile(teleporter.Item2);
            else if (teleporter.Item2 == this.Point)
                return Game.GetTile(teleporter.Item1);
            else
                throw new Exception();
        }

        public override string ToString()
        {
            return "(" + this.X + "," + this.Y + ")";
        }

        #endregion //public
    }
}
