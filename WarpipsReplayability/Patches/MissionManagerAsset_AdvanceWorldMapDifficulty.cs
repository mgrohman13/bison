using HarmonyLib;
using LevelGeneration;
using System;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(MissionManagerAsset))]
    [HarmonyPatch("AdvanceWorldMapDifficulty")]
    internal class MissionManagerAsset_AdvanceWorldMapDifficulty
    {
        public static void Prefix(FloatDynamicStat ___worldMapDifficultyMultipler, FloatDynamicStat ___worldMapDifficultyMultiplerTick, ref float __state)
        {
            try
            {
                Plugin.Log.LogDebug("MissionManagerAsset_AdvanceWorldMapDifficulty Prefix");

                //I have no explanation for why ___worldMapDifficultyMultiplerTick shows nonsensical values
                Plugin.Log.LogDebug($"Prefix worldMapDifficultyMultiplerTick {___worldMapDifficultyMultiplerTick.Value} (???)");

                __state = ___worldMapDifficultyMultipler.Value;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
        public static void Postfix(MissionManagerAsset __instance, FloatDynamicStat ___worldMapDifficultyMultipler, FloatDynamicStat ___worldMapDifficultyMultiplerTick, float __state)
        {
            try
            {
                Plugin.Log.LogDebug("MissionManagerAsset_AdvanceWorldMapDifficulty Postfix");

                //I have no explanation for why ___worldMapDifficultyMultiplerTick shows nonsensical values
                Plugin.Log.LogDebug($"Postfix worldMapDifficultyMultiplerTick {___worldMapDifficultyMultiplerTick.Value} (???)");

                float value, inc;
                Calc();
                void Calc()
                {
                    value = ___worldMapDifficultyMultipler.Value;
                    inc = value - __state;
                };

                if (inc > .06875 && (__instance.WorldMapIndex == 3))// || __instance.WorldMapIndex == 0))
                {
                    ___worldMapDifficultyMultipler.Value = value - inc / 2f;
                    Calc();
                    Plugin.Log.LogInfo($"slowing worldMapDifficultyMultipler increase");
                }

                Plugin.Log.LogInfo($"worldMapDifficultyMultipler {value} (+{inc})");

                if (__state < 1 && value >= 1)
                    Map.ReduceTechRewards();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
