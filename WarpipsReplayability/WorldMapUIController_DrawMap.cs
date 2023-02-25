using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using GameUI;
using System.Runtime.CompilerServices;
using LevelGeneration.WorldMap;
using LevelGeneration;

namespace WarpipsReplayability
{
    [HarmonyPatch(typeof(WorldMapUIController))]
    [HarmonyPatch("DrawMap")]
    internal class WorldMapUIController_DrawMap
    {
        public static void Postfix(ref WorldMapUIController __instance, ref MissionManagerAsset ___missionManagerAsset)
        {
            try
            {
                Plugin.Log.LogDebug("WorldMapUIController_DrawMap Postfix");

                Operations.UpdateShroud(__instance, ___missionManagerAsset);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
