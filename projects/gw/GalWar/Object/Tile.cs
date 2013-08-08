using System;
using System.Collections.Generic;
using MattUtil;
using System.Drawing;

namespace GalWar
{
    //NOT Serializable
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
            return ( IsRawNeighbor(tile1, tile2.X, tile2.Y) || tile1.Teleporter == tile2 );
        }

        public static int GetDistance(Tile tile1, Tile tile2)
        {
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
                    dist = Math.Min(dist, Math.Min(GetTeleporterDistance(x1, y1, x2, y2, teleporter, subset),
                            GetTeleporterDistance(x2, y2, x1, y1, teleporter, subset)));
                }
            }
            return dist;
        }
        private static int GetTeleporterDistance(int x1, int y1, int x2, int y2, Tuple<Tile, Tile> teleporter, ICollection<Tuple<Tile, Tile>> teleporters)
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

        public static List<Tile> PathFind(Ship ship)
        {
            AssertException.Assert(ship.Vector != null);

            //MTRandom rand = ( ship == null ? null : new MTRandom(new uint[] { (uint)ship.GetHashCode(), (uint)ship.Vector.GetHashCode() }) );

            return PathFind(ship.Tile, ship.Vector, ship.VectorZOC ? ship.Player : null);//, rand);
        }
        public static List<Tile> PathFind(Tile from, Tile to)
        {
            return PathFind(from, to, null);
        }
        public static List<Tile> PathFind(Tile from, Tile to, Player player)
        {
            //    return PathFind(from, to, player, null);
            //}
            //private static List<Tile> PathFind(Tile from, Tile to, Player player, MTRandom rand)
            //{
            if (player != null)
            {
                //if (rand == null)
                //    rand = Game.Random;
                List<Tile> path = PathFindZOC(from, to, player);//, rand);
                if (path != null)
                    return path;
            }

            return PathFindNoZOC(from, to);
        }

        private static List<Tile> PathFindNoZOC(Tile from, Tile to)
        {
            List<Tile> retVal = new List<Tile>();
            retVal.Add(to);

            while (to != from)
            {
                Tile next = null;
                int minDist = int.MaxValue;
                foreach (Tile neighbor in Game.Random.Iterate(GetNeighbors(to)))
                {
                    int dist = Tile.GetDistance(neighbor, from);
                    if (dist < minDist)
                    {
                        next = neighbor;
                        minDist = dist;
                    }
                }

                retVal.Add(next);
                to = next;
            }

            retVal.Reverse();
            return retVal;
        }

        private static List<Tile> PathFindZOC(Tile from, Tile to, Player player)//, MTRandom rand)
        {
            var bounds = player.Game.GetGameBounds();
            bounds.Inflate(3, 3);

            //the set of nodes already evaluated
            var closed = new HashSet<Tile>();
            //the set of tentative nodes to be evaluated
            var open = new SortedDictionary<int, SetWithRand>();
            //the map of navigated nodes
            var cameFrom = new Dictionary<Tile, Tile>();
            //cost from start along best known path
            var gScore = new Dictionary<Tile, int>();
            //estimated total cost from start to goal through y
            var fScore = new Dictionary<Tile, int>();

            gScore[to] = 0;
            fScore[to] = GetDistance(to, from);
            Add(open, to, fScore);

            while (open.Count > 0)
            {
                var enumerator = open.GetEnumerator();
                enumerator.MoveNext();
                Tile current = enumerator.Current.Value.RandomKey();

                if (current == from)
                {
                    List<Tile> path = ReconstructPath(cameFrom, from);
                    path.Reverse();
                    return path;
                }

                Remove(open, current, fScore);
                closed.Add(current);

                int tentativeGScore = gScore[current] + 1;
                foreach (Tile neighbor in Game.Random.Iterate(Tile.GetNeighbors(current)))
                    if (bounds.Contains(neighbor.X, neighbor.Y) && Ship.CheckZOC(player, neighbor, current)
                            && ( to == neighbor || from == neighbor || neighbor.SpaceObject == null || ( neighbor.SpaceObject is Ship && neighbor.SpaceObject.Player == player ) ))
                    {
                        if (closed.Contains(neighbor) && tentativeGScore >= gScore[neighbor])
                            continue;

                        if (!Contains(open, neighbor, fScore) || tentativeGScore < gScore[neighbor])
                        {
                            cameFrom[neighbor] = current;
                            gScore[neighbor] = tentativeGScore;

                            Remove(open, neighbor, fScore);
                            fScore[neighbor] = tentativeGScore + GetDistance(neighbor, from);
                            Add(open, neighbor, fScore);
                        }
                    }
            }

            return null;
        }
        private static void Add(SortedDictionary<int, SetWithRand> open, Tile tile, Dictionary<Tile, int> fScore)
        {
            int key = fScore[tile];
            SetWithRand temp;
            if (!open.TryGetValue(key, out temp))
                open[key] = ( temp = new SetWithRand() );
            temp.Add(tile);
        }
        private static void Remove(SortedDictionary<int, SetWithRand> open, Tile tile, Dictionary<Tile, int> fScore)
        {
            int key;
            SetWithRand temp;
            if (fScore.TryGetValue(tile, out key) && open.TryGetValue(key, out temp) && temp.Remove(tile) && temp.Count == 0)
                open.Remove(key);
        }
        private static bool Contains(SortedDictionary<int, SetWithRand> open, Tile tile, Dictionary<Tile, int> fScore)
        {
            int key;
            SetWithRand temp;
            if (fScore.TryGetValue(tile, out key) && open.TryGetValue(key, out temp))
                return temp.Contains(tile);
            return false;
        }
        private static List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile current)
        {
            List<Tile> path;
            if (cameFrom.ContainsKey(current))
                path = ReconstructPath(cameFrom, cameFrom[current]);
            else
                path = new List<Tile>();
            path.Add(current);
            return path;
        }

        //an ICollection of Tiles that provides RandomKey, Add, Contains, and Remove operations all in constant time
        private class SetWithRand : ICollection<Tile>
        {
            private Dictionary<Tile, int> indices;
            private List<Tile> contents;

            public SetWithRand()
            {
                indices = new Dictionary<Tile, int>();
                contents = new List<Tile>();
            }

            public Tile RandomKey()
            {
                return contents[Game.Random.Next(contents.Count)];
            }

            #region ICollection<Tile> Members

            public void Add(Tile item)
            {
                indices.Add(item, contents.Count);
                contents.Add(item);
            }

            public void Clear()
            {
                indices.Clear();
                contents.Clear();
            }

            public bool Contains(Tile item)
            {
                return indices.ContainsKey(item);
            }

            public void CopyTo(Tile[] array, int arrayIndex)
            {
                contents.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get
                {
                    return contents.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public bool Remove(Tile item)
            {
                int index;
                if (indices.TryGetValue(item, out index))
                {
                    Tile move = contents[contents.Count - 1];

                    indices[move] = index;
                    indices.Remove(item);

                    contents[index] = move;
                    contents.RemoveAt(contents.Count - 1);

                    return true;
                }
                return false;
            }

            #endregion

            #region IEnumerable<Tile> Members

            public IEnumerator<Tile> GetEnumerator()
            {
                return contents.GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
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
