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
    [HarmonyPatch(nameof(ResourceController.SetCash))]
    internal class ResourceController_SetCash
    {
        public static void Prefix()//ref int newValue)
        {
            try
            {
                Plugin.Log.LogDebug("ResourceController_SetCash Prefix");

                //if (Map.MissionManagerAsset != null)
                //    newValue = Map.MissionManagerAsset.WorldMapIndex switch
                //    {
                //        0 => 40,
                //        1 => 60,
                //        2 => 80,
                //        3 => 100,
                //        _ => throw new Exception($"Map.MissionManagerAsset.WorldMapIndex {Map.MissionManagerAsset.WorldMapIndex}"),
                //    };
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
