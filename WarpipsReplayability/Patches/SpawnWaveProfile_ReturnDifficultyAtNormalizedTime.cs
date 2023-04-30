using HarmonyLib;
using LevelGeneration;
using System;
using UnityEngine;
using WarpipsReplayability.Mod;

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
