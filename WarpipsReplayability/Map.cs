using LevelGeneration;
using LevelGeneration.WorldMap;
using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using GameIO;
using MonoMod.Utils;
using HarmonyLib;

namespace WarpipsReplayability
{
    internal class Map
    {
        //no reference from WorldMapAsset to MissionManagerAsset so this gets set externally
        public static MissionManagerAsset MissionManagerAsset { get; set; }
        public static WorldMapAsset WorldMapAsset { get; set; }
        public static TerritoryInstance[] Territories { get; set; }
        public static bool DoShuffle { get; internal set; }

        private static TerritoryInstance Start { get; set; }
        private static TerritoryInstance End { get; set; }
        private static HashSet<TerritoryInstance> Rewards { get; set; }
        private static Dictionary<int, int> Shuffle { get; set; }

        public static void Randomize()
        {
            if (DoShuffle)
            {
                //Shuffle = null;
                ShuffleTerritories();
                var edges = ModifyConnections();
                if (ValidateAndFinalize(edges))
                {
                    //must happen after due to recursive retries
                    DoShuffle = false;

                    TBSUtil.SaveGame(Shuffle, "BepInEx/plugins/shuffle.dat");
                    Shuffle = null;
                }
            }
        }

        private static void ShuffleTerritories()
        {
            Operation[] operations = Territories.Select(t => t.operation).ToArray();
            Plugin.Log.LogInfo(operations.Select(o => o.spawnWaveProfile.name).Aggregate((a, b) => a + "," + b));
            Plugin.Log.LogInfo(operations.Select(o => o.spawnWaveProfile.GetInstanceID().ToString()).Aggregate((a, b) => a + "," + b));

            if (Shuffle == null)
                Shuffle = Enumerable.Range(0, Territories.Length).ToDictionary(a => a);
            Plugin.Rand.Shuffle(Territories);
            for (int c = 0; c < Territories.Length; c++)
            {
                Shuffle[c] = Shuffle[Territories[c].index];
                Territories[c].operation = operations[Territories[c].index];
                Territories[c].index = c;

                if (Territories[c].operation.techReward > 1)
                    Territories[c].operation.techReward = Plugin.Rand.GaussianCappedInt(Territories[c].operation.techReward - 1, .13, 1);
            }

            operations = Territories.Select(t => t.operation).ToArray();
            Plugin.Log.LogInfo(operations.Select(o => o.spawnWaveProfile.name).Aggregate((a, b) => a + "," + b));
            Plugin.Log.LogInfo(operations.Select(o => o.spawnWaveProfile.GetInstanceID().ToString()).Aggregate((a, b) => a + "," + b));


            Plugin.Log.LogInfo("Shuffled territories");
        }
        public static void LoadShuffle()
        {
            Dictionary<int, int> shuffle = TBSUtil.LoadGame<Dictionary<int, int>>("BepInEx/plugins/shuffle.dat");
            Operation[] operations = Territories.Select(t => t.operation).ToArray();
            TerritoryData[] data = operations.Select(o => NewTerritoryData(o)).ToArray();
            Plugin.Log.LogInfo(operations.Select(o => o.spawnWaveProfile.name).Aggregate((a, b) => a + "," + b));
            Plugin.Log.LogInfo(operations.Select(o => o.spawnWaveProfile.GetInstanceID().ToString()).Aggregate((a, b) => a + "," + b));
            for (int c = 0; c < Territories.Length; c++)
            {
                TerritoryData from = data[shuffle[c]];
                Operation to = Territories[c].operation;

                //AssignBaselineData sets the following fields based on the underlying territory assets
                //we must restore it instead from the shuffled values
                to.operationTargetHealth = from.operationTargetHealth;
                to.revealEnemyIcons = from.revealEnemyLineup;
                to.showBattleIntro = from.showBattleIntro;
                if (from.operationRewardProfile)
                {
                    to.techReward = from.operationRewardProfile.TechReward;
                    to.tokenReward = from.operationRewardProfile.TokenReward;
                }
                if (from.spawnWaveProfile)
                {
                    to.spawnWaveProfile = from.spawnWaveProfile;
                }
            }

            operations = Territories.Select(t => t.operation).ToArray();
            Plugin.Log.LogInfo(operations.Select(o => o.spawnWaveProfile.enemySpawnProfiles.Length.ToString()).Aggregate((a, b) => a + "," + b));

            Plugin.Log.LogInfo("Restored shuffle for save");
        }

