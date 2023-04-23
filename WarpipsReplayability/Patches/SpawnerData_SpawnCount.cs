using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using GameUI;
using System.Runtime.CompilerServices;
using WarpipsReplayability.Mod;
using DynamicEnums;
using UnityEngine;
using static LevelGeneration.WorldMap.TerritoryInstance;

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
