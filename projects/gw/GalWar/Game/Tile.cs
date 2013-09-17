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

        //A* Pathfinding Algorithm
        public static List<Tile> PathFind(Tile from, Tile to, Player player)
        {
            AssertException.Assert(to != null);
            AssertException.Assert(from != null);

            int dist = GetDistance(to, from);

            //boundary of all objects currently in play, with enough room to move around the edge
            Rectangle bounds = from.Game.GetGameBounds(to, from);
            bounds.Inflate(3, 3);

            //priority queue of tiles to be traversed
            var queue = new SortedDictionary<int, HashSet<Tile>> { { dist, new HashSet<Tile> { to } } };
            //tiles already traversed
            var closed = new HashSet<Tile>();

            //distance from start along best known path
            var distTo = new Dictionary<Tile, int> { { to, 0 } };
            //best guess total distance when moving through tile; back-index into queue
            var distThrough = new Dictionary<Tile, int> { { to, dist } };

            var solutions = new Dictionary<Tile, List<Tile>>();

            while (queue.Count > 0)
            {
                //select the next tile from the queue, skipping the target so that we can collect all solutions
                var pair = queue.First();
                Tile current = pair.Value.FirstOrDefault(tile => ( tile != from ));

                //all solutions found; use secondary algorithm to determine which one to return
                if (current == null)
                    return GetBestPath(solutions, from);

                int newDist = distTo[current] + 1;

                foreach (Tile neighbor in Tile.GetNeighbors(current))
                    if (CanMove(player, current, neighbor, bounds))
                    {
                        //a non-null inQueue set means the tile is already in the queue
                        HashSet<Tile> inQueue = null;
                        int priorGuess;
                        if (distThrough.TryGetValue(neighbor, out priorGuess) && queue.TryGetValue(priorGuess, out inQueue) && !inQueue.Contains(neighbor))
                            inQueue = null;

                        //default to MaxValue so that the inner if will evaluate true
                        int priorDist = int.MaxValue;

                        //check if we have not looked at this tile yet, or if this path is equal to or shorter than the previous path
                        if (( inQueue == null && !closed.Contains(neighbor) ) || newDist <= ( priorDist = distTo[neighbor] ))
                            //check if this path is superior to the previously found path
                            if (newDist < priorDist)
                            {
                                int newGuess = newDist + GetDistance(neighbor, from);

                                //reposition in the queue
                                if (inQueue != null)
                                    Dequeue(queue, priorGuess, inQueue, neighbor);
                                Enqueue(queue, newGuess, neighbor);

                                //update distances
                                distTo[neighbor] = newDist;
                                distThrough[neighbor] = newGuess;

                                //other paths are now obsolete
                                solutions[neighbor] = new List<Tile> { current };
                            }
                            else
                            {
                                //add as a possible path
                                Enqueue(solutions, neighbor, current);
                            }
                    }

                //current tile fully traversed
                Dequeue(queue, pair.Key, pair.Value, current);
                closed.Add(current);
            }

            return null;
        }
        private static void Enqueue<TKey, TValue>(IDictionary<TKey, TValue> queue, TKey key, Tile tile)
             where TValue : ICollection<Tile>
        {
            TValue col;
            if (!queue.TryGetValue(key, out col))
                queue[key] = ( col = Activator.CreateInstance<TValue>() );
            col.Add(tile);
        }
        private static void Dequeue(SortedDictionary<int, HashSet<Tile>> queue, int key, HashSet<Tile> set, Tile tile)
        {
            if (set.Count == 1)
                queue.Remove(key);
            else
                set.Remove(tile);
        }
        private static bool CanMove(Player player, Tile current, Tile neighbor, Rectangle bounds)
        {
            SpaceObject spaceObject;
            //a null player means we don't want to do any collision checking at all
            return ( player == null || ( bounds.Contains(neighbor.X, neighbor.Y) &&
                    ( ( spaceObject = neighbor.SpaceObject ) == null || ( spaceObject is Ship && spaceObject.Player == player ) )
                    && Ship.CheckZOC(player, neighbor, current) ) );
        }

        private static List<Tile> GetBestPath(Dictionary<Tile, List<Tile>> solutions, Tile current)
        {
            var retVal = new List<Tile> { current };

            //weight paths based on which ones keep the most options available
            Dictionary<Tile, BigInteger> weights = WeightPaths(solutions, current);

            List<Tile> options;
            while (solutions.TryGetValue(current, out options))
            {
                //choose the path with the highest weight
                BigInteger max = options.Max(option => weights[option]);
                options = options.Where(option => ( weights[option] == max )).ToList();
                //choose randomly if equivalent
                current = options[Game.Random.Next(options.Count)];

                retVal.Add(current);
            }
            return retVal;
        }
        private static Dictionary<Tile, BigInteger> WeightPaths(Dictionary<Tile, List<Tile>> solutions, Tile start)
        {
            //since all solution paths have the same length, we can group tiles into 'levels' by their distance from the start
            var levels = new List<HashSet<Tile>> { new HashSet<Tile> { start } };
            while (true)
            {
                HashSet<Tile> next = new HashSet<Tile>();
                List<Tile> step;
                foreach (Tile cur in levels[levels.Count - 1])
                    if (solutions.TryGetValue(cur, out step))
                        next.UnionWith(step);
                    else
                        break;
                if (next.Count == 0)
                    break;
                levels.Add(next);
            }

            var weights = new Dictionary<Tile, BigInteger> { { levels[levels.Count - 1].Single(), 0 } };

            BigInteger mult = 0;
            for (int a = levels.Count - 2 ; a > 0 ; --a)
            {
                BigInteger total = 1;
                foreach (Tile cur in levels[a])
                {
                    //the weight is primarily the number of immediate path options, and secondarily the weight of those options
                    List<Tile> options = solutions[cur];
                    BigInteger weight = mult * options.Count;
                    foreach (Tile o in options)
                        weight += weights[o];
                    total += weight;

                    weights.Add(cur, weight);
                }

                //the mult for the next level is set to one higher than the total sum of the current level
                //so that a single immediate option always outweighs all subsequent options
                mult = total;
            }

            return weights;
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