        private static TerritoryData NewTerritoryData(Operation operation)
        {
            TerritoryData data = new();
            data.operationTargetHealth = operation.operationTargetHealth;
            data.revealEnemyLineup = operation.revealEnemyIcons;
            data.showBattleIntro = operation.showBattleIntro;
            data.operationRewardProfile = new();
            AccessTools.Field(typeof(OperationRewardProfile), "techReward").SetValue(data.operationRewardProfile, operation.techReward);
            AccessTools.Field(typeof(OperationRewardProfile), "tokenReward").SetValue(data.operationRewardProfile, operation.tokenReward);
            data.spawnWaveProfile = operation.spawnWaveProfile;
            return data;
        }

        private static HashSet<int>[] ModifyConnections()
        {
            HashSet<int>[] edges = GetEdges();
            SeverEdges(edges, WorldMapAsset.TerritoryConnections.Count);
            return edges;
        }

        private static HashSet<int>[] GetEdges()
        {
            int count = WorldMapAsset.TerritoryConnections.Count;
            HashSet<int>[] edges = InitEdgeArray(count);
            Rewards = new();

            for (int a = 0; a < count; a++)
            {
                TerritoryInstance territory = Territories[a];
                if (territory.specialTag == TerritoryInstance.SpecialTag.PlayerBase)
                    Start = territory;
                else if (territory.specialTag == TerritoryInstance.SpecialTag.EnemyObjective)
                    End = territory;
                else if (territory.specialTag == TerritoryInstance.SpecialTag.HighValueReward)
                    Rewards.Add(territory);

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
            //Plugin.Log.LogInfo($"GaussianCappedInt {avg}, .065, {min}");
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
                    a = Plugin.Rand.SelectValue(Enumerable.Range(0, count), c => edges[c].Count * edges[c].Count);
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

        private static bool ValidateAndFinalize(HashSet<int>[] edges)
        {
            IEnumerable<Tuple<TerritoryInstance, int>> GetNeighbors(TerritoryInstance t) =>
                edges[t.index].Select(e => new Tuple<TerritoryInstance, int>(Territories[e], 1));
            int GetDistance(TerritoryInstance a, TerritoryInstance b) => 1;

            //path to end must be no longer than 10 to allow completion without seeing super soldiers
            int pathLength = TBSUtil.PathFind(Plugin.Rand, Start, End, GetNeighbors, GetDistance).Count - 1;
            if (pathLength > 10)
            {
                Plugin.Log.LogInfo($"pathLength {pathLength}, retrying");
                Randomize();
                return false;
            }

            //speical rewards should not be blocked by the end goal   
            if (Rewards.Any(reward => TBSUtil.PathFind(Plugin.Rand, Start, reward, GetNeighbors, GetDistance).Contains(End)))
            {
                Plugin.Log.LogInfo("end is blocking reward path, retrying");
                Randomize();
                return false;
            }

            //accept setup and finalize map
            GenerateConnections(edges);
            return true;
        }

        private static void GenerateConnections(HashSet<int>[] edges)
        {
            int count = WorldMapAsset.TerritoryConnections.Count;
            WorldMapAsset.TerritoryConnections.Clear();
            for (int c = 0; c < count; c++)
            {
                WorldMapAsset.TerritoryConnection connection = new()
                {
                    //maintain 1-based indexing 
                    connection = edges[c].Select(d => d + 1).ToList()
                };
                WorldMapAsset.TerritoryConnections.Add(connection);

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
