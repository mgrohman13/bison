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
        private static readonly FieldInfo field_difficultyCurve = AccessTools.Field(typeof(SpawnWaveProfile), "difficultyCurve");

        //display an alert for the lowest-spawning unit in the first listed group that has a spawn
        private static readonly HashSet<string>[] groups = new HashSet<string>[] {
            new string[] { "Hind", "Rocket", }.ToHashSet(),
            new string[] { "Bubba", "Predator", "T92", }.ToHashSet(),
            new string[] { "Tanya", "DuneBuggy", "Gruz", }.ToHashSet(),
            new string[] { "GasPip", "Sharpshooter", "RPGSoldier", }.ToHashSet(),
        };

        public static bool Prefix(SpawnWaveProfile __instance, ref List<float> __result)
        {
            try
            {
                Plugin.Log.LogDebug("SpawnWaveProfile_ReturnAllWarningMessageTimings Prefix");

                __result = new();
                IEnumerable<EnemySpawnProfile> profiles = __instance.enemySpawnProfiles.Cast<EnemySpawnProfile>();

                float? startAtDifficulty = GetStartAtDifficulty(profiles);
                if (startAtDifficulty.HasValue)
                {
                    uint[] seed = profiles.Select(p => p.UnitSpawnData).SelectMany(d =>
                        new object[] { d.CooldownAfterSpawn, d.SpawnCapCycleMultipler, d.StartAtDifficulty, d.TimeBetweenClusters })
                        .Select(o => (uint)o.GetHashCode()).ToArray();
                    Plugin.Log.LogInfo("alert timings seed:" + Plugin.GetSeedString(seed));
                    MTRandom temp = new(seed);

                    AnimationCurve curve = (AnimationCurve)field_difficultyCurve.GetValue(__instance);
                    float warningDifficulty = startAtDifficulty.Value;
                    float displayThreshold = GetDisplayThreshold(temp, warningDifficulty);

                    bool prev = false;
                    float prevTime = 0;
                    foreach (var k in curve.keys)
                    {
                        bool cur = curve.Evaluate(k.time) > warningDifficulty;
                        if (!prev && cur)
                        {
                            float min = prevTime, max = Math.Min(1, k.time), result;
                            do
                            {
                                result = Mathf.Lerp(min, max, temp.FloatHalf());
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
                            Plugin.Log.LogInfo($"alert {prevTime:0.000}-{k.time:0.000}: {result:0.000}");
                            __result.Add(result);
                        }
                        prev = cur;
                        prevTime = k.time;
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
            foreach (var group in groups)
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
                displayThreshold = temp.GaussianCapped(warningDifficulty * .75f, .13f, warningDifficulty * .5f);
            return displayThreshold;
        }
    }
}
