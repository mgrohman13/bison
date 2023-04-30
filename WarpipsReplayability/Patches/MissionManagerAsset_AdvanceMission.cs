using HarmonyLib;
using LevelGeneration;
using LevelGeneration.WorldMap;
using System;
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

                //IntDynamicStat cycleIndex = (IntDynamicStat)AccessTools.Field(typeof(MissionManagerAsset), "cycleIndex").GetValue(__instance);
                //Plugin.Log.LogInfo("cycleIndex: " + cycleIndex.Value);
                //cycleIndex.Value = 0;
                //Plugin.Log.LogInfo("cycleIndex: " + cycleIndex.Value);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
