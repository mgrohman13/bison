﻿using System;
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
    [HarmonyPatch(typeof(MissionManagerAsset))]
    [HarmonyPatch(nameof(MissionManagerAsset.InitializeWorldMap))]
    internal class MissionManagerAsset_InitializeWorldMap
    {
        public static void Prefix(ref MissionManagerAsset __instance)
        {
            try
            {
                Plugin.Log.LogInfo("MissionManagerAsset_InitializeWorldMap Prefix");

                Map.MissionManagerAsset = __instance;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
