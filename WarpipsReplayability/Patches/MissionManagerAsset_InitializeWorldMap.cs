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
    [HarmonyPatch(typeof(MissionManagerAsset))]
    [HarmonyPatch(nameof(MissionManagerAsset.InitializeWorldMap))]
    internal class MissionManagerAsset_InitializeWorldMap
    {
        public static void Prefix(MissionManagerAsset __instance, ref int worldMapIndex)
        {
            try
            {
                Plugin.Log.LogDebug("MissionManagerAsset_InitializeWorldMap Prefix");

                Map.MissionManagerAsset = __instance;

                //worldMapIndex = 3;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
