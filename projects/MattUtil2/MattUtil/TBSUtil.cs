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
            int numShuffles = random.OEInt(playerLength * shuffle);
            bool[] affected = new bool[playerLength];
            for (int a = 0 ; a < numShuffles ; ++a)
            {
                int index = random.Next(playerLength);
                int swap = ( index - 1 );
                Player player = players[index];

                if (index == 0 || affected[index] || affected[swap])
                {
                    int amt;
                    retVal.TryGetValue(player, out amt);
                    retVal[player] = amt + 1;
                }
                else
                {
                    affected[swap] = true;
                    affected[index] = true;

                    players[index] = players[swap];
                    players[swap] = player;
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
        public static List<Tile> PathFind<Tile>(MTRandom random, Tile from, Tile to, Func<Tile, IEnumerable<Tuple<Tile, int>>> GetNeighbors, Func<Tile, Tile, int> GetDistance)
            where Tile : class
        {
            if (from == null)
                throw new ArgumentNullException("from");
            if (to == null)
                throw new ArgumentNullException("to");
            if (GetNeighbors == null)
                throw new ArgumentNullException("GetNeighbors");

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
                Tile current = pair.Value.FirstOrDefault(tile => ( tile != from ));

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

        private static void Enqueue<Tile, TKey, TValue>(IDictionary<TKey, TValue> queue, TKey key, Tile tile)
             where TValue : ICollection<Tile>
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
            Dictionary<Tile, BigInteger> weights = WeightPaths(solutions, current, 1);

            List<Tile> options;
            while (solutions.TryGetValue(current, out options))
            {
                //choose the path with the highest weight
                BigInteger max = options.Max(option => weights[option]);
                options = options.Where(option => ( weights[option] == max )).ToList();
                //choose randomly if equivalent
                current = random.SelectValue(options);

                retVal.Add(current);
            }
            return retVal;
        }
        private static Dictionary<Tile, BigInteger> WeightPaths<Tile>(Dictionary<Tile, List<Tile>> solutions, Tile start, BigInteger level, int max = -1, Dictionary<Tile, BigInteger> weights = null)
        {
            if (max == -1)
                max = solutions.Values.Max(list => list.Count);
            if (weights == null)
                weights = new Dictionary<Tile, BigInteger>();

            if (!weights.ContainsKey(start))
            {
                BigInteger weight = 1;
                List<Tile> next;
                if (solutions.TryGetValue(start, out next))
                {
                    BigInteger sum = 0;
                    foreach (Tile tile in next)
                    {
                        weights = WeightPaths(solutions, tile, level * max, max, weights);
                        sum += weights[tile];
                    }
                    weight = sum;
                }
                weights.Add(start, weight);
            }

            return weights;

            ////since all solution paths have the same length, we can group tiles into 'levels' by their distance from the start
            //var levels = new List<HashSet<Tile>> { new HashSet<Tile> { start } };
            //while (true)
            //{
            //    HashSet<Tile> next = new HashSet<Tile>();
            //    List<Tile> step;
            //    foreach (Tile cur in levels[levels.Count - 1])
            //        if (solutions.TryGetValue(cur, out step))
            //            next.UnionWith(step);
            //        else
            //            break;
            //    if (next.Count == 0)
            //        break;
            //    levels.Add(next);
            //}

            //var weights = new Dictionary<Tile, BigInteger> { { levels[levels.Count - 1].Single(), 0 } };

            //BigInteger mult = 0;
            //for (int a = levels.Count - 2 ; a > 0 ; --a)
            //{
            //    BigInteger total = 1;
            //    foreach (Tile cur in levels[a])
            //    {
            //        //the weight is primarily the number of immediate path options, and secondarily the weight of those options
            //        List<Tile> options = solutions[cur];
            //        BigInteger weight = mult * options.Count;
            //        foreach (Tile o in options)
            //            weight += weights[o];
            //        total += weight;

            //        weights.Add(cur, weight);
            //    }

            //    //the mult for the next level is set to one higher than the total sum of the current level
            //    //so that a single immediate option always outweighs all subsequent options
            //    mult = total;
            //}

            //return weights;
        }
    }
}
