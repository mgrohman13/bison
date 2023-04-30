using HarmonyLib;
using LevelGeneration;
using System;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(MissionManagerAsset))]
    [HarmonyPatch(nameof(MissionManagerAsset.InitializeWorldMap))]
    internal class MissionManagerAsset_InitializeWorldMap
    {
        public static void Prefix(MissionManagerAsset __instance, ref int worldMapIndex)
        {
            try
            {
                Plugin.Log.LogDebug("MissionManagerAsset_InitializeWorldMap Prefix");

                Map.MissionManagerAsset = __instance;

                int? forceWorldMapIndex = Map.ForceWorldMapIndex;
                if (forceWorldMapIndex.HasValue && worldMapIndex != forceWorldMapIndex.Value)
                    worldMapIndex = forceWorldMapIndex.Value;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
