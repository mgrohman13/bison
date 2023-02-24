using LevelGeneration;
using LevelGeneration.WorldMap;
using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;

namespace WarpipsReplayability
{
    internal class Map
    {
        //no reference from WorldMapAsset to MissionManagerAsset so this gets set externally
        public static MissionManagerAsset MissionManagerAsset { get; set; }

        private static TerritoryInstance Start { get; set; }
        private static TerritoryInstance End { get; set; }
        private static HashSet<TerritoryInstance> Rewards { get; set; }

        public static void Randomize(WorldMapAsset worldMapAsset, TerritoryInstance[] territories)
        {
            ShuffleTerritories(territories);
            ModifyConnections(worldMapAsset, territories);
        }

        private static void ShuffleTerritories(TerritoryInstance[] territories)
        {
            Operation[] operations = new Operation[territories.Length];
            for (int a = 0; a < territories.Length; a++)
                operations[a] = territories[a].operation;

            Plugin.Rand.Shuffle(territories);
            for (int c = 0; c < territories.Length; c++)
            {
                territories[c].operation = operations[territories[c].index];
                territories[c].index = c;
            }

            Plugin.Log.LogInfo("Shuffled territories");
        }

        private static void ModifyConnections(WorldMapAsset worldMapAsset, TerritoryInstance[] territories)
        {
            HashSet<int>[] edges = GetEdges(worldMapAsset, territories);
            SeverEdges(edges, worldMapAsset.TerritoryConnections.Count);
            ValidateAndFinalize(worldMapAsset, territories, edges);
        }

        private static HashSet<int>[] GetEdges(WorldMapAsset worldMapAsset, TerritoryInstance[] territories)
        {
            int count = worldMapAsset.TerritoryConnections.Count;
            HashSet<int>[] edges = InitEdgeArray(count);
            Rewards = new();

            for (int a = 0; a < count; a++)
            {
                TerritoryInstance territory = territories[a];
                if (territory.specialTag == TerritoryInstance.SpecialTag.PlayerBase)
                    Start = territory;
                else if (territory.specialTag == TerritoryInstance.SpecialTag.EnemyObjective)
                    End = territory;
                else if (territory.specialTag == TerritoryInstance.SpecialTag.HighValueReward)
                    Rewards.Add(territory);

                foreach (int connection in worldMapAsset.TerritoryConnections[a].connection)
                {
                    //TerritoryConnections are inexplicably 1-based
                    int b = connection - 1;

                    //this is a bug, they meant 3 should connect to 2, not 1
                    if (MissionManagerAsset.WorldMapIndex == 0 && a == 3 && b == 1)
                    {
                        Plugin.Log.LogInfo($"connection bugfix {a} -/> {b}");
                        continue;
                    }

                    edges[a].Add(b);
                    edges[b].Add(a);
                }
            }

            Plugin.Log.LogInfo("Cleaned up connections");

            return edges;
        }

        private static void SeverEdges(HashSet<int>[] edges, int count)
        {
            static int CountEdges(HashSet<int>[] collection) => collection.Sum(e => e.Count) / 2;

            int numEdges = CountEdges(edges);
            //sever roughly half the edges, still ensuring a fully connected graph 
            double avg = (count - 1 + numEdges) / 2.0;
            int min = (int)Math.Ceiling(2 * avg - numEdges);
            Plugin.Log.LogInfo($"GaussianCappedInt {avg}, .065, {min}");
            int target = Plugin.Rand.GaussianCappedInt(avg, .065, min);
            Plugin.Log.LogInfo($"target {target} edges");
            while (numEdges > target)
            {
                HashSet<int>[] critical = FindCritical(edges, count);
                //Plugin.Log.LogInfo($"critical = {CountEdges(critical)}");
                if (CountEdges(critical) == numEdges)
                    break;

                int a, b;
                do
                {
                    a = Plugin.Rand.SelectValue(Enumerable.Range(0, count), a => edges[a].Count);
                    b = Plugin.Rand.SelectValue(edges[a]);
                } while (critical[a].Contains(b));

                //Plugin.Log.LogInfo($"Remove {a} - {b}");
                edges[a].Remove(b);
                edges[b].Remove(a);
                numEdges--;
            }
        }
        public static HashSet<int>[] FindCritical(HashSet<int>[] edges, int count)
        {
            int[] levels = new int[count];
            HashSet<int>[] critical = InitEdgeArray(count);

            DFS(edges, levels, critical, -1, 0, 1);

            return critical;
        }
        private static int DFS(HashSet<int>[] edges, int[] levels, HashSet<int>[] critical, int parent, int node, int level)
        {
            levels[node] = level;

            int minLevel = level;
            if (edges[node] != null)
                foreach (int edge in edges[node])
                    if (edge != parent)
                    {
                        int childLevel = levels[edge];
                        if (childLevel == 0)
                        {
                            childLevel = DFS(edges, levels, critical, node, edge, level + 1);
                            if (childLevel > level)
                            {
                                critical[node].Add(edge);
                                critical[edge].Add(node);
                            }
                        }
                        minLevel = Math.Min(minLevel, childLevel);
                    }
            return minLevel;
        }

        private static void ValidateAndFinalize(WorldMapAsset worldMapAsset, TerritoryInstance[] territories, HashSet<int>[] edges)
        {
            IEnumerable<Tuple<TerritoryInstance, int>> GetNeighbors(TerritoryInstance t) =>
                edges[t.index].Select(e => new Tuple<TerritoryInstance, int>(territories[e], 1));
            int GetDistance(TerritoryInstance a, TerritoryInstance b) => 1;

            //path to end must be no longer than 10 to allow completion without seeing super soldiers
            int pathLength = TBSUtil.PathFind(Plugin.Rand, Start, End, GetNeighbors, GetDistance).Count - 1;
            if (pathLength > 10)
            {
                Plugin.Log.LogInfo($"pathLength {pathLength}, retrying");
                Randomize(worldMapAsset, territories);
                return;
            }

            //speical rewards should not be blocked by the end goal   
            if (Rewards.Any(reward => TBSUtil.PathFind(Plugin.Rand, Start, reward, GetNeighbors, GetDistance).Contains(End)))
            {
                Plugin.Log.LogInfo("end is blocking reward path, retrying");
                Randomize(worldMapAsset, territories);
                return;
            }

            //accept setup and finalize map
            GenerateConnections(worldMapAsset, edges);
        }

        private static void GenerateConnections(WorldMapAsset worldMapAsset, HashSet<int>[] edges)
        {
            int count = worldMapAsset.TerritoryConnections.Count;
            worldMapAsset.TerritoryConnections.Clear();
            for (int c = 0; c < count; c++)
            {
                WorldMapAsset.TerritoryConnection connection = new()
                {
                    //maintain 1-based indexing 
                    connection = edges[c].Select(d => d + 1).ToList()
                };
                worldMapAsset.TerritoryConnections.Add(connection);

                //Plugin.Log.LogInfo($"{c} -> ({connection.connection.Aggregate("", (x, y) => x + ", " + y)})");
            }

            Plugin.Log.LogInfo($"Generated {edges.Sum(e => e.Count) / 2} connections");
        }

        private static HashSet<int>[] InitEdgeArray(int count)
        {
            HashSet<int>[] edges = new HashSet<int>[count];
            for (int c = 0; c < count; c++)
                edges[c] = new();
            return edges;
        }
    }
}
