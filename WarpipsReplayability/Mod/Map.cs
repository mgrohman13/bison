using LevelGeneration;
using LevelGeneration.WorldMap;
using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using GameIO;
using MonoMod.Utils;
using HarmonyLib;
using System.Xml.Linq;

namespace WarpipsReplayability.Mod
{
    internal class Map
    {
        //properties set externally through patches
        public static MissionManagerAsset MissionManagerAsset { get; set; }
        public static WorldMapAsset WorldMapAsset { get; set; }
        public static TerritoryInstance[] Territories { get; set; }
        public static bool DoShuffle { get; set; }

        public static List<WorldMapAsset.TerritoryConnection> OriginalConnections { get; private set; }

        public static void Randomize()
        {
            if (DoShuffle)
            {
                DoShuffle = false;

                int[] shuffle = RandomizeTerritories();
                var graph = ModifyConnections();
                GenerateConnections(graph);

                Persist.SaveData(shuffle);
            }
        }
        public static void Load()
        {
            Persist.LoadData();
            LoadShuffle();
            if (!Validate(null, null))
                Plugin.Log.LogError($"loaded invalid state");
        }

        private static void LoadShuffle()
        {
            //LogShuffle();

            var territories = Territories;
            TerritoryData[] data = territories.Select(t => t.operation).Select(o => NewTerritoryData(o)).ToArray();

            for (int a = 0; a < territories.Length; a++)
            {
                int b = Persist.Instance.Shuffle[a];
                TerritoryData from = data[a];
                Operation to = territories[b].operation;

                //AssignBaselineData sets the following fields based on the underlying territory assets
                //we must restore it instead from the shuffled values
                to.operationTargetHealth = from.operationTargetHealth;
                to.revealEnemyIcons = from.revealEnemyLineup;
                to.showBattleIntro = from.showBattleIntro;
                OperationRewardProfile reward = from.operationRewardProfile;
                if (reward)
                {
                    to.techReward = reward.TechReward;
                    to.tokenReward = reward.TokenReward;
                }
                if (from.spawnWaveProfile)
                    to.spawnWaveProfile = from.spawnWaveProfile;
            }

            for (int c = 0; c < territories.Length; c++)
                territories[c].operation.techReward = Persist.Instance.TechRewards[c];

            Plugin.Log.LogInfo("Restored shuffle for save");
            LogShuffle();

            static TerritoryData NewTerritoryData(Operation operation)
            {
                TerritoryData data = new()
                {
                    operationTargetHealth = operation.operationTargetHealth,
                    revealEnemyLineup = operation.revealEnemyIcons,
                    showBattleIntro = operation.showBattleIntro,
                    operationRewardProfile = new(),
                    spawnWaveProfile = operation.spawnWaveProfile,
                };
                OperationRewardProfile reward = data.operationRewardProfile;
                AccessTools.Field(typeof(OperationRewardProfile), "techReward").SetValue(reward, operation.techReward);
                AccessTools.Field(typeof(OperationRewardProfile), "tokenReward").SetValue(reward, operation.tokenReward);
                return data;
            };
        }
        private static int[] RandomizeTerritories()
        {
            //LogShuffle();

            int[] shuffle;
            do
                shuffle = Plugin.Rand.Iterate(Territories.Length).ToArray();
            while (!Validate(shuffle, null));

            var territories = Territories.ToArray();
            //var operations = territories.Select(t => t.operation).ToArray();
            for (int a = 0; a < territories.Length; a++)
            {
                int b = shuffle[a];
                TerritoryInstance territory = territories[a];
                Territories[b] = territory;
                //territory.operation = operations[a];
                territory.index = b;

                //randomize tech rewards, with an average of one less per territory
                int techReward = territory.operation.techReward;
                if (techReward % 5 != 0)
                    Plugin.Log.LogError($"techReward already randomized {techReward}");
                if (techReward > 1)
                    territory.operation.techReward = Plugin.Rand.GaussianCappedInt(techReward - 1, .169, 1);
            }

            Plugin.Log.LogInfo("Shuffled territories");
            LogShuffle();

            return shuffle;
        }
        private static void ApplyShuffle(TerritoryInstance[] territories, int[] shuffle)
        {
            for (int a = 0; a < territories.Length; a++)
            {
                int b = shuffle[a];
                territories[b] = Territories[a];
            }
        }
        private static void LogShuffle()
        {
            Operation[] operations = Territories.Select(t => t.operation).ToArray();
            //Plugin.Log.LogInfo(operations.Select(o => o.spawnWaveProfile.name).Aggregate((a, b) => a + "," + b));
            Plugin.Log.LogInfo(operations.Select(o => o.spawnWaveProfile.GetInstanceID().ToString()).Aggregate((a, b) => a + "," + b));
        }

