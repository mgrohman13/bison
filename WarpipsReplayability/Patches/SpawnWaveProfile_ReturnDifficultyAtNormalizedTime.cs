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
using LevelGeneration;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(SpawnWaveProfile))]
    [HarmonyPatch(nameof(SpawnWaveProfile.ReturnDifficultyAtNormalizedTime))]
    internal class SpawnWaveProfile_ReturnDifficultyAtNormalizedTime
    {
        public static void Postfix(ref float __result)
        {
            try
            {
                Plugin.Log.LogDebug("SpawnWaveProfile_ReturnDifficultyAtNormalizedTime Postfix");

                __result = Mathf.Clamp01(__result);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
