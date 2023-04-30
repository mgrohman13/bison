using HarmonyLib;
using LevelGeneration;
using System;
using UnityEngine;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(SpawnWaveProfile))]
    [HarmonyPatch(nameof(SpawnWaveProfile.ReturnDifficultyAtTime))]
    internal class SpawnWaveProfile_ReturnDifficultyAtTime
    {
        public static void Postfix(ref float __result)
        {
            try
            {
                Plugin.Log.LogDebug("SpawnWaveProfile_ReturnDifficultyAtTime Postfix");

                __result = Mathf.Clamp01(__result);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