        private static GraphInfo ModifyConnections()
        {
            GraphInfo graph = GetEdges(Territories);
            int count = WorldMapAsset.TerritoryConnections.Count;

            int numEdges = CountEdges(graph.Edges);
            //sever roughly half the extra edges (-1), still ensuring a fully connected graph 
            double avg = (numEdges + count - 3) / 2.0;
            //int min = Math.Max((int)Math.Ceiling(2 * avg - numEdges), count - 1);
            //Plugin.Log.LogInfo($"GaussianCappedInt {avg}, .065, {min}");
            int target = Plugin.Rand.GaussianCappedInt(avg, .065, count - 1);
            Plugin.Log.LogInfo($"{numEdges} edges, target {target}");

            int attempts = 0, maxAttempts = 6 * numEdges * (numEdges - target);
            while (numEdges > target && attempts < maxAttempts)
            {
                HashSet<int>[] critical = FindCritical(graph.Edges, count);
                //Plugin.Log.LogInfo($"critical = {CountEdges(critical)}");
                if (CountEdges(critical) == numEdges)
                    break;

                int a, b;
                do
                {
                    a = Plugin.Rand.SelectValue(Enumerable.Range(0, count), c => graph.Edges[c].Count * graph.Edges[c].Count);
                    b = Plugin.Rand.SelectValue(graph.Edges[a]);
                } while (critical[a].Contains(b));

                graph.Edges[a].Remove(b);
                graph.Edges[b].Remove(a);
                if (Validate(null, graph))
                {
                    //Plugin.Log.LogInfo($"Remove {a} - {b}");
                    numEdges--;
                }
                else
                {
                    graph.Edges[a].Add(b);
                    graph.Edges[b].Add(a);
                }
                attempts++;
            }
            Plugin.Log.LogInfo($"Edge removal attempts {attempts} (max {maxAttempts})");

            return graph;

            static HashSet<int>[] FindCritical(HashSet<int>[] edges, int count)
            {
                int[] levels = new int[count];
                HashSet<int>[] critical = InitEdgeArray(count);
                DFS(edges, levels, critical, -1, 0, 1);
                return critical;
            }
            static int DFS(HashSet<int>[] edges, int[] levels, HashSet<int>[] critical, int parent, int node, int level)
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
        }

