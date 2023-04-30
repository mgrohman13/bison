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

        public static bool Prefix(SpawnWaveProfile __instance, ref List<float> __result)
        {
            try
            {
                Plugin.Log.LogDebug("SpawnWaveProfile_ReturnAllWarningMessageTimings Prefix");

                __result = new();

                IEnumerable<EnemySpawnProfile> profiles = __instance.enemySpawnProfiles.Cast<EnemySpawnProfile>();
                float warningDifficulty = profiles.Max(p => p.UnitSpawnData.StartAtDifficulty);
                float displayThreshold = DifficultyBar_BuildDifficultyBar.DisplayThreshold;
                if (warningDifficulty > displayThreshold)
                {
                    uint[] seed = profiles.Select(p => p.UnitSpawnData).SelectMany(d =>
                        new object[] { d.CooldownAfterSpawn, d.SpawnCapCycleMultipler, d.StartAtDifficulty, d.TimeBetweenClusters })
                        .Select(o => (uint)o.GetHashCode()).ToArray();
                    Plugin.Log.LogInfo("alert timings seed:" + seed.Select(s => s.ToString("X")).Aggregate(" ", (a, b) => a + b));
                    MTRandom temp = new(seed);

                    AnimationCurve difficultyCurve = (AnimationCurve)field_difficultyCurve.GetValue(__instance);
                    bool prev = false;
                    float prevTime = 0;
                    foreach (var k in difficultyCurve.keys)
                    {
                        bool cur = (k.value > warningDifficulty);
                        if (!prev && cur)
                        {
                            float min = prevTime, max = k.time, result;
                            do
                            {
                                result = Mathf.Lerp(min, max, temp.FloatHalf());
                                float eval = difficultyCurve.Evaluate(result);
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
    }
}
