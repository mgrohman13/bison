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
        private static float value = float.NaN;

        public static void Prefix(ref FloatDynamicStat ___worldMapDifficultyMultipler)
        {
            try
            {
                Plugin.Log.LogDebug("MissionManagerAsset_AdvanceWorldMapDifficulty Prefix");

                value = ___worldMapDifficultyMultipler.Value;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
        public static void Postfix(ref FloatDynamicStat ___worldMapDifficultyMultipler, ref FloatDynamicStat ___worldMapDifficultyMultiplerTick)
        {
            try
            {
                Plugin.Log.LogDebug("MissionManagerAsset_AdvanceWorldMapDifficulty Postfix");

                Plugin.Log.LogInfo($"after {___worldMapDifficultyMultipler.Value}, diff {___worldMapDifficultyMultipler.Value - value}, inc {___worldMapDifficultyMultiplerTick.Value}");
                value = float.NaN;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
