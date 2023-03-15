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
        public static bool ModifySpawns(string logDesc, InvokeableType spawnTech, float min, float max, float t, ref int __result)
        {
            if (Map.MissionManagerAsset != null)
            {
                LevelGeneration.WorldMap.TerritoryInstance territory = Map.Territories[Map.MissionManagerAsset.CurrentWorldMap.lastAttackedTerritory];
                if (Map.MissionManagerAsset.WorldMapIndex == 0 && territory.specialTag == SpecialTag.None)
                {
                    const float totalMult = 1.69f, minInc = .39f;

                    min *= totalMult;
                    max *= totalMult;
                    min = Mathf.Lerp(min, max, minInc);
                    __result = Plugin.Rand.Round(Mathf.Lerp(min, max, t));

                    Log(logDesc, spawnTech, __result);

                    return false;
                }
            }
            else
            {
                Plugin.Log.LogInfo($"Spawns null MissionManagerAsset");
            }
            return true;
        }

        private static string Mission = string.Empty;
        private static readonly Dictionary<string, string> PreviousLog = new();
        private static void Log(string logDesc, InvokeableType spawnTech, int __result)
        {
            string mission = Map.MissionManagerAsset.CurrentOperation.spawnWaveProfile.name;
            if (Mission != mission)
            {
                Mission = mission;
                Plugin.Log.LogInfo(mission);
            }

            string key = $"{spawnTech} {logDesc}";
            string log = $"{key}: {__result}";

            PreviousLog.TryGetValue(key, out string prev);
            if (prev != log)
            {
                PreviousLog[key] = log;
                Plugin.Log.LogInfo(log);
            }
        }
    }
}
