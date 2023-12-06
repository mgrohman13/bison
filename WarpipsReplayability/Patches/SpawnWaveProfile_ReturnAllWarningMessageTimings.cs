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
                                Plugin.Log.LogDebug($"skipping alert at 1 {prevTime:0.000}-{key.time:0.000} (1= {curve.Evaluate(1):0.000})");
                            }
                            else
                            {
                                bool loop = true;
                                float min = prevTime, max = Math.Min(1, key.time), result, mid;
                                do
                                {
                                    result = deterministic.Range(min, max);
                                    float eval = curve.Evaluate(result);
                                    Plugin.Log.LogDebug($"{min} {max} {result} {eval}");
                                    if (eval > warningDifficulty)
                                        max = result;
                                    else if (eval < displayThreshold)
                                        min = result;
                                    else
                                        break;

                                    mid = (min + max) / 2f;
                                    loop = (mid > min && mid < max);
                                    Plugin.Log.LogDebug($"{mid} {loop}");
                                } while (loop);
                                if (!loop)
                                {
                                    result = deterministic.Bool() ? min : max;
                                    Plugin.Log.LogWarning($"setting alert to {result}, {min} ({curve.Evaluate(min)}) - {max} ({curve.Evaluate(max)})");
                                }

                                Plugin.Log.LogDebug($"alert {prevTime:0.000}-{key.time:0.000}: {result:0.000}");
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
        private static float GetDisplayThreshold(MTRandom deterministic, float warningDifficulty)
        {
            float displayThreshold = DifficultyBar_BuildDifficultyBar.DisplayThreshold;
            if (warningDifficulty < displayThreshold)
            {
                float rand = deterministic.GaussianCapped(warningDifficulty * .75f, .13f, warningDifficulty * .5f);
                Plugin.Log.LogWarning($"difficulty < displayThreshold ({warningDifficulty} < {displayThreshold}): set to {rand}");
                displayThreshold = rand;
            }
            return displayThreshold;
        }

        public static uint[] GenerateSeed(SpawnWaveProfile waveProfile) =>
            GenerateSeed((AnimationCurve)_difficultyCurve.GetValue(waveProfile));
        private static uint[] GenerateSeed(AnimationCurve curve)
        {
            //TODO: strings?
            //generate a simple seed based on the curve
            uint[] seed = MTRandom.GenerateSeed(curve.keys.SelectMany(k =>
                    new object[] { k.value, k.time, }));
            Plugin.Log.LogInfo("AnimationCurve seed: " + Plugin.GetSeedString(seed));
            return seed;
        }
    }
}
