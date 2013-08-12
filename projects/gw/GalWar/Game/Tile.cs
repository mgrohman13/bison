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

            return PathFind(ship.Tile, ship.Vector, ship.VectorZOC ? ship.Player : null);
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
            Rectangle bounds = from.Game.GetGameBounds(from, to);
            bounds.Inflate(3, 3);

            //priority queue of tiles to be traversed
            var queue = new SortedDictionary<int, HashSet<Tile>>();
            Enqueue(queue, dist, to);
            //tiles already traversed
            var closed = new HashSet<Tile>();

            //distance from start along best known path
            var distTo = new Dictionary<Tile, int>();
            distTo[to] = 0;
            //best guess total distance when moving through tile; back-index into open queue
            var distThrough = new Dictionary<Tile, int>();
            distThrough[to] = dist;

            var solutions = new Dictionary<Tile, List<Tile>>();

            while (queue.Count > 0)
            {
                //select one of the next most promising open tiles
                var a = queue.GetEnumerator();
                a.MoveNext();
                var b = a.Current.Value.GetEnumerator();
                b.MoveNext();
                Tile current = b.Current;

                if (current == from)
                    if (a.Current.Value.Count > 1)
                    {
                        //select the target last, so that we can collect all solutions
                        b.MoveNext();
                        current = b.Current;
                    }
                    else
                    {
                        //all solutions found; use secondary algorithm to determine which one to return
                        return GetBestPath(solutions, from);
                    }

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
                        {
                            List<Tile> solutionList;
                            if (!solutions.TryGetValue(neighbor, out solutionList))
                                solutions[neighbor] = ( solutionList = new List<Tile>() );

                            //check if this path is superior to the previously found path
                            if (newDist < priorDist)
                            {
                                int newGuess = newDist + GetDistance(neighbor, from);

                                //reposition in the open set
                                if (inQueue != null)
                                    Dequeue(queue, priorGuess, inQueue, neighbor);
                                Enqueue(queue, newGuess, neighbor);

                                //update distances
                                distTo[neighbor] = newDist;
                                distThrough[neighbor] = newGuess;

                                //other paths are now obsolete
                                solutionList.Clear();
                            }

                            //possible path
                            solutionList.Add(current);
                        }
                    }

                //current tile fully traversed
                Dequeue(queue, a.Current.Key, a.Current.Value, current);
                closed.Add(current);
            }

            return null;
        }
        private static void Enqueue(SortedDictionary<int, HashSet<Tile>> open, int key, Tile tile)
        {
            HashSet<Tile> set;
            if (!open.TryGetValue(key, out set))
                open[key] = ( set = new HashSet<Tile>() );
            set.Add(tile);
        }
        private static void Dequeue(SortedDictionary<int, HashSet<Tile>> open, int key, HashSet<Tile> set, Tile tile)
        {
            if (set.Count == 1)
                open.Remove(key);
            else
                set.Remove(tile);
        }
        private static bool CanMove(Player player, Tile current, Tile neighbor, Rectangle bounds)
        {
            SpaceObject spaceObject;
            return ( player == null || ( bounds.Contains(neighbor.X, neighbor.Y) &&
                    ( ( spaceObject = neighbor.SpaceObject ) == null || ( spaceObject is Ship && spaceObject.Player == player ) )
                    && Ship.CheckZOC(player, neighbor, current) ) );
        }
        private static List<Tile> GetBestPath(Dictionary<Tile, List<Tile>> solutions, Tile current)
        {
            var weights = new Dictionary<Tile, double>();
            WeightPaths(solutions, current, weights);

            List<Tile> retVal = new List<Tile>();
            retVal.Add(current);

            List<Tile> options;
            while (solutions.TryGetValue(current, out options))
            {
                double best = double.MinValue;
                foreach (Tile option in Game.Random.Iterate(options))
                {
                    double cur = weights[option];
                    if (best < cur)
                    {
                        best = cur;
                        current = option;
                    }
                }

                retVal.Add(current);
            }
            return retVal;
        }
        private static double WeightPaths(Dictionary<Tile, List<Tile>> solutions, Tile current, Dictionary<Tile, double> weights)
        {
            double weight = 0;

            List<Tile> options;
            if (solutions.TryGetValue(current, out options))
            {
                double sum = 0;
                foreach (Tile option in options)
                {
                    double cur;
                    if (!weights.TryGetValue(option, out cur))
                        cur = WeightPaths(solutions, option, weights);
                    sum += cur;
                }

                weight = options.Count + sum / ( sum + 1 );
            }

            weights.Add(current, weight);
            return weight;
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
