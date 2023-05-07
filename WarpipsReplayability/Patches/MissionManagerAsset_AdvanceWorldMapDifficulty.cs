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
        public static bool Prefix(MissionManagerAsset __instance, FloatDynamicStat ___worldMapDifficultyMultipler)
        {
            try
            {
                Plugin.Log.LogDebug("MissionManagerAsset_AdvanceWorldMapDifficulty Prefix");

                float inc = Map.MissionManagerAsset.GameDifficultyIndex switch
                {
                    3 => 0.085f, //*11=.935
                    1 => 0.095f, //*10=.95
                    0 => 0.105f, //* 9=.945
                    2 => 0.115f, //* 8=.92
                    _ => throw new Exception($"Map.MissionManagerAsset.GameDifficultyIndex {Map.MissionManagerAsset.GameDifficultyIndex}")
                };
                if (__instance.WorldMapIndex == 3)
                {
                    inc /= 2;
                    Plugin.Log.LogInfo($"slowing worldMapDifficultyMultipler increase");
                }

                float previous = ___worldMapDifficultyMultipler.Value;
                float next = Mathf.Clamp01(previous + inc);

                if (previous < 1 && next >= 1)
                    Map.ReduceTechRewards();
                ___worldMapDifficultyMultipler.Value = next;

                Plugin.Log.LogInfo($"worldMapDifficultyMultipler {next} ({previous}+{inc})");

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
