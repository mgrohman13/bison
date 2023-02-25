﻿using LevelGeneration;
using LevelGeneration.WorldMap;
using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using GameIO;
using MonoMod.Utils;
using HarmonyLib;

namespace WarpipsReplayability.Mod
{
    internal class Map
    {
        //no reference from WorldMapAsset to MissionManagerAsset so this gets set externally
        public static MissionManagerAsset MissionManagerAsset { get; set; }
        public static WorldMapAsset WorldMapAsset { get; set; }
        public static bool DoShuffle { get; internal set; }

        public static void Randomize()
        {
            if (DoShuffle)
            {
                DoShuffle = false;

                RandomizeTerritories();
                var graph = ModifyConnections();
                GenerateConnections(graph);

                GameRandom.Shuffled = true;
            }
        }
        public static bool ValidateShuffle(MTRandom rand, bool applyShuffle)
        {
            return Validate(rand, applyShuffle, null);
        }
        public static void LoadShuffle()
        {
            LogShuffle();

            var territories = GameRandom.Territories;
            TerritoryData[] data = territories.Select(t => t.operation).Select(o => NewTerritoryData(o)).ToArray();
            for (int a = 0; a < territories.Length; a++)
            {
                int b = GameRandom.Shuffle[a];
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
                {
                    to.spawnWaveProfile = from.spawnWaveProfile;
                }
            }

            Plugin.Log.LogInfo("Restored shuffle for save");
            LogShuffle();

            GameRandom.Shuffled = true;

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

        private static void RandomizeTerritories()
        {
            LogShuffle();

            var territories = GameRandom.Territories.ToArray();
            //var operations = territories.Select(t => t.operation).ToArray();
            for (int a = 0; a < territories.Length; a++)
            {
                int b = GameRandom.Shuffle[a];
                TerritoryInstance territory = territories[a];
                GameRandom.Territories[b] = territory;
                //territory.operation = operations[a];
                territory.index = b;

                if (territory.operation.techReward > 1)
                    territory.operation.techReward = Plugin.Rand.GaussianCappedInt(territory.operation.techReward - 1, .13, 1);
            }

            Plugin.Log.LogInfo("Shuffled territories");
            LogShuffle();
        }
        private static void ApplyShuffle(TerritoryInstance[] territories)
        {
            for (int a = 0; a < territories.Length; a++)
            {
                int b = GameRandom.Shuffle[a];
                territories[b] = GameRandom.Territories[a];
            }
        }
        private static void LogShuffle()
        {
            Operation[] operations = GameRandom.Territories.Select(t => t.operation).ToArray();
            //Plugin.Log.LogInfo(operations.Select(o => o.spawnWaveProfile.name).Aggregate((a, b) => a + "," + b));
            Plugin.Log.LogInfo(operations.Select(o => o.spawnWaveProfile.GetInstanceID().ToString()).Aggregate((a, b) => a + "," + b));
        }

        private static GraphInfo ModifyConnections()
        {
            GraphInfo graph = GetEdges(GameRandom.Territories);
            int count = WorldMapAsset.TerritoryConnections.Count;

            int numEdges = CountEdges(graph.Edges);
            //sever roughly half the extra edges, still ensuring a fully connected graph 
            double avg = (numEdges + count - 2.6) / 2.0;
            int min = (int)Math.Ceiling(2 * avg - numEdges);
            //Plugin.Log.LogInfo($"GaussianCappedInt {avg}, .065, {min}");
            int target = Plugin.Rand.GaussianCappedInt(avg, .065, min);
            Plugin.Log.LogInfo($"{numEdges} edges, target {target}");

            int attempts = 0, maxAttempts = numEdges * numEdges * (numEdges - target);
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
                if (Validate(Plugin.Rand, false, graph))
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

        private static bool Validate(MTRandom random, bool applyShuffle, GraphInfo graph)
        {
            //Plugin.Log.LogInfo($"GameRandom.Territories {GameRandom.Territories}");
            //Plugin.Log.LogInfo($"graph {graph}");

            var territories = GameRandom.Territories.ToArray();
            if (applyShuffle)
                ApplyShuffle(territories);
            if (applyShuffle || graph == null)
                graph = GetEdges(territories);

            //Plugin.Log.LogInfo($"graph.Start {graph.Start}");
            //Plugin.Log.LogInfo($"graph.End {graph.End}");
            //Plugin.Log.LogInfo($"graph.Rewards {graph.Rewards}");

            //path to end must be no longer than 10 to allow completion without seeing super soldiers
            List<TerritoryInstance> path = TBSUtil.PathFind(random, graph.Start, graph.End, GetNeighbors, GetDistance);
            //Plugin.Log.LogInfo($"path {path}");
            int pathLength = path.Count - 1;
            if (pathLength > 10)
            {
                Plugin.Log.LogInfo($"pathLength {pathLength}, invalid");
                return false;
            }

            //Plugin.Log.LogInfo($"here");

            //speical rewards should not be blocked by the end goal   
            if (graph.Rewards.Any(reward => TBSUtil.PathFind(random, graph.Start, reward, GetNeighbors, GetDistance).Contains(graph.End)))
            {
                Plugin.Log.LogInfo("end is blocking reward path, invalid");
                return false;
            }

            return true;

            IEnumerable<Tuple<TerritoryInstance, int>> GetNeighbors(TerritoryInstance t) =>
                graph.Edges[Array.IndexOf(territories, t)].Select(e => new Tuple<TerritoryInstance, int>(territories[e], 1));
            static int GetDistance(TerritoryInstance a, TerritoryInstance b) => 1;
        }
        private static void GenerateConnections(GraphInfo graph)
        {
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
            public HashSet<int>[] Edges = new HashSet<int>[GameRandom.Territories.Length];
            public TerritoryInstance Start, End;
            public HashSet<TerritoryInstance> Rewards = new();
        }
    }
}
