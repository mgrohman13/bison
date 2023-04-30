using HarmonyLib;
using LevelGeneration.WorldMap;
using System;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(WorldMapSession))]
    [HarmonyPatch(nameof(WorldMapSession.RestoreMapState))]
    internal class WorldMapSession_RestoreMapState
    {
        public static void Postfix(WorldMapAsset ___worldMapAsset, TerritoryInstance[] ___territories)
        {
            try
            {
                Plugin.Log.LogDebug("WorldMapSession_RestoreMapState Postfix");

                Map.WorldMapAsset = ___worldMapAsset;
                Map.Territories = ___territories;

                Map.Load();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