        private static bool Validate(int[] shuffle, GraphInfo graph)
        {
            //Plugin.Log.LogInfo($"GameRandom.Territories {GameRandom.Territories}");
            //Plugin.Log.LogInfo($"graph {graph}");

            var territories = Territories.ToArray();
            if (shuffle != null)
            {
                ApplyShuffle(territories, shuffle);
                graph = null;
            }
            graph ??= GetEdges(territories);

            //Plugin.Log.LogInfo($"graph.Start {graph.Start}");
            //Plugin.Log.LogInfo($"graph.End {graph.End}");
            //Plugin.Log.LogInfo($"graph.Rewards {graph.Rewards}");

            //path to end must be no longer than 10 to allow completion without seeing super soldiers
            List<TerritoryInstance> path = TBSUtil.PathFind(Plugin.Rand, graph.Start, graph.End, GetNeighbors, GetDistance);
            //Plugin.Log.LogInfo($"path {path}");
            int pathLength = path.Count - 1;
            //Plugin.Log.LogInfo($"pathLength {pathLength}");
            if (pathLength > 10)
            {
                Plugin.Log.LogInfo($"pathLength {pathLength}, invalid");
                return false;
            }

            //Plugin.Log.LogInfo($"here");

            //reachable territories counting end but not start
            HashSet<TerritoryInstance> reachable = new();
            DFS(graph.Start);
            //Plugin.Log.LogInfo($"reachable.Count {reachable.Count}");

            //must be able to reach at least 10 territories 
            if (reachable.Count < Math.Min(territories.Length - 1, 10))
            {
                Plugin.Log.LogInfo($"reachable.Count {reachable.Count}, invalid");
                return false;
            }

            //Plugin.Log.LogInfo($"here2");

            //all speical rewards should be reachable 
            if (graph.Rewards.Any(r => !reachable.Contains(r)))
            {
                Plugin.Log.LogInfo("end is blocking reward path, invalid");
                return false;
            }

            return true;

            void DFS(TerritoryInstance parent)
            {
                foreach (var child in GetNeighbors(parent).Select(t => t.Item1))
                    if (child != graph.Start && !reachable.Contains(child))
                    {
                        reachable.Add(child);
                        if (child != graph.End)
                            DFS(child);
                    }
            }
            IEnumerable<Tuple<TerritoryInstance, int>> GetNeighbors(TerritoryInstance t) =>
                graph.Edges[Idx(t)].Select(e => new Tuple<TerritoryInstance, int>(territories[e], 1));
            static int GetDistance(TerritoryInstance a, TerritoryInstance b) => 1;
            int Idx(TerritoryInstance t) => Array.IndexOf(territories, t);
        }

        private static void GenerateConnections(GraphInfo graph)
        {
            OriginalConnections = WorldMapAsset.TerritoryConnections
                .Select(c => new WorldMapAsset.TerritoryConnection() { connection = c.connection.ToList() }).ToList();

            int count = WorldMapAsset.TerritoryConnections.Count;
            WorldMapAsset.TerritoryConnections.Clear();
            for (int c = 0; c < count; c++)
            {
                WorldMapAsset.TerritoryConnection connection = new()
                {
                    //maintain 1-based indexing 
                    connection = graph.Edges[c].Select(d => d + 1).ToList()
                };
                WorldMapAsset.TerritoryConnections.Add(connection);

                //Plugin.Log.LogInfo($"{c} -> ({connection.connection.Aggregate("", (x, y) => x + ", " + y)})");
            }

            Plugin.Log.LogInfo($"Generated {CountEdges(graph.Edges)} connections");
        }

        private static GraphInfo GetEdges(TerritoryInstance[] territories)
        {
            int count = WorldMapAsset.TerritoryConnections.Count;
            GraphInfo graph = new() { Edges = InitEdgeArray(count) };

            for (int a = 0; a < count; a++)
            {
                TerritoryInstance territory = territories[a];
                if (territory.specialTag == TerritoryInstance.SpecialTag.PlayerBase)
                    graph.Start = territory;
                else if (territory.specialTag == TerritoryInstance.SpecialTag.EnemyObjective)
                    graph.End = territory;
                else if (territory.specialTag == TerritoryInstance.SpecialTag.HighValueReward)
                    graph.Rewards.Add(territory);

                foreach (int connection in WorldMapAsset.TerritoryConnections[a].connection)
                {
                    //TerritoryConnections are inexplicably 1-based
                    int b = connection - 1;

                    //this is a bug, they meant 3 should connect to 2, not 1
                    if (MissionManagerAsset.WorldMapIndex == 0 && a == 3 && b == 1)
                    {
                        Plugin.Log.LogInfo($"connection bugfix {a} -/> {b}");
                        continue;
                    }

                    graph.Edges[a].Add(b);
                    graph.Edges[b].Add(a);
                }
            }

            //Plugin.Log.LogInfo("Cleaned up connections");
            return graph;
        }
        private static HashSet<int>[] InitEdgeArray(int count)
        {
            HashSet<int>[] edges = new HashSet<int>[count];
            for (int c = 0; c < count; c++)
                edges[c] = new();
            return edges;
        }
        private static int CountEdges(HashSet<int>[] edges) => edges.Sum(e => e.Count) / 2;

        private class GraphInfo
        {
            public HashSet<int>[] Edges = new HashSet<int>[Territories.Length];
            public TerritoryInstance Start, End;
            public HashSet<TerritoryInstance> Rewards = new();
        }
    }
}
