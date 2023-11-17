using HarmonyLib;
using LevelGeneration.WorldMap;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static LevelGeneration.WorldMap.WorldMapAsset;

namespace WarpipsReplayability.Mod
{
    [Serializable]
    internal class Persist
    {
        private const string saveFile = "BepInEx/plugins/WarpipsReplayability.dat";
        private static readonly FieldInfo _territoryConnections = AccessTools.Field(typeof(WorldMapAsset), "territoryConnections");

        public static Persist Instance { get; private set; }

        static Persist()
        {
            Instance = new();
        }

        public readonly int[] Shuffle;
        public readonly List<TerritoryConnection> Connections;
        public readonly bool[] HiddenRewards;
        public readonly int[] TechRewards;
        public readonly Operations.OperationInfo[] OperationInfo;

        public readonly List<TerritoryConnection>[] OriginalConnections = new List<TerritoryConnection>[4];

        public int SaleIndex { get; set; } = -1;

        private Persist()
        {
        }
        private Persist(int[] shuffle, Operations.OperationInfo[] operationInfo)
        {
            Shuffle = shuffle;
            OperationInfo = operationInfo;
            Connections = Map.WorldMapAsset.TerritoryConnections;
            HiddenRewards = Operations.RollHiddenRewards();
            TechRewards = Map.Territories.Select(t => t.operation.techReward).ToArray();

            //OriginalConnections are never modified, so always maintain the same list
            OriginalConnections = Instance.OriginalConnections;
        }

        public static void SaveNew(int[] shuffle, Operations.OperationInfo[] operationInfo)
        {
            Instance = new(shuffle, operationInfo);
            SaveCurrent();
            Plugin.Log.LogInfo($"Persist saved new mod data");
        }
        public static void SaveCurrent()
        {
            Plugin.Log.LogInfo("Save Persist");
            TBSUtil.SaveGame(Instance, saveFile);
        }
        public static void Load()
        {
            Plugin.Log.LogInfo($"Load Persist");
            Instance = TBSUtil.LoadGame<Persist>(saveFile);
            _territoryConnections.SetValue(Map.WorldMapAsset, Instance.Connections);
        }
    }
}
