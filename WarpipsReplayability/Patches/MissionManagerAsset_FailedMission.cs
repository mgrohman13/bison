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

                __state = ___gameDifficultyIndex.Value;
                //ensure ConsumeItemsOnHotbar is always called
                ___gameDifficultyIndex.Value = 2;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
        public static void Postfix(IntDynamicStat ___gameDifficultyIndex, int __state)
        {
            try
            {
                Plugin.Log.LogDebug("MissionManagerAsset_FailedMission Postfix");

                ___gameDifficultyIndex.Value = __state;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
