using HarmonyLib;
using System;
using System.Reflection;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(SuccessOutroDirector))]
    [HarmonyPatch("AwakeExtension")]
    internal class SuccessOutroDirector_AwakeExtension
    {
        private static readonly FieldInfo _difficultyStats = AccessTools.Field(typeof(FloatDifficultyModifiedValue), "difficultyStats");
        private static readonly FieldInfo _difficultyModifierIndex = AccessTools.Field(typeof(FloatDifficultyModifiedValue), "difficultyModifierIndex");

        public static float WorldMapDifficultyMultiplerTick { get; private set; }

        public static void Prefix(FloatDifficultyModifiedValue ___worldMapDifficultyMultiplerTick)
        {
            try
            {
                Plugin.Log.LogDebug("SuccessOutroDirector_AwakeExtension Prefix");
                int gameDifficultyIndex = Map.MissionManagerAsset.GameDifficultyIndex;
                float inc = gameDifficultyIndex switch
                {
                    3 => .085f, //*11=.935
                    1 => .095f, //*10=.95
                    0 => .105f, //* 9=.945
                    2 => .115f, //* 8=.92
                    _ => throw new Exception($"Map.MissionManagerAsset.GameDifficultyIndex {gameDifficultyIndex}")
                };

                //allow capturing final island territories with less difficulty increase to the final boss
                if (Map.MissionManagerAsset.WorldMapIndex == 3)
                {
                    inc /= 2;
                    Plugin.Log.LogInfo($"slowing worldMapDifficultyMultipler increase");
                }

                //the maximum amount we can randomize each difficulty increase without throwing off the number of territories before maxed
                float range = gameDifficultyIndex switch
                {
                    3 => 1f / 601,
                    1 => 1f / 245,
                    0 => 1f / 201,
                    2 => 1f / 258,
                    _ => throw new Exception()
                };
                inc += Plugin.Rand.Range(-range, range);

                FloatReference[] difficultyStats = (FloatReference[])_difficultyStats.GetValue(___worldMapDifficultyMultiplerTick);
                IntDynamicStat difficultyModifierIndex = (IntDynamicStat)_difficultyModifierIndex.GetValue(___worldMapDifficultyMultiplerTick);

                //ensure the end of mission progress bar matches the actual difficulty increase
                int difficulty = difficultyModifierIndex.Value;
                difficultyStats[difficulty].useConstant = true;
                difficultyStats[difficulty].constantValue = inc;

                WorldMapDifficultyMultiplerTick = inc;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
