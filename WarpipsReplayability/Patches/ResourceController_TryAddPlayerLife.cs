﻿using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using GameUI;
using System.Runtime.CompilerServices;
using LevelGeneration.WorldMap;
using LevelGeneration;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(ResourceController))]
    [HarmonyPatch(nameof(ResourceController.TryAddPlayerLife))]
    internal class ResourceController_TryAddPlayerLife
    {
        public static void Prefix(ResourceController __instance, ref int __state)
        {
            try
            {
                Plugin.Log.LogDebug("ResourceController_TryAddPlayerLife Prefix");

                __state = __instance.PlayerLives.value;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
        public static void Postfix(ResourceController __instance, int __state)
        {
            try
            {
                Plugin.Log.LogDebug("ResourceController_TryAddPlayerLife Postfix");

                if (__state == __instance.PlayerLives.value)
                {
                    int points = Plugin.Rand.RangeInt(6, 9);
                    __instance.AddTechPoints(points);

                    Plugin.Log.LogInfo($"Bonus max life tech points {points}");
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
