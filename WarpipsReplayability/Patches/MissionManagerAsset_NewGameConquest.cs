using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using GameUI;
using System.Runtime.CompilerServices;
using LevelGeneration.WorldMap;
using WarpipsReplayability.Mod;
using LevelGeneration;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(MissionManagerAsset))]
    [HarmonyPatch(nameof(MissionManagerAsset.NewGameConquest))]
    internal class MissionManagerAsset_NewGameConquest
    {
        public static void Postfix(MissionManagerAsset __instance)
        {
            try
            {
                Plugin.Log.LogDebug("MissionManagerAsset_NewGameConquest Postfix");

                if (Map.ForceWorldMapIndex.HasValue)
                    typeof(MissionManagerAsset).GetProperty("WorldMapIndex").SetValue(__instance, Map.ForceWorldMapIndex.Value);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
