using HarmonyLib;
using LevelGeneration;
using System;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(MissionManagerAsset))]
    [HarmonyPatch(nameof(MissionManagerAsset.FailedMission))]
    internal class MissionManagerAsset_FailedMission
    {
        public static void Prefix(IntDynamicStat ___gameDifficultyIndex, ref int __state)
        {
            try
            {
                Plugin.Log.LogDebug("MissionManagerAsset_FailedMission Prefix");

                //ensure ConsumeItemsOnHotbar is always called
                __state = ___gameDifficultyIndex.Value;
                ___gameDifficultyIndex.Value = 2;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
        public static void Postfix(MissionManagerAsset __instance, IntDynamicStat ___gameDifficultyIndex, int __state)
        {
            try
            {
                Plugin.Log.LogDebug("MissionManagerAsset_FailedMission Postfix");

                ___gameDifficultyIndex.Value = __state;
                Operations.FailedMission = Map.Territories[__instance.CurrentWorldMap.lastAttackedTerritory];
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
