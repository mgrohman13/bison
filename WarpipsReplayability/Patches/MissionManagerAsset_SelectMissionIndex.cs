//using System;
//using System.Collections.Generic;
//using System.Linq;
//using BepInEx;
//using HarmonyLib;
//using GameUI;
//using System.Runtime.CompilerServices;
//using LevelGeneration.WorldMap;
//using WarpipsReplayability.Mod;
//using LevelGeneration;

//namespace WarpipsReplayability.Patches
//{
//    [HarmonyPatch(typeof(MissionManagerAsset))]
//    [HarmonyPatch(nameof(MissionManagerAsset.SelectMissionIndex))]
//    internal class MissionManagerAsset_SelectMissionIndex
//    {
//        public static void Prefix(ref int worldMapIndex)
//        {
//            try
//            {
//                Plugin.Log.LogDebug("MissionManagerAsset_SelectMissionIndex Prefix");

//                int? forceWorldMapIndex = Map.ForceWorldMapIndex;
//                if (forceWorldMapIndex.HasValue)
//                    worldMapIndex = forceWorldMapIndex.Value;

//                Plugin.Log.LogDebug($"SelectMissionIndex '{Operations.SelectedTerritory?.operation.spawnWaveProfile.name}'");
//            }
//            catch (Exception e)
//            {
//                Plugin.Log.LogError(e);
//            }
//        }
//    }
//}
