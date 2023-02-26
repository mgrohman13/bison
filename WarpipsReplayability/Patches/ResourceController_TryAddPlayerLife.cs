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

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(ResourceController))]
    [HarmonyPatch(nameof(ResourceController.TryAddPlayerLife))]
    internal class ResourceController_TryAddPlayerLife
    {
        private static int lives = -1;

        public static void Prefix(ref ResourceController __instance)
        {
            try
            {
                Plugin.Log.LogDebug("ResourceController_TryAddPlayerLife Prefix");

                lives = __instance.PlayerLives.value;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
        public static void Postfix(ref ResourceController __instance)
        {
            try
            {
                Plugin.Log.LogDebug("ResourceController_TryAddPlayerLife Postfix");

                if (lives == __instance.PlayerLives.value)
                {
                    int points = Plugin.Rand.RangeInt(6, 9);
                    __instance.AddTechPoints(points);
                    lives = -1;

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
