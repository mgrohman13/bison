using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using GameUI;
using System.Runtime.CompilerServices;
using LevelGeneration.WorldMap;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(WorldMapSession))]
    [HarmonyPatch(nameof(WorldMapSession.InitailizeNewMapInstance))]
    internal class WorldMapSession_InitailizeNewMapInstance
    {
        public static void Postfix(WorldMapAsset ___worldMapAsset, TerritoryInstance[] ___territories)
        {
            try
            {
                Plugin.Log.LogDebug("WorldMapSession_InitailizeNewMapInstance Postfix");

                Map.WorldMapAsset = ___worldMapAsset;
                Map.Territories = ___territories;

                Map.Randomize();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
