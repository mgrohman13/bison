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
        private const string saveFile = "BepInEx/plugins/WarpipsReplayability.save";

        private int[] techRewards;
        private List<TerritoryConnection> territoryConnections;

        public static void SaveData()
        {
            Persist save = new()
            {
                techRewards = GameRandom.Territories.Select(t => t.operation.techReward).ToArray(),
                territoryConnections = Map.WorldMapAsset.TerritoryConnections,
            };
            TBSUtil.SaveGame(save, saveFile);
        }
        public static void LoadData()
        {
            Persist save = TBSUtil.LoadGame<Persist>(saveFile);

            int a = 0;
            foreach (var territory in GameRandom.Territories)
                territory.operation.techReward = save.techRewards[a++];

            AccessTools.Field(typeof(WorldMapAsset), "territoryConnections").SetValue(Map.WorldMapAsset, save.territoryConnections);
        }
    }
}
