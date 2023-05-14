using HarmonyLib;
using LevelGeneration;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(SpawnWaveProfile))]
    [HarmonyPatch(nameof(SpawnWaveProfile.ReturnAllWarningMessageTimings))]
    internal class SpawnWaveProfile_ReturnAllWarningMessageTimings
    {
        private static readonly FieldInfo _difficultyCurve = AccessTools.Field(typeof(SpawnWaveProfile), "difficultyCurve");

        public static bool Prefix(SpawnWaveProfile __instance, ref List<float> __result)
        {
            try
            {
                Plugin.Log.LogDebug("SpawnWaveProfile_ReturnAllWarningMessageTimings Prefix");

                __result = new();
                IEnumerable<EnemySpawnProfile> profiles = __instance.enemySpawnProfiles.Cast<EnemySpawnProfile>();

                //display difficulty bar alerts for the lowest-spawning unit in the first group from Plugin.DifficultTechs that has a spawn
                float? startAtDifficulty = GetStartAtDifficulty(profiles);
                if (startAtDifficulty.HasValue)
                {
                    AnimationCurve curve = (AnimationCurve)_difficultyCurve.GetValue(__instance);
                    MTRandom deterministic = new(GenerateSeed(curve));

                    float warningDifficulty = startAtDifficulty.Value;
                    float displayThreshold = GetDisplayThreshold(deterministic, warningDifficulty);

                    bool prev = false;
                    float prevTime = 0;
                    foreach (var key in curve.keys)
                    {
                        bool cur = curve.Evaluate(key.time) > warningDifficulty;
                        if (!prev && cur)
                        {
                            if (key.time > 1 && curve.Evaluate(1) < warningDifficulty)
                            {
                                Plugin.Log.LogInfo($"skipping alert at 1 {prevTime:0.000}-{key.time:0.000} (1= {curve.Evaluate(1):0.000})");
                            }
                            else
                            {
                                float min = prevTime, max = Math.Min(1, key.time), result;
                                do
                                {
                                    result = Mathf.Lerp(min, max, deterministic.FloatHalf());
                                    float eval = curve.Evaluate(result);
                                    if (eval > warningDifficulty)
                                        max = result;
                                    else if (eval < displayThreshold)
                                        min = result;
                                    else
                                        break;
                                } while (max > min);
                                if (max <= min)
                                    Plugin.Log.LogWarning($"setting alert to {result}");
                                Plugin.Log.LogInfo($"alert {prevTime:0.000}-{key.time:0.000}: {result:0.000}");
                                __result.Add(result);
                            }
                        }
                        prev = cur;
                        prevTime = key.time;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
                return true;
            }
        }

        private static float? GetStartAtDifficulty(IEnumerable<EnemySpawnProfile> profiles)
        {
            foreach (var group in Plugin.DifficultTechs)
            {
                var spawns = profiles.Where(p => group.Contains(p.ReturnTechType().name));
                if (spawns.Any())
                    return spawns.Select(p => p.UnitSpawnData.StartAtDifficulty).Min();
            }
            return null;
        }
        private static float GetDisplayThreshold(MTRandom temp, float warningDifficulty)
        {
            float displayThreshold = DifficultyBar_BuildDifficultyBar.DisplayThreshold;
            if (warningDifficulty < displayThreshold)
            {
                float rand = temp.GaussianCapped(warningDifficulty * .75f, .13f, warningDifficulty * .5f);
                Plugin.Log.LogInfo($"difficulty < displayThreshold ({warningDifficulty} < {displayThreshold}): set to {rand}");
                displayThreshold = rand;
            }
            return displayThreshold;
        }

        private static uint[] GenerateSeed(AnimationCurve curve)
        {
            //generate a simple seed based on the curve
            uint[] seed = curve.keys.SelectMany(k =>
                    new float[] { k.value, k.time, })
                .Select(o => (uint)o.GetHashCode()).ToArray();
            Plugin.Log.LogInfo("alert timings seed: " + Plugin.GetSeedString(seed));
            return seed;
        }
    }
}
