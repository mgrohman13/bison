﻿using HarmonyLib;
using LevelGeneration.WorldMap;
using System;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(WorldMapUIController))]
    [HarmonyPatch("DrawMap")]
    internal class WorldMapUIController_DrawMap
    {
        public static void Postfix(WorldMapUIController __instance)
        {
            try
            {
                Plugin.Log.LogDebug("WorldMapUIController_DrawMap Postfix");

                Operations.DrawMap(__instance);

                foreach (var t in Map.Territories)
                    Plugin.Log.LogDebug($"{t.index} - {t.centerPos}");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
