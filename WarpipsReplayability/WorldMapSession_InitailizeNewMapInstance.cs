using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using GameUI;
using System.Runtime.CompilerServices;
using LevelGeneration.WorldMap;

namespace WarpipsReplayability
{
    [HarmonyPatch(typeof(WorldMapSession))]
    [HarmonyPatch(nameof(WorldMapSession.InitailizeNewMapInstance))]
    internal class WorldMapSession_InitailizeNewMapInstance
    {
        public static void Postfix(ref WorldMapAsset ___worldMapAsset, ref TerritoryInstance[] ___territories)
        {
            try
            {
                Plugin.Log.LogInfo("WorldMapSession_InitailizeNewMapInstance Postfix");

                Map.Shuffle(___worldMapAsset, ___territories);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
