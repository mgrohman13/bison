using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using GameUI;
using System.Runtime.CompilerServices;
using LevelGeneration.WorldMap;
using LevelGeneration;
using WarpipsReplayability.Mod;
using UnityEngine;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(MissionManagerAsset))]
    [HarmonyPatch("AdvanceWorldMapDifficulty")]
    internal class MissionManagerAsset_AdvanceWorldMapDifficulty
    {
        public static void Prefix(ref FloatDynamicStat ___worldMapDifficultyMultipler, ref FloatDynamicStat ___worldMapDifficultyMultiplerTick)
        {
            try
            {
                Plugin.Log.LogInfo("MissionManagerAsset_AdvanceWorldMapDifficulty Prefix");

                Plugin.Log.LogInfo($"before {___worldMapDifficultyMultipler}, inc {___worldMapDifficultyMultiplerTick}");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
