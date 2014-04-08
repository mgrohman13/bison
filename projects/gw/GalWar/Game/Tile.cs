using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Drawing;
using MattUtil;

namespace GalWar
{
    //NOT Serializable
    public class Tile
    {
        #region static

        public static HashSet<Tile> GetNeighbors(Tile tile)
        {
            AssertException.Assert(tile != null);

            var neighbors = new HashSet<Tile>();
            //loop through the nine regional tiles in the square grid
            for (int x = -1 ; x <= 1 ; ++x)
                for (int y = -1 ; y <= 1 ; ++y)
                {
                    int x2 = tile.X + x;
                    int y2 = tile.Y + y;
                    //check this is truly a neighbor in the hexagonal grid
                    if (IsRawNeighbor(tile, x2, y2))
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
            AssertException.Assert(tile1 != null);
            AssertException.Assert(tile2 != null);

            return ( IsRawNeighbor(tile1, tile2.X, tile2.Y) || tile1.Teleporter == tile2 );
        }

        public static int GetDistance(Tile tile1, Tile tile2)
        {
            AssertException.Assert(tile1 != null);
            AssertException.Assert(tile2 != null);

            return GetDistance(tile1.X, tile1.Y, tile2.X, tile2.Y, tile1.Game.GetTeleporters());
        }

        private static bool IsRawNeighbor(Tile tile, int x2, int y2)
        {
            return ( GetRawDistance(tile.X, tile.Y, x2, y2) == 1 );
        }
        private static int GetDistance(int x1, int y1, int x2, int y2, ICollection<Tuple<Tile, Tile>> teleporters)
        {
            int dist = GetRawDistance(x1, y1, x2, y2);
            if (dist > 1 && teleporters != null && teleporters.Count > 0)
            {
                ICollection<Tuple<Tile, Tile>> subset = null;
                if (teleporters.Count > 1)
                    subset = new HashSet<Tuple<Tile, Tile>>(teleporters);
                foreach (var teleporter in teleporters)
                {
                    if (subset != null)
                        subset.Remove(teleporter);
                    dist = Math.Min(dist, GetTeleporterDistance(x1, y1, x2, y2, teleporter, dist, subset));
                    if (dist > 1)
                        dist = Math.Min(dist, GetTeleporterDistance(x2, y2, x1, y1, teleporter, dist, subset));
                    if (dist == 1)
                        break;
                }
            }
            return dist;
        }
        private static int GetTeleporterDistance(int x1, int y1, int x2, int y2, Tuple<Tile, Tile> teleporter, int cutoff, ICollection<Tuple<Tile, Tile>> teleporters)
        {
            int dist = 1 + GetDistance(x1, y1, teleporter.Item1.X, teleporter.Item1.Y, teleporters);
            if (dist < cutoff)
                dist += GetDistance(x2, y2, teleporter.Item2.X, teleporter.Item2.Y, teleporters);
            return dist;
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

        public static List<Tile> PathFind(Ship ship)
        {
            AssertException.Assert(ship != null);
            AssertException.Assert(ship.Vector != null);

            return ( PathFind(ship.Tile, ship.Vector, ship.VectorZOC ? ship.Player : null)
                    ?? PathFind(ship.Tile, ship.Vector, null) );
        }
        public static List<Tile> PathFind(Tile from, Tile to)
        {
            return PathFind(from, to, null);
        }

        public static List<Tile> PathFind(Tile from, Tile to, Player player)
        {
            AssertException.Assert(to != null);
            AssertException.Assert(from != null);

            Rectangle bounds = from.Game.GetGameBounds(to, from);
            int count = 0, max = bounds.Width * bounds.Height;

            return TBSUtil.PathFind(Game.Random, from, to, current =>
            {
                if (++count > max)
                    return Enumerable.Empty<Tuple<Tile, int>>();
                return Tile.GetNeighbors(current).Where(neighbor => CanMove(player, current, neighbor))
                        .Select(neighbor => new Tuple<Tile, int>(neighbor, 1));
            }, GetDistance);
        }
        private static bool CanMove(Player player, Tile current, Tile neighbor)
        {
            SpaceObject spaceObject;
            //a null player means we don't want to do any collision checking at all
            return ( player == null || ( ( ( spaceObject = neighbor.SpaceObject ) == null
                    || ( spaceObject is Ship && spaceObject.Player == player ) ) && Ship.CheckZOC(player, neighbor, current) ) );
        }

        #endregion //static

        #region fields and constructors

        public readonly Game Game;

        private readonly short _x, _y;

        internal Tile(Game game, int x, int y)
        {
            checked
            {
                this.Game = game;

                this._x = (short)x;
                this._y = (short)y;
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
                return Game.GetSpaceObject(X, Y);
            }
            internal set
            {
                checked
                {
                    if (( value is Colony ) || ( ( value == null ) == ( SpaceObject == null ) ))
                        throw new Exception();

                    Game.SetSpaceObject(X, Y, value);
                }
            }
        }

        #endregion //fields and constructors

        #region public

        public Tile Teleporter
        {
            get
            {
                int index;
                return GetTeleporter(out index);
            }
        }
        public int TeleporterNumber
        {
            get
            {
                int index;
                GetTeleporter(out index);
                return index;
            }
        }
        private Tile GetTeleporter(out int number)
        {
            number = 0;
            foreach (Tuple<Tile, Tile> teleporter in Game.GetTeleporters())
            {
                ++number;
                if (teleporter.Item1 == this)
                    return teleporter.Item2;
                else if (teleporter.Item2 == this)
                    return teleporter.Item1;
            }

            number = -1;
            return null;
        }

        public override string ToString()
        {
            return "(" + this.X + "," + this.Y + ")";
        }

        #endregion //public

        #region equality

        public override int GetHashCode()
        {
            return ( ( ( this.X << 16 ) | this.Y ) ^ Game.GetHashCode() );
        }

        public override bool Equals(object obj)
        {
            Tile tile = ( obj as Tile );
            bool equals = ( tile != null );
            if (equals)
                equals = ( this.X == tile.X && this.Y == tile.Y && this.Game == tile.Game );
            return equals;
        }

        public static bool operator ==(Tile t1, Tile t2)
        {
            return Equals(t1, t2);
        }
        public static bool operator !=(Tile t1, Tile t2)
        {
            return !Equals(t1, t2);
        }

        #endregion
    }
}
