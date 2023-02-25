using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using GameUI;
using System.Runtime.CompilerServices;
using LevelGeneration.WorldMap;
using GameIO;

namespace WarpipsReplayability
{
    [HarmonyPatch(typeof(WorldMapSession))]
    [HarmonyPatch(nameof(WorldMapSession.RestoreMapState))]
    internal class WorldMapSession_RestoreMapState
    {
        public static void Postfix(ref WorldMapAsset ___worldMapAsset, ref TerritoryInstance[] ___territories)
        {
            try
            {
                Plugin.Log.LogDebug("WorldMapSession_RestoreMapState Postfix");

                Map.WorldMapAsset = ___worldMapAsset;
                Map.Territories = ___territories;

                Map.LoadShuffle();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
