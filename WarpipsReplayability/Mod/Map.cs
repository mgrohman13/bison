﻿using HarmonyLib;
using LevelGeneration;
using LevelGeneration.WorldMap;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static WarpipsReplayability.Mod.Operations;
using TerritoryConnection = LevelGeneration.WorldMap.WorldMapAsset.TerritoryConnection;

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

        private static readonly FieldInfo _tokenReward = AccessTools.Field(typeof(OperationRewardProfile), "tokenReward");
        private static readonly FieldInfo _techReward = AccessTools.Field(typeof(OperationRewardProfile), "techReward");
        private static readonly FieldInfo _territoryConnections = AccessTools.Field(typeof(WorldMapAsset), "territoryConnections");

        public static void Randomize()
        {
            SetOriginalConnections(DoShuffle);

            if (DoShuffle)
            {
                Plugin.Log.LogInfo($"{Territories.Length} territories");

                DoShuffle = false;

                int[] shuffle = RandomizeTerritories();
                var graph = ModifyConnections();
                GenerateConnections(graph);

                OperationInfo[] spawnerInfo = Operations.Randomize();

                Persist.SaveNew(shuffle, spawnerInfo);
            }
        }

        public static void Load()
        {
            try
            {
                SetOriginalConnections(false);

                Persist.Load();
                LoadShuffle();
                if (!Validate(doRand: false))
                    throw new Exception($"loaded invalid state");

                Operations.Load(Persist.Instance.OperationInfo);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Error loading Persist, shuffling instead");
                Plugin.Log.LogError(e);

                DoShuffle = true;
                Randomize();
            }
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
            LogShuffle(Territories);

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
                _techReward.SetValue(reward, operation.techReward);
                _tokenReward.SetValue(reward, operation.tokenReward);
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
            while (!Validate(shuffle));

            var territories = Territories.ToArray();
            //var operations = territories.Select(t => t.operation).ToArray();
            for (int a = 0; a < territories.Length; a++)
            {
                int b = shuffle[a];
                TerritoryInstance territory = territories[a];
                Territories[b] = territory;
                territory.index = b;

                InitTechRewards(territory);
            }

            Plugin.Log.LogInfo("Shuffled territories");
            LogShuffle(Territories);

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
        private static void LogShuffle(TerritoryInstance[] territories)
        {
            Operation[] operations = territories.Select(t => t.operation).ToArray();
            //Plugin.Log.LogInfo(operations.Select(o => o.spawnWaveProfile.name).Aggregate((a, b) => a + "," + b));
            Plugin.LogAtLevel(operations.Select(o => o.spawnWaveProfile.GetInstanceID().ToString()).Aggregate((a, b) => a + "," + b),
                territories.Count() != territories.Distinct().Count());
        }

        private static void InitTechRewards(TerritoryInstance territory)
        {
            //randomize tech rewards, with an average of slightly more per territory   

            Operation operation = territory.operation;

            int techReward = operation.techReward;
            if (techReward % 5 != 0)
            {
                Plugin.Log.LogError($"techReward already randomized {techReward}");
                return;
            }

            //if you don't fully complete islands, you should capture 32 territories before the final mission (9*3+5)
            //with Math.PI bonus, on average this means an additional ~100.5 tech points (on General)
            float bonus = (float)Math.PI;// (float)(Config.RebalanceTech ? Math.PI : 0);

            float avg = techReward + bonus;

            //multiply the average tech points by the relative number of missions you can complete
            //since this is only applied to non-special territories, you will still end up with more tech points on easier difficulties
            //but this allows me to give more for General and still avoid leaving you swimming in tech points on easier 
            int gameDifficultyIndex = MissionManagerAsset.GameDifficultyIndex;
            float mult = gameDifficultyIndex switch
            {
                3 => 8f / 11,
                1 => 8f / 10,
                0 => 8f / 9,
                2 => 8f / 8,
                _ => throw new Exception($"Map.MissionManagerAsset.GameDifficultyIndex {gameDifficultyIndex}")
            };
            Plugin.Log.LogDebug($"GameDifficultyIndex: {gameDifficultyIndex}, mult: {mult}");

            //standard territories have the full amount reduced, special territories only the bonus 
            if (territory.specialTag == TerritoryInstance.SpecialTag.None)
                avg *= mult;
            else
                avg -= (float)(bonus * (1.0 - Math.Pow(mult, 6.5)));

            if (techReward >= 5)
            {
                float dev = 3.9f / techReward + .052f, oe = 2.1f / techReward + .021f;
                //float dev = 3.9f / avg + .052f, oe = 2.1f / avg + .021f;
                operation.techReward = Plugin.Rand.GaussianOEInt(avg, dev, oe, 3);
                Plugin.Log.LogInfo($"techReward {techReward} -> {operation.techReward} ({avg * (1 - oe):0.00}, {dev * avg * (1 - oe):0.00}, {oe * avg:0.00})");
            }
        }
        public static void ReduceTechRewards()
        {
            //when difficulty bar hits max, reduce tech rewards

            //if (Config.RebalanceTech)
            for (int a = 0; a < Territories.Length; a++)
            //if (Territories[a].specialTag == TerritoryInstance.SpecialTag.None)
            {
                int value = Persist.Instance.TechRewards[a];
                int reduce = Math.Min(25, Plugin.Rand.Round(value * .75f));
                Plugin.Log.LogInfo($"techReward {value} -> {value - reduce}");
                Persist.Instance.TechRewards[a] = value - reduce;
            }

            LoadTechRewards();
            Persist.SaveCurrent();

            Plugin.Log.LogInfo($"reduced tech rewards for standard territories");
        }

        private static GraphInfo ModifyConnections()
        {
            GraphInfo graph = GetEdges(Territories);
            int numTerritories = Territories.Length;

            int numEdges = CountEdges(graph.Edges);
            //sever roughly half the extra edges, still ensuring a fully connected graph 
            float avg = (numEdges + numTerritories - 3f) / 2f;

            if (avg < numTerritories - 1)
            {
                Plugin.Log.LogError($"OriginalConnections did not restore properly ({numEdges} {avg} {numTerritories - 1})");
                return graph;
            }

            Plugin.Log.LogDebug($"ModifyConnections Gaussian({avg},.065,{numTerritories - 1})");
            int target = Plugin.Rand.GaussianCappedInt(avg, .065f, numTerritories - 1);
            Plugin.Log.LogInfo($"{numEdges} edges, target {target} (avg {avg:0.0})");

            int attempts = 0, maxAttempts = 6 * numEdges * (numEdges - target);
            while (numEdges > target && attempts < maxAttempts)
            {
                HashSet<int>[] critical = FindCritical(graph.Edges, numTerritories);
                Plugin.Log.LogDebug($"critical = {CountEdges(critical)}");
                if (CountEdges(critical) == numEdges)
                {
                    Plugin.Log.LogWarning($"all remaining edges are critical connections ({CountEdges(critical)},{numEdges})");
                    break;
                }

                int a, b;
                do
                {
                    a = Plugin.Rand.SelectValue(Enumerable.Range(0, numTerritories), c => graph.Edges[c].Count * graph.Edges[c].Count);
                    b = Plugin.Rand.SelectValue(graph.Edges[a]);
                } while (critical[a].Contains(b));

                graph.Edges[a].Remove(b);
                graph.Edges[b].Remove(a);
                if (Validate(graph: graph))
                {
                    Plugin.Log.LogInfo($"Remove {a} <-> {b}");
                    numEdges--;
                }
                else
                {
                    graph.Edges[a].Add(b);
                    graph.Edges[b].Add(a);
                }
                attempts++;
            }
            string log = $"Edge removal attempts {attempts} (max {maxAttempts})";
            Plugin.LogAtLevel(log, attempts >= maxAttempts);

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
                if (edges[node] is not null)
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

        private static bool Validate(int[] shuffle = null, GraphInfo graph = null, bool doRand = true)
        {
            //need to copy array because we might shuffle it
            var territories = Territories.ToArray();
            if (shuffle is not null)
            {
                Plugin.Log.LogInfo("Validate shuffle: " + shuffle.Select(o => o.ToString()).Aggregate((a, b) => a + "," + b));
                ApplyShuffle(territories, shuffle);
                graph = null;
            }
            graph ??= GetEdges(territories);

            int maxMissions = territories.Length - 1;
            int numMissions = MissionManagerAsset.GameDifficultyIndex switch
            {
                3 => 12,
                1 => 11,
                0 => 10,
                2 => 9,
                _ => throw new Exception()
            };
            numMissions = Math.Min(numMissions, maxMissions);

            //path to end must allow completion without seeing super soldiers
            List<TerritoryInstance> path = TBSUtil.PathFind(Plugin.Rand, graph.Start, graph.End, GetNeighbors, (a, b) => 1);
            int pathLength = path.Count - 1;
            if (pathLength > numMissions || (doRand && pathLength < Plugin.Rand.GaussianInt(numMissions / 2.1, .13)))
            {
                Plugin.Log.LogWarning($"({CountEdges(graph.Edges)}) pathLength {pathLength} ({numMissions}), invalid");
                return false;
            }

            //ensure you can reach entire island
            HashSet<TerritoryInstance> reachable = new();
            DFS(graph.Start, t => t != graph.End);
            if (reachable.Count != maxMissions)
            {
                Plugin.Log.LogWarning($"({CountEdges(graph.Edges)}) reachable.Count {reachable.Count} != {maxMissions}, invalid");
                return false;
            }

            //ensure HighReward territories don't block off too much
            reachable.Clear();
            DFS(graph.Start, t => t.specialTag == TerritoryInstance.SpecialTag.None);
            if (reachable.Count < numMissions)
            {
                Plugin.Log.LogWarning($"({CountEdges(graph.Edges)}) reachable.Count {reachable.Count} < {numMissions}, invalid");
                return false;
            }

            // --- ensure this is the last validation step we run --- 
            //validation that only applies if a save file is from an old version or corrupted
            //(or a bug prevents load) and we reshuffle an existing game
            foreach (TerritoryInstance territory in territories)
                if (territory.specialTag == TerritoryInstance.SpecialTag.EnemyObjective)
                    SetTeam(territory, UnitTeam.Team2);
                else if (territory.specialTag == TerritoryInstance.SpecialTag.PlayerBase || !HasEnemies(territory))
                    SetTeam(territory, UnitTeam.Team1);
            reachable.Clear();
            DFS(graph.Start, t => t.owner == UnitTeam.Team1);
            reachable.Add(graph.Start);
            foreach (TerritoryInstance territory in territories)
                if (territory.owner == UnitTeam.Team1 && !reachable.Contains(territory))
                {
                    Plugin.Log.LogWarning($"non-contiguous Team1 territories, invalid");
                    return false;
                }

            return true;

            //reachable territories counting end but not start
            void DFS(TerritoryInstance parent, Predicate<TerritoryInstance> CanPass)
            {
                foreach (var child in GetNeighbors(parent).Select(t => t.Item1))
                    if (child != graph.Start && !reachable.Contains(child))
                    {
                        reachable.Add(child);
                        if (CanPass(child))
                            DFS(child, CanPass);
                    }
            }
            IEnumerable<Tuple<TerritoryInstance, int>> GetNeighbors(TerritoryInstance t) =>
              graph.Edges[Idx(t)].Select(e => new Tuple<TerritoryInstance, int>(territories[e], 1));
            //can't use t.index because territories will be out of order at this point
            int Idx(TerritoryInstance t) => Array.IndexOf(territories, t);
            static bool HasEnemies(TerritoryInstance territory)
            {
                SpawnWaveProfile spawnWaveProfile = territory?.operation?.spawnWaveProfile;
                return (spawnWaveProfile is not null && Operations.GetSpawnTechs(spawnWaveProfile).Any());
            }
            void SetTeam(TerritoryInstance territory, UnitTeam team)
            {
                if (territory.owner != team)
                {
                    territory.owner = team;
                    SpawnWaveProfile spawnWaveProfile = territory.operation.spawnWaveProfile;
                    Plugin.Log.LogWarning($"setting {Idx(territory)} to {team} " +
                        $"({spawnWaveProfile.name} - {territory.index},{spawnWaveProfile.GetInstanceID()})");
                }
            }
        }

        private static void GenerateConnections(GraphInfo graph)
        {
            List<TerritoryConnection> connections = WorldMapAsset.TerritoryConnections;
            int count = connections.Count;
            connections.Clear();
            for (int c = 0; c < count; c++)
            {
                TerritoryConnection connection = new()
                {
                    //maintain 1-based indexing 
                    connection = graph.Edges[c].Select(d => d + 1).ToList()
                };
                connections.Add(connection);

                Plugin.Log.LogDebug($"GenerateConnections {c} -> ({connection.connection.Select(o => o.ToString()).Aggregate((x, y) => x + "," + y)},)");
            }

            Plugin.Log.LogInfo($"Generated {CountEdges(graph.Edges)} connections");
        }

        private static void SetOriginalConnections(bool doShuffle)
        {
            if (MissionManagerAsset is not null && WorldMapAsset is not null && Territories is not null)
            {
                var original = CloneConnections(Persist.Instance.OriginalConnections[MissionManagerAsset.WorldMapIndex]);
                if (original is not null && original.Count != Territories.Length)
                {
                    Plugin.Log.LogError($"Invalid original connections ({original.Count}, {Territories.Length})");
                    original = null;
                }

                if (doShuffle)
                    if (original is not null)
                    {
                        //restore the original connections so we can re-sever some
                        _territoryConnections.SetValue(WorldMapAsset, original);
                        Plugin.Log.LogInfo("restored connections " + original.Sum(c => c.connection.Count) / 2);
                    }
                    else
                    {
                        Plugin.Log.LogWarning($"no original connections ({WorldMapAsset}, {original})");
                    }

                if (original is null)
                {
                    var current = CloneConnections(WorldMapAsset.TerritoryConnections);
                    if (current.Count == Territories.Length)
                    {
                        //we need to store off the original connections so we can restore them when starting a new randomized island
                        Persist.Instance.OriginalConnections[MissionManagerAsset.WorldMapIndex] = current;
                        Plugin.Log.LogInfo($"SetOriginalConnections {current.Sum(c => c.connection.Count) / 2}");
                        if (doShuffle)
                            Persist.SaveCurrent();
                    }
                    else
                    {
                        Plugin.Log.LogError($"Invalid current connections ({current.Count}, {Territories.Length})");
                    }
                }
            }
            else
            {
                Plugin.Log.LogInfo($"Skipping SetOriginalConnections ({MissionManagerAsset}, {WorldMapAsset}, {Territories})");
            }
        }

        private static List<TerritoryConnection> CloneConnections(List<TerritoryConnection> connections) =>
            connections?.Select(c => new TerritoryConnection()
            {
                connection = c.connection.ToList()
            }).ToList();

        private static GraphInfo GetEdges(TerritoryInstance[] territories)
        {
            var connections = WorldMapAsset.TerritoryConnections;
            int count = connections.Count;
            GraphInfo graph = new() { Edges = InitEdgeArray(count) };

            if (count != territories.Length)
            {
                Plugin.Log.LogError($"Invalid connections ({count}, {territories.Length})");
                LogShuffle(territories);
            }

            for (int a = 0; a < count; a++)
            {
                TerritoryInstance territory = territories[a];
                if (territory.specialTag == TerritoryInstance.SpecialTag.PlayerBase)
                    graph.Start = territory;
                else if (territory.specialTag == TerritoryInstance.SpecialTag.EnemyObjective)
                    graph.End = territory;
                else if (territory.specialTag == TerritoryInstance.SpecialTag.HighValueReward)
                    graph.Rewards.Add(territory);

                foreach (int connection in connections[a].connection)
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

            if (graph.Start is null || graph.End is null)
            {
                Plugin.Log.LogError($"Invalid shuffle ({Array.IndexOf(territories, graph.Start)}, {Array.IndexOf(territories, graph.End)})");
                LogShuffle(territories);
            }

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
