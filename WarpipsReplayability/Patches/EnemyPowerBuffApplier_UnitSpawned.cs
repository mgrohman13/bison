using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(EnemyPowerBuffApplier))]
    [HarmonyPatch("UnitSpawned")]
    internal class EnemyPowerBuffApplier_UnitSpawned
    {
        public static void Prefix(EnemyPowerBuffApplier __instance, List<int> ___excludeOnDifficultyIndex)
        {
            try
            {
                Plugin.Log.LogDebug("EnemyPowerBuffApplier_UnitSpawned Prefix " + __instance.name);

                if (__instance.name == "SuperEnemyBuff" && ___excludeOnDifficultyIndex.Count > 0)
                {
                    Plugin.Log.LogInfo("Allowing SuperEnemyBuff: " + ___excludeOnDifficultyIndex.Select(d => d.ToString()).Aggregate("", (a, b) => a + " " + b));
                    ___excludeOnDifficultyIndex.Clear();
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
