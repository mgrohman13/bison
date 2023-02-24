using LevelGeneration;
using LevelGeneration.WorldMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarpipsReplayability
{
    internal class Map
    {
        public static MissionManagerAsset MissionManagerAsset { get; set; }

        public static void Shuffle(WorldMapAsset worldMapAsset, TerritoryInstance[] territories)
        {
            Dictionary<int, int> shuffle = ShuffleTerritories(territories);
            FixConnections(worldMapAsset);//, null);// shuffle);
        }

        private static Dictionary<int, int> ShuffleTerritories(TerritoryInstance[] territories)
        {
            Dictionary<int, int> shuffle = new Dictionary<int, int>();

            Plugin.Rand.Shuffle(territories);

            for (int i = 0; i < territories.Length; i++)
            {
                shuffle[territories[i].index] = i;

                territories[i].index = i;
                //Plugin.Log.LogInfo(i);
                //Plugin.Log.LogInfo(territories[i].centerPos);
            }

            Plugin.Log.LogInfo("Shuffled territories");
            return shuffle;
        }

        private static void FixConnections(WorldMapAsset worldMapAsset)//, Dictionary<int, int> shuffle)
        {
            int count = worldMapAsset.TerritoryConnections.Count;

            Dictionary<int, HashSet<int>> edges = new Dictionary<int, HashSet<int>>();
            for (int a = 0; a < count; a++)
            {
                int t1 = a;
                foreach (int connection in worldMapAsset.TerritoryConnections[a].connection)
                {
                    int b = connection - 1;
                    if (MissionManagerAsset.WorldMapIndex == 0 && a == 3 && b == 1)
                    {
                        Plugin.Log.LogInfo($"connection bugfix {a} -/> {b}");
                        continue;
                    }

                    int t2 = b;
                    AddEdge(edges, t1, t2);
                    AddEdge(edges, t2, t1);
                }
            }

            worldMapAsset.TerritoryConnections.Clear();
            for (int c = 0; c < count; c++)
            {
                WorldMapAsset.TerritoryConnection connection = new WorldMapAsset.TerritoryConnection();
                connection.connection = edges[c].Select(d => d + 1).ToList();
                worldMapAsset.TerritoryConnections.Add(connection);
            }

            Plugin.Log.LogInfo("Cleaned up connections");
        }

        private static void AddEdge(Dictionary<int, HashSet<int>> edges, int t1, int t2)
        {
            if (!edges.ContainsKey(t1))
                edges[t1] = new HashSet<int>();
            edges[t1].Add(t2);
        }
    }
}
