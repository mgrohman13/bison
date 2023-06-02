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
        public static void Prefix(MissionManagerAsset __instance, ref bool __state)
        {
            try
            {
                Plugin.Log.LogDebug("MissionManagerAsset_AdvanceMission Prefix");

                WorldMapSession currentWorldMap = __instance.CurrentWorldMap;
                TerritoryInstance lastAttackedTerritory = currentWorldMap.territories[currentWorldMap.lastAttackedTerritory];
                __state = lastAttackedTerritory.specialTag == TerritoryInstance.SpecialTag.EnemyObjective;
                Plugin.Log.LogDebug($"lastAttackedTerritory {lastAttackedTerritory.operation.spawnWaveProfile.name}");
                if (__state)
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

        public static void Postfix(MissionManagerAsset __instance, ref FloatDynamicStat ___worldMapDifficultyMultipler, ref bool __state)
        {
            try
            {
                Plugin.Log.LogDebug("MissionManagerAsset_AdvanceMission Postfix");

                int worldMapIndex = __instance.WorldMapIndex;
                Plugin.Log.LogDebug($"state {__state}  worldMapIndex {worldMapIndex}  worldMapDifficultyMultipler {___worldMapDifficultyMultipler.Value}");
                if (__state && worldMapIndex == 3)
                {
                    ___worldMapDifficultyMultipler.Value = Plugin.Rand.GaussianCapped(.5f, .065f, .35f);
                    Plugin.Log.LogInfo($"WorldMapIndex {worldMapIndex} set worldMapDifficultyMultipler to {___worldMapDifficultyMultipler.Value}");
                }

                //save SaleIndex
                Persist.SaveCurrent();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
