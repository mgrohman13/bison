using DynamicEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LevelGeneration.WorldMap.TerritoryInstance;
using UnityEngine;

namespace WarpipsReplayability.Mod
{
    internal class Spawns
    {
        public static bool ModifySpawns(bool cap, InvokeableType spawnTech, float min, float max, float t, ref int __result)
        {
            if (Config.DifficultMode)
                //Plugin.Log.LogDebug($"{Map.Territories} {Map.MissionManagerAsset.CurrentWorldMap.lastAttackedTerritory}");
                if (Map.MissionManagerAsset != null && Map.MissionManagerAsset.CurrentWorldMap.lastAttackedTerritory > -1)
                {
                    string logDesc = cap ? "SpawnCap" : "SpawnCount";
                    LogInfo(logDesc, spawnTech, min, max);

                    HashSet<string> techTypes = new() { "PistolPip", "Shotgunner", "Warfighter", };// "UAZ", "Warmule", };             
                    if (techTypes.Contains(spawnTech.name))
                    {
                        max *= 1.25f;
                        min = (min + 2f * max) / 4f;
                        LogInfo(logDesc, spawnTech, min, max);

                        __result = Plugin.Rand.Round(Mathf.Lerp(min, max, t));
                        LogResult(logDesc, spawnTech, __result);
                        return false;
                    }
                }
                else
                {
                    Plugin.Log.LogDebug($"Spawns skipping due to initialization");
                }
            return true;
        }

        private static string Mission = string.Empty;
        private static readonly HashSet<string> LoggedInfo = new();
        private static void LogInfo(string logDesc, InvokeableType spawnTech, float min, float max)
        {
            string mission = Map.MissionManagerAsset.CurrentOperation.spawnWaveProfile.name;
            if (Mission != mission)
            {
                Mission = mission;
                Plugin.Log.LogInfo(mission);
            }

            string info = $"{mission} {spawnTech.name} {logDesc}: {min}-{max}";
            if (!LoggedInfo.Contains(info))
            {
                LoggedInfo.Add(info);
                Plugin.Log.LogInfo(info);
            }
        }
        private static readonly Dictionary<string, string> PreviousLog = new();
        private static void LogResult(string logDesc, InvokeableType spawnTech, int __result)
        {
            string key = $"{spawnTech.name} {logDesc}";
            string log = $"{key}: {__result}";

            PreviousLog.TryGetValue(key, out string prev);
            if (prev != log)
            {
                PreviousLog[key] = log;
                Plugin.Log.LogDebug(log);
            }
        }
    }
}
