using DynamicEnums;
using HarmonyLib;
using System;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(SpawnerData))]
    [HarmonyPatch(nameof(SpawnerData.SpawnCap))]
    internal class SpawnerData_SpawnCap
    {
        public static bool Prefix(InvokeableType ___spawnTech, int ___spawnCapMin, int ___spawnCapMax, float t, ref int __result)
        {
            try
            {
                Plugin.Log.LogDebug("SpawnerData_SpawnCap Prefix");
                return Spawns.ModifySpawns(true, ___spawnTech, ___spawnCapMin, ___spawnCapMax, t, ref __result);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
                return true;
            }
        }
    }
}
