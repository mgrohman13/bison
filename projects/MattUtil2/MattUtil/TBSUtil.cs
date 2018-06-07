using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;

namespace MattUtil
{
    public static class TBSUtil
    {
        public static Dictionary<Player, int> RandMoveOrder<Player>(MTRandom random, IList<Player> players, double shuffle)
        {
            Dictionary<Player, int> retVal = new Dictionary<Player, int>();

            int playerLength = players.Count;
            if (playerLength > 1)
            {
                int numShuffles = random.GaussianOEInt(( playerLength - 1 ) * shuffle, .39, .26);
                bool[] affected = new bool[playerLength];
                for (int a = 0 ; a < numShuffles ; ++a)
                {
                    int index = random.Next(1, playerLength);
                    int swap = index - 1;

                    if (!affected[index] && !affected[swap])
                    {
                        Player player = players[index];
                        Player next = players[swap];

                        int amt;
                        retVal.TryGetValue(player, out amt);
                        retVal[player] = amt - 1;

                        retVal.TryGetValue(next, out amt);
                        retVal[next] = amt + 1;

                        affected[index] = true;
                        affected[swap] = true;

                        players[index] = next;
                        players[swap] = player;
                    }
                }
            }

            return retVal;
        }

        //performs a binary search to find the minimum value (when trueHigh is true, otherwise the maximum value)
        //for which Predicate returns true
        public static int FindValue(Predicate<int> Predicate, int min, int max, bool trueHigh)
        {
            if (trueHigh)
                --min;
            else
                ++max;
            int mid;
            while (( mid = max - min ) > 1)
            {
                mid = min + mid / 2;
                if (Predicate(mid))
                    if (trueHigh)
                        max = mid;
                    else
                        min = mid;
                else if (trueHigh)
                    min = mid;
                else
                    max = mid;
            }
            return ( trueHigh ? max : min );
        }

        public static double FindValue(Func<double, double> Func, double target, double min, double max)
        {
            //int steps = 0;
            while (min != max)
            {
                //++steps;
                double mid = ( min + max ) / 2.0;
                if (mid == min || mid == max)
                {
                    if (Math.Abs(Func(min) - target) > Math.Abs(Func(max) - target))
                        min = max;
                    break;
                }
                double result = Func(mid);
                if (result <= target)
                {
                    min = mid;
                    if (result == target)
                        break;
                }
                else
                {
                    max = mid;
                }
            }
            //Console.WriteLine(steps);
            return min;
        }

        public static void SaveGame(object game, string path, string fileName)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            SaveGame(game, path.TrimEnd('/', '\\') + "/" + fileName);
        }

        public static void SaveGame(object game, string filePath)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                new BinaryFormatter().Serialize(memory, game);
                using (Stream file = new FileStream(filePath, FileMode.Create))
                using (Stream compress = new DeflateStream(file, CompressionMode.Compress))
                    memory.WriteTo(compress);
            }
        }

        public static T LoadGame<T>(string filePath)
        {
            using (Stream file = new FileStream(filePath, FileMode.Open))
            using (Stream decompress = new DeflateStream(file, CompressionMode.Decompress))
                return (T)new BinaryFormatter().Deserialize(decompress);
        }

        //A* Pathfinding Algorithm
        public static List<Tile> PathFind<Tile>(MTRandom random, Tile from, Tile to,
                Func<Tile, IEnumerable<Tuple<Tile, int>>> GetNeighbors, Func<Tile, Tile, int> GetDistance)
                where Tile : class
        {
            if (random == null)
                throw new ArgumentNullException("random");
            if (from == null)
                throw new ArgumentNullException("from");
            if (to == null)
                throw new ArgumentNullException("to");
            if (GetNeighbors == null)
                throw new ArgumentNullException("GetNeighbors");
            if (GetDistance == null)
                throw new ArgumentNullException("GetDistance");

            int dist = GetDistance(to, from);

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
                Tile current = pair.Value.FirstOrDefault(tile => tile != from);

                //all solutions found; use secondary algorithm to determine which one to return
                if (current == null)
                    return GetBestPath(random, solutions, from);

                int curDist = distTo[current];
                foreach (var tuple in GetNeighbors(current))
                {
                    Tile neighbor = tuple.Item1;
                    int newDist = curDist + tuple.Item2;

                    //a non-null inQueue set means the tile is already in the queue
                    HashSet<Tile> inQueue = null;
                    int priorGuess;
                    if (distThrough.TryGetValue(neighbor, out priorGuess) && queue.TryGetValue(priorGuess, out inQueue)
                            && !inQueue.Contains(neighbor))
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

        private static void Enqueue<Tile, TKey, TValue>(IDictionary<TKey, TValue> queue, TKey key, Tile tile)
                where TValue : ICollection<Tile>, new()
        {
            TValue col;
            if (!queue.TryGetValue(key, out col))
                queue[key] = ( col = Activator.CreateInstance<TValue>() );
            col.Add(tile);
        }
        private static void Dequeue<Tile>(SortedDictionary<int, HashSet<Tile>> queue, int key, HashSet<Tile> set, Tile tile)
        {
            if (set.Count == 1)
                queue.Remove(key);
            else
                set.Remove(tile);
        }

        private static List<Tile> GetBestPath<Tile>(MTRandom random, Dictionary<Tile, List<Tile>> solutions, Tile current)
        {
            var retVal = new List<Tile> { current };

            //weight paths based on which ones keep the most options available
            var weights = new Dictionary<Tile, List<BigInteger>>();
            WeightPaths(solutions, current, weights);

            List<Tile> allOptions;
            while (solutions.TryGetValue(current, out allOptions))
            {
                //choose the path with the highest weight
                if (allOptions.Count == 1)
                {
                    current = allOptions[0];
                }
                else
                {
                    var options = allOptions.Select(tile => new Tuple<Tile, List<BigInteger>>(tile, weights[tile]));

                    for (int a = 0 ; options.Skip(1).Any() ; ++a)
                    {
                        int b = a;

                        var filtered = options.Where(tuple => tuple.Item2.Count <= b);
                        if (filtered.Any())
                        {
                            options = filtered;
                            break;
                        }

                        BigInteger max = options.Max(tuple => tuple.Item2[b]);
                        options = options.Where(tuple => tuple.Item2[b] == max);
                    }

                    //choose randomly if equivalent
                    current = random.SelectValue(options).Item1;
                }

                retVal.Add(current);
            }
            return retVal;
        }
        private static void WeightPaths<Tile>(Dictionary<Tile, List<Tile>> solutions, Tile start, Dictionary<Tile, List<BigInteger>> weights)
        {
            var weight = new List<BigInteger>();

            List<Tile> next;
            if (solutions.TryGetValue(start, out next))
            {
                weight.Add(next.Count);

                var children = new List<List<BigInteger>>();
                foreach (Tile tile in next)
                {
                    List<BigInteger> child;
                    if (!weights.TryGetValue(tile, out child))
                    {
                        WeightPaths(solutions, tile, weights);
                        child = weights[tile];
                    }
                    children.Add(child);
                }

                int a = children.Min(child => child.Count);
                foreach (var child in children)
                    for (int b = 1 ; b <= a ; ++b)
                    {
                        BigInteger entry = child[b - 1];
                        if (b == weight.Count)
                            weight.Add(entry);
                        else
                            weight[b] = weight[b] + entry;
                    }
            }

            weights.Add(start, weight);
        }
    }
}
