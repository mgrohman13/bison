using HarmonyLib;
using LevelGeneration;
using System;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(MissionManagerAsset))]
    [HarmonyPatch(nameof(MissionManagerAsset.NewGameConquest))]
    internal class MissionManagerAsset_NewGameConquest
    {
        public static void Postfix(MissionManagerAsset __instance)
        {
            try
            {
                Plugin.Log.LogDebug("MissionManagerAsset_NewGameConquest Postfix");

                if (Map.ForceWorldMapIndex.HasValue && __instance.WorldMapIndex != Map.ForceWorldMapIndex.Value)
                    typeof(MissionManagerAsset).GetProperty("WorldMapIndex").SetValue(__instance, Map.ForceWorldMapIndex.Value);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
