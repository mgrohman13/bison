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
using SpecialTag = LevelGeneration.WorldMap.TerritoryInstance.SpecialTag;

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
