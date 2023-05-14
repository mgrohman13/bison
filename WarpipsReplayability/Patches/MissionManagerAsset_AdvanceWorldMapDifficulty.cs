using HarmonyLib;
using LevelGeneration;
using System;
using UnityEngine;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(MissionManagerAsset))]
    [HarmonyPatch("AdvanceWorldMapDifficulty")]
    internal class MissionManagerAsset_AdvanceWorldMapDifficulty
    {
        public static bool Prefix(FloatDynamicStat ___worldMapDifficultyMultipler)
        {
            try
            {
                Plugin.Log.LogDebug("MissionManagerAsset_AdvanceWorldMapDifficulty Prefix");

                float inc = SuccessOutroDirector_AwakeExtension.WorldMapDifficultyMultiplerTick;

                float previous = ___worldMapDifficultyMultipler.Value;
                float next = Mathf.Clamp01(previous + inc);

                if (previous < 1 && next >= 1)
                    Map.ReduceTechRewards();
                ___worldMapDifficultyMultipler.Value = next;

                Plugin.Log.LogInfo($"worldMapDifficultyMultipler {next:0.000} ({previous:0.000}+{inc:0.000})");
                return false;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
                return true;
            }
        }
    }
}
