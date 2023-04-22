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
    [HarmonyPatch(typeof(EnemySpawner))]
    [HarmonyPatch("ReturnSpawnDifficulty")]
    internal class EnemySpawner_ReturnSpawnDifficulty
    {
        public static bool Prefix(SpawnWaveProfile ___spawnWaveProfile, FloatDynamicStat ___roundTimeSinceStart,
                FloatDynamicStat ___worldMapDifficultyMultipler, FloatDynamicStat ___cycleDifficultyMultipler,
                ref float __result)
        {
            try
            {
                float wave = 1 - Mathf.Clamp01(___spawnWaveProfile.ReturnDifficultyAtTime(___roundTimeSinceStart.Value));
                //reduce worldMapDifficultyMultipler so enemies still come and go in waves with the difficultyCurve
                //difficultyCurve and spawn amounts are increased so balance is largely maintained
                float world = 1 - Mathf.Clamp01(___worldMapDifficultyMultipler.Value / 2f);
                float cycle = 1 - Mathf.Clamp01(___cycleDifficultyMultipler.Value);

                //this combination algorithm makes much more sense to me rather than adding and clamping
                //otherwise some multipliers become completely irrelevant when others are high
                __result = 1 - (wave * world * cycle);

                Plugin.Log.LogInfo($"difficulty: {__result:0.00} ({1 - wave:0.00} x {1 - world:0.00} x {1 - cycle:0.00})");

                return false;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
                return true;
            }
        }
    }
}
