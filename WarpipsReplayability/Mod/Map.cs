using HarmonyLib;
using LevelGeneration;
using LevelGeneration.WorldMap;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using static WarpipsReplayability.Mod.Operations;

namespace WarpipsReplayability.Mod
{
    internal class Map
    {
        //for testing and debugging different world maps
        public static readonly int? ForceWorldMapIndex = null;

        //properties set externally through patches
        public static MissionManagerAsset MissionManagerAsset { get; set; }
        public static WorldMapAsset WorldMapAsset { get; set; }
        public static TerritoryInstance[] Territories { get; set; }
        public static bool DoShuffle { get; set; }

        public static List<WorldMapAsset.TerritoryConnection> OriginalConnections { get; private set; }

        public static void Randomize()
        {
            //DoShuffle = false;
            //Persist.SaveNew(Enumerable.Range(0, Territories.Length).ToArray());

            if (DoShuffle)
            {
                Plugin.Log.LogInfo($"{Territories.Length} territories");

                DoShuffle = false;

                int[] shuffle = RandomizeTerritories();
                var graph = ModifyConnections();
                GenerateConnections(graph);

                SpawnerInfo[] spawnerInfo = Operations.Randomize();

                Persist.SaveNew(shuffle, spawnerInfo);
            }
        }

        public static void Load()
        {
            SetOriginalConnections();
            Persist.Load();
            LoadShuffle();
            if (!Validate(null, null))
                Plugin.Log.LogError($"loaded invalid state");

            Operations.Load(Persist.Instance.SpawnerInfo);
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

            LoadTechRewards();

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
        public static void LoadTechRewards()
        {
            for (int c = 0; c < Territories.Length; c++)
                Territories[c].operation.techReward = Persist.Instance.TechRewards[c];
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

                InitTechRewards(territory);
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

        private static void InitTechRewards(TerritoryInstance territory)
        {
            //randomize tech rewards, with an average of slightly more per territory   

            Operation operation = territory.operation;

            int techReward = operation.techReward;
            if (techReward % 5 != 0)
                Plugin.Log.LogError($"techReward already randomized {techReward}");

            //note - if you don't fully complete islands, you should capture 32 territories before the final mission (9*3+5)
            //with Math.PI bonus, on average this means an additional ~100.5 tech points
            float bonus = (float)Math.PI;// (float)(Config.RebalanceTech ? Math.E : 0);

            float avg = techReward + bonus, dev = 3.9f / techReward + .052f, oe = 1.69f / techReward + .039f;

            //multiply the average tech points by the relative number of missions you can complete
            //since this is only applied to non-special territories, you will still end up with more tech points on easier difficulties
            //but this allows me to give more for General and still avoid leaving you swimming in tech points on easier 
            float mult = MissionManagerAsset.GameDifficultyIndex switch
            {
                3 => 8f / 11,
                1 => 8f / 10,
                0 => 8f / 9,
                2 => 8f / 8,
                _ => throw new Exception($"Map.MissionManagerAsset.GameDifficultyIndex {MissionManagerAsset.GameDifficultyIndex}"),
            };

            //if (Config.RebalanceTech)
            if (territory.specialTag == TerritoryInstance.SpecialTag.None)
            {
                Plugin.Log.LogDebug($"GameDifficultyIndex: {MissionManagerAsset.GameDifficultyIndex}, mult: {mult}");
                avg *= mult;
            }
            else
            {
                //special territories only have the bonus reduced, not the full amount
                avg -= bonus * (1 - mult * mult);
            }

            if (techReward >= 5)
                operation.techReward = Plugin.Rand.GaussianOEInt(avg, dev, oe, 3);
            Plugin.Log.LogInfo($"techReward {techReward} -> {operation.techReward} ({avg * (1 - oe):0.00}, {dev * avg * (1 - oe):0.00}, {oe * avg:0.00})");
        }
        public static void ReduceTechRewards()
        {
            //when difficulty bar hits max, reduce non-special territory tech rewards
            //if (Config.RebalanceTech)
            for (int a = 0; a < Territories.Length; a++)
                if (Territories[a].specialTag == TerritoryInstance.SpecialTag.None)
                    Persist.Instance.TechRewards[a] = Plugin.Rand.Round(Persist.Instance.TechRewards[a] / 3f);

            LoadTechRewards();
            Persist.SaveCurrent();

            Plugin.Log.LogInfo($"reduced tech rewards for standard territories");
        }

        private static GraphInfo ModifyConnections()
        {
            GraphInfo graph = GetEdges(Territories);
            int count = WorldMapAsset.TerritoryConnections.Count;

            int numEdges = CountEdges(graph.Edges);
            //sever roughly half the extra edges, still ensuring a fully connected graph 
            float avg = (numEdges + count - 3f) / 2f;
            //int min = Math.Max((int)Math.Ceiling(2 * avg - numEdges), count - 1);
            //Plugin.Log.LogInfo($"GaussianCappedInt {avg}, .065, {min}"); 

            if (avg < count - 1)
            {
                Plugin.Log.LogError($"OriginalConnections did not restore properly ({numEdges} {avg} {count - 1})");
                return graph;
            }

            int target = Plugin.Rand.GaussianCappedInt(avg, .065f, count - 1);
            //target = numEdges;
            Plugin.Log.LogInfo($"{numEdges} edges, target {target} (avg {avg:0.0})");

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
            // todo: vary with difficulty
            const int numMissions = 9;
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

            //path to end must allow completion without seeing super soldiers
            List<TerritoryInstance> path = TBSUtil.PathFind(Plugin.Rand, graph.Start, graph.End, GetNeighbors, GetDistance);
            //Plugin.Log.LogInfo($"path {path}");
            int pathLength = path.Count - 1;
            //Plugin.Log.LogInfo($"pathLength {pathLength}");
            if (pathLength > numMissions)
            {
                Plugin.Log.LogInfo($"({CountEdges(graph.Edges)}) pathLength {pathLength}, invalid");
                return false;
            }

            //Plugin.Log.LogInfo($"here");

            //reachable territories counting end but not start
            HashSet<TerritoryInstance> reachable = new();
            DFS(graph.Start);
            //Plugin.Log.LogInfo($"reachable.Count {reachable.Count}");

            if (reachable.Count != territories.Length - 1)
            {
                Plugin.Log.LogInfo($"({CountEdges(graph.Edges)}) reachable.Count {reachable.Count}, invalid");
                return false;
            }

            ////must be able to reach enough territories to max out difficulty bar
            //if (reachable.Count < Math.Min(territories.Length - 1, numMissions))
            //{
            //    Plugin.Log.LogInfo($"({CountEdges(graph.Edges)}) reachable.Count {reachable.Count}, invalid");
            //    return false;
            //}

            ////Plugin.Log.LogInfo($"here2");

            ////all speical rewards should be reachable 
            //if (graph.Rewards.Any(r => !reachable.Contains(r)))
            //{
            //    Plugin.Log.LogInfo($"({CountEdges(graph.Edges)}) end is blocking reward path, invalid");
            //    return false;
            //}

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
            SetOriginalConnections();

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

        private static void SetOriginalConnections()
        {
            if (OriginalConnections == null)
            {
                OriginalConnections = WorldMapAsset.TerritoryConnections
                    .Select(c => new WorldMapAsset.TerritoryConnection()
                    {
                        connection = c.connection.ToList()
                    }).ToList();
                Plugin.Log.LogInfo($"OriginalConnections {OriginalConnections.Sum(c => c.connection.Count)}");
            }
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
                    Plugin.Log.LogDebug($"{a} -> {b}");

                    //this is a bug, they meant 3 should connect to 2, not 1
                    if ((MissionManagerAsset.WorldMapIndex == 0 && a == 3 && b == 1)
                        //this one seems intentional, as it is relevant in the unmodded game, but graphically they don't look adjacent 
                        //I'm removing it because it looks extra wack if 0 <-> 2 gets severed but 0 <-> 3 remains
                        || (MissionManagerAsset.WorldMapIndex == 3 && a == 0 && b == 3))
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
