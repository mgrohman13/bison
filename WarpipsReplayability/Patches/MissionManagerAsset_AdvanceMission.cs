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
    [HarmonyPatch(nameof(MissionManagerAsset.AdvanceMission))]
    internal class MissionManagerAsset_AdvanceMission
    {
        public static void Prefix(MissionManagerAsset __instance)
        {
            try
            {
                Plugin.Log.LogDebug("MissionManagerAsset_AdvanceMission Prefix");

                WorldMapSession currentWorldMap = __instance.CurrentWorldMap;
                TerritoryInstance lastAttackedTerritory = currentWorldMap.territories[currentWorldMap.lastAttackedTerritory];
                if (lastAttackedTerritory.specialTag == TerritoryInstance.SpecialTag.EnemyObjective)
                {
                    Map.DoShuffle = true;
                    Plugin.Log.LogInfo("DoShuffle set for new WorldMapIndex");
                } 
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
