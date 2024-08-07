﻿using DynamicEnums;
using System.Collections.Generic;
using UnityEngine;

namespace WarpipsReplayability.Mod
{
    internal class Spawns
    {
        public static bool DoLog = true;
        private static readonly HashSet<string> LoggedInfo = new();

        public static bool ModifySpawns(bool cap, InvokeableType spawnTech, float min, float max, float t, ref int __result)
        {
            ////Plugin.Log.LogDebug($"{Map.Territories} {Map.MissionManagerAsset.CurrentWorldMap.lastAttackedTerritory}");
            //if (Map.MissionManagerAsset != null)// && Map.MissionManagerAsset.CurrentWorldMap.lastAttackedTerritory > -1)
            //{
            string logDesc = cap ? "SpawnCap" : "SpawnCount";
            //LogInfo(logDesc, spawnTech, min, max, t);

            //HashSet<string> techTypes = new() { "PistolPip", "Shotgunner", "Warfighter", };// "UAZ", "Warmule", };     
            //if (Config.DifficultMode)
            //    if (techTypes.Contains(spawnTech.name))
            //    {
            //        float mult = 1.25f;
            //        min = (min + (1 + mult) * max) / 4f;
            //        max *= mult;
            //        LogInfo(logDesc, spawnTech, min, max);
            //    }

            __result = Plugin.Rand.Round(Mathf.Lerp(min, max, t));
            if (DoLog)
                LogResult(logDesc, spawnTech, min, max, t, __result);
            return false;
            //}
            //else
            //{
            //    Plugin.Log.LogInfo($"Spawns skipping due to initialization");
            //}
            //return true;
        }

        //private static string Mission = string.Empty;
        //private static void LogInfo(string logDesc, InvokeableType spawnTech, float min, float max, float t)
        //{
        //    string mission = Map.MissionManagerAsset?.CurrentOperation.spawnWaveProfile.name;
        //    if (mission != "MainMenu")
        //    {
        //        if (Mission != mission)
        //        {
        //            Mission = mission;
        //            Plugin.Log.LogDebug(mission);
        //        }

        //        string info = $"{mission} {spawnTech.name} {logDesc}: {min}-{max}";
        //        if (!LoggedInfo.Contains(info))
        //        {
        //            LoggedInfo.Add(info);
        //            Plugin.Log.LogInfo(info + $" {t:0.000}");
        //        }
        //    }
        //}
        //private static readonly Dictionary<string, string> PreviousLog = new();
        private static void LogResult(string logDesc, InvokeableType spawnTech, float min, float max, float t, int __result)
        {
            string mission = Map.MissionManagerAsset?.CurrentOperation.spawnWaveProfile.name;
            if (mission != "MainMenu")
            {
                //if (Mission != mission)
                //{
                //    Mission = mission;
                //    Plugin.Log.LogDebug(mission);
                //}

                string info = $"{mission} {spawnTech.name} {logDesc} ({min}-{max}): {__result}";
                if (!LoggedInfo.Contains(info))
                {
                    LoggedInfo.Add(info);
                    Plugin.Log.LogInfo(info + $" {t:0.000}");
                }
            }

            //string key = $"{spawnTech.name} {logDesc}";
            //string log = $"{key}: {__result}";

            //PreviousLog.TryGetValue(key, out string prev);
            //if (prev != log)
            //{
            //    PreviousLog[key] = log;
            //    Plugin.Log.LogDebug(log);
            //}
        }
    }
}
