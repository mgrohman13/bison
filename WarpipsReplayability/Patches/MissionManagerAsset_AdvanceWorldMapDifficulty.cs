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
        public static void Prefix(FloatDynamicStat ___worldMapDifficultyMultipler, ref float __state)
        {
            try
            {
                Plugin.Log.LogDebug("MissionManagerAsset_AdvanceWorldMapDifficulty Prefix");

                __state = ___worldMapDifficultyMultipler.Value;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
        public static void Postfix(FloatDynamicStat ___worldMapDifficultyMultipler, FloatDynamicStat ___worldMapDifficultyMultiplerTick, float __state)
        {
            try
            {
                Plugin.Log.LogDebug("MissionManagerAsset_AdvanceWorldMapDifficulty Postfix");

                float value = ___worldMapDifficultyMultipler.Value;
                Plugin.Log.LogInfo($"after {value}, diff {value - __state}, inc {___worldMapDifficultyMultiplerTick}");

                if (__state < 1 && value >= 1)
                    Persist.ReduceTechRewards();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
