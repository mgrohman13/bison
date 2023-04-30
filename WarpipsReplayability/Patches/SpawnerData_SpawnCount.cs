using DynamicEnums;
using HarmonyLib;
using System;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(SpawnerData))]
    [HarmonyPatch(nameof(SpawnerData.SpawnCount))]
    internal class SpawnerData_SpawnCount
    {
        public static bool Prefix(InvokeableType ___spawnTech, int ___spawnCountMin, int ___spawnCountMax, float t, ref int __result)
        {
            try
            {
                Plugin.Log.LogDebug("SpawnerData_SpawnCount Prefix");
                return Spawns.ModifySpawns(false, ___spawnTech, ___spawnCountMin, ___spawnCountMax, t, ref __result);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
                return true;
            }
        }
    }
}
