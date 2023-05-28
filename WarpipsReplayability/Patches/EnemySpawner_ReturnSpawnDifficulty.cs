using HarmonyLib;
using LevelGeneration;
using System;
using UnityEngine;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(EnemySpawner))]
    [HarmonyPatch("ReturnSpawnDifficulty")]
    internal class EnemySpawner_ReturnSpawnDifficulty
    {
        private static string lastLog = "";

        public static bool Prefix(SpawnWaveProfile ___spawnWaveProfile, FloatDynamicStat ___roundTimeSinceStart,
            FloatDynamicStat ___worldMapDifficultyMultipler, FloatDynamicStat ___cycleDifficultyMultipler,
            ref float __result)
        {
            try
            {
                float wave = Mathf.Clamp01(___spawnWaveProfile.ReturnDifficultyAtTime(___roundTimeSinceStart.Value));
                float cycle = Mathf.Clamp01(___cycleDifficultyMultipler.Value);
                //float cycle = Mathf.Clamp01(Plugin.Rand.DoubleFull(___cycleDifficultyMultipler.Value));

                //for each call, sample a random value up to the total worldMapDifficultyMultipler
                //so that enemies will still (on average) come and go in waves with the difficultyCurve
                //but the worldMapDifficultyMultipler does increase the overall difficulty
                float world = Mathf.Clamp01(Plugin.Rand.DoubleHalf(___worldMapDifficultyMultipler.Value));

                //this combination algorithm makes much more sense to me rather than adding and clamping
                //otherwise some multipliers become completely irrelevant when others are high
                __result = 1 - ((1 - wave) * (1 - cycle) * (1 - world));

                string log = wave.ToString("0.0");
                if (lastLog != log)
                {
                    lastLog = log;
                    Plugin.Log.LogInfo($"difficulty: {__result:0.000} ({wave:0.00} x {cycle:0.00} x {world:0.00}" +
                        $", {___worldMapDifficultyMultipler.Value:0.00})");
                }

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
