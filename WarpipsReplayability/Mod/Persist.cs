using HarmonyLib;
using LevelGeneration.WorldMap;
using LevelGeneration;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LevelGeneration.WorldMap.WorldMapAsset;

namespace WarpipsReplayability.Mod
{
    [Serializable]
    internal class Persist
    {
        public static Persist Instance;
        private const string saveFile = "BepInEx/plugins/WarpipsReplayability.dat";

        public int[] Shuffle { get; private set; }
        public List<TerritoryConnection> Connections { get; private set; }
        public bool[] HiddenRewards { get; private set; }
        public int[] TechRewards { get; private set; }

        public static void SaveNew(int[] shuffle)
        {
            Instance = new()
            {
                Shuffle = shuffle,
                Connections = Map.WorldMapAsset.TerritoryConnections,
                HiddenRewards = Operations.RollHiddenRewards(),
                TechRewards = Map.Territories.Select(t => t.operation.techReward).ToArray(),
            };
            SaveCurrent();
            Plugin.Log.LogInfo($"saved mod data");
        }
        public static void SaveCurrent()
        {
            TBSUtil.SaveGame(Instance, saveFile);
        }
        public static void Load()
        {
            Instance = TBSUtil.LoadGame<Persist>(saveFile);
            AccessTools.Field(typeof(WorldMapAsset), "territoryConnections").SetValue(Map.WorldMapAsset, Instance.Connections);
            Plugin.Log.LogInfo($"loaded mod data");
        }
    }
}
