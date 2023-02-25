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
    [HarmonyPatch(typeof(MissionManagerAsset))]
    [HarmonyPatch(nameof(MissionManagerAsset.AdvanceMission))]
    internal class MissionManagerAsset_AdvanceMission
    {
        public static void Prefix()
        {
            try
            {
                Plugin.Log.LogInfo("MissionManagerAsset_AdvanceMission Prefix");

                Map.DoShuffle = true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
