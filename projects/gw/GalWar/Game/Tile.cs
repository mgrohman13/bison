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
            //add wormhole destination if there is one
            Wormhole wormhole = tile.Wormhole;
            if (wormhole != null)
                neighbors.UnionWith(wormhole.Tiles.Except(new Tile[] { tile }));
            return neighbors;
        }

        public static bool IsNeighbor(Tile tile1, Tile tile2)
        {
            AssertException.Assert(tile1 != null);
            AssertException.Assert(tile2 != null);

            bool isNeighbor = IsRawNeighbor(tile1, tile2.X, tile2.Y);
            if (!isNeighbor && tile1 != tile2)
            {
                Wormhole wormhole = tile1.Wormhole;
                if (wormhole != null)
                    isNeighbor = wormhole.Tiles.Contains(tile2);
            }
            else if (tile1 == tile2)
                ;
            return isNeighbor;
        }

        public static int GetDistance(Tile tile1, Tile tile2)
        {
            AssertException.Assert(tile1 != null);
            AssertException.Assert(tile2 != null);

            return GetDistance(tile1.X, tile1.Y, tile2.X, tile2.Y, tile1.Game.GetWormholes());
        }

        public static bool IsRawNeighbor(Tile tile, int x2, int y2)
        {
            return ( GetRawDistance(tile.X, tile.Y, x2, y2) == 1 );
        }
        private static int GetDistance(int x1, int y1, int x2, int y2, ICollection<Wormhole> wormholes)
        {
            int dist = GetRawDistance(x1, y1, x2, y2);
            if (dist > 1 && wormholes != null && wormholes.Count > 0)
            {
                ICollection<Wormhole> subset = null;
                if (wormholes.Count > 1)
                    subset = new HashSet<Wormhole>(wormholes);
                foreach (var wormhole in wormholes)
                {
                    if (subset != null)
                        subset.Remove(wormhole);
                    dist = Math.Min(dist, GetWormholeDistance(x1, y1, x2, y2, wormhole, dist, subset));
                    if (dist == 1)
                        return 1;
                }
            }
            return dist;
        }
        private static int GetWormholeDistance(int x1, int y1, int x2, int y2, Wormhole wormhole, int minDist, ICollection<Wormhole> subset)
        {
            foreach (Tile t1 in wormhole.Tiles)
            {
                int dist = 1 + GetDistance(x1, y1, t1.X, t1.Y, subset);
                if (dist < minDist)
                {
                    dist += wormhole.Tiles.Min(t2 => t1 == t2 ? minDist : GetDistance(x2, y2, t2.X, t2.Y, subset));
                    if (dist == 1)
                        return 1;
                    minDist = Math.Min(minDist, dist);
                }
            }
            return minDist;
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

            bool checkTarget = !( ship.Vector.SpaceObject != null && !( ship.Vector.SpaceObject is Ship && ship.Player == ship.Vector.SpaceObject.Player ) );
            return ( PathFind(ship.Tile, ship.Vector, ship.VectorZOC ? ship.Player : null, checkTarget)
                    ?? PathFind(ship.Tile, ship.Vector, null, checkTarget) );
        }
        public static List<Tile> PathFind(Tile from, Tile to, Player player, bool checkTarget, int? max = null)
        {
            AssertException.Assert(to != null);
            AssertException.Assert(from != null);

            if (!max.HasValue)
            {
                Rectangle bounds = from.Game.GetGameBounds(to, from);
                max = bounds.Right + bounds.Bottom - bounds.Top - bounds.Left + 13;
            }

            return TBSUtil.PathFind(Game.Random, from, to, current =>
            {
                if (GetDistance(current, from) + GetDistance(current, to) >= max)
                    return Enumerable.Empty<Tuple<Tile, int>>();
                return Tile.GetNeighbors(current).Where(neighbor => CanMove(player, current, neighbor, checkTarget ? null : to))
                        .Select(neighbor => new Tuple<Tile, int>(neighbor, 1));
            }, GetDistance);
        }
        private static bool CanMove(Player player, Tile current, Tile neighbor, Tile skipZOC)
        {
            SpaceObject spaceObject;
            //a null player means we don't want to do any collision checking at all
            return ( player == null || ( ( ( spaceObject = neighbor.SpaceObject ) == null || spaceObject is Anomaly
                   || ( spaceObject is Ship && spaceObject.Player == player ) )
                    && ( ( skipZOC != null && current == skipZOC ) || Ship.CheckZOC(player, neighbor, current) ) ) );
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

        public Wormhole Wormhole
        {
            get
            {
                int index;
                return GetWormhole(out index);
            }
        }
        public int WormholeNumber
        {
            get
            {
                int index;
                GetWormhole(out index);
                return index;
            }
        }
        private Wormhole GetWormhole(out int number)
        {
            number = 0;
            foreach (Wormhole wormhole in Game.GetWormholes())
            {
                ++number;
                if (wormhole.Tiles.Contains(this))
                    return wormhole;
            }

            number = -1;
            return null;
        }

        public Player GetZOC()
        {
            var ships = GetNeighbors(this)//.Concat(new Tile[] { this })
                    .Select(tile => tile.SpaceObject).OfType<Ship>();
            if (ships.Count() > 0)
            {
                int def = ships.Max(ship => ship.Def);
                var players = ships.Where(ship => ship.Def == def).Select(ship => ship.Player).Distinct();
                if (players.Count() == 1)
                    return players.Single();
            }
            return null;
        }

        public string GetLoction()
        {
            return new MattUtil.Point(this.X - Game.Center.X, this.Y - Game.Center.Y).ToString();
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
