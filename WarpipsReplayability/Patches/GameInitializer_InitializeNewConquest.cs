using HarmonyLib;
using LevelGeneration;
using System;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(GameInitializer))]
    [HarmonyPatch(nameof(GameInitializer.InitializeNewConquest))]
    internal class GameInitializer_InitializeNewConquest
    {
        public static void Prefix(MissionManagerAsset ___missionManagerAsset)
        {
            try
            {
                Plugin.Log.LogDebug("GameInitializer_InitializeNewConquest Postfix");
                Map.MissionManagerAsset = ___missionManagerAsset;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
