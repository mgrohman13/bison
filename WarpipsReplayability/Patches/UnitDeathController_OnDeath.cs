using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(UnitDeathController))]
    [HarmonyPatch("OnDeath")]
    internal class UnitDeathController_OnDeath
    {
        private static readonly FieldInfo _unitBuffs = AccessTools.Field(AccessTools.TypeByName("UpgradeController+BuffSource"), "unitBuffs");
        private static readonly FieldInfo _buffs = AccessTools.Field(typeof(UnitUpgradeController), "buffs");

        public static void Prefix(UnitStatController ___statController, UnitTeamController ___unitTeamController, ref float __state)
        {
            try
            {
                Plugin.Log.LogDebug("UnitDeathController_OnDeath Prefix");

                __state = ___statController.UnitData.xpBaseOnKill;
                string name = ___statController.UnitData.name;

                //balance tweaks
                Dictionary<string, float> offsets = new() {
                    { "Hind",           +1/2f }, //4
                    { "Bubba",          -1/1f }, //5
                    { "Howitzer",       +2/3f }, //3
                    { "Predator",       +1/2f }, //3
                    { "Tanya",          -7/4f }, //5
                  //{ "T92",                  }, //3
                  //{ "MediumTurret",         }, //3
                    { "GRUZ",           +1/2f }, //2
                    { "GuardTower",     -2/3f }, //3
                  //{ "DuneBuggy",            }, //2
                  //{ "GasPip",               }, //2
                    { "RPGSoldier",     +3/4f }, //1
                    { "Warmule",        -1/3f }, //2
                    { "Sharpshooter",   +1/2f }, //1
                    { "UAZ",            -2/3f }, //2
                  //{ "Shotgunner",           }, //1
                  //{ "Warfighter",           }, //1
                    { "PistolPip",      -1/3f }, //1
                };

                float exp = __state;
                if (offsets.TryGetValue(name, out float offset))
                    exp += offset;
                bool super = IsSuperEnemy(___statController.gameObject);
                if (super)
                    exp += 1 / 6f;

                if (___unitTeamController.UnitTeam == UnitTeam.Team2)
                    LogExp(name, exp, super);

                ___statController.UnitData.xpBaseOnKill = exp;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }

        public static void Postfix(UnitStatController ___statController, float __state)
        {
            Plugin.Log.LogDebug("UnitDeathController_OnDeath Postfix");
            ___statController.UnitData.xpBaseOnKill = __state;
        }

        private static bool IsSuperEnemy(GameObject gameObject)
        {
            return gameObject.GetComponentsInChildren<UnitUpgradeController>()
                .SelectMany(unitUpgradeController =>
                    ((IDictionary)_buffs.GetValue(unitUpgradeController)).Values.Cast<object>())
                .SelectMany(buffSource => (List<UnitBuff>)_unitBuffs.GetValue(buffSource))
                .Any(IsSpeed);
            static bool IsSpeed(UnitBuff unitBuff)
            {
                bool speed = unitBuff.name == "SuperEnemy_Speed";
                LogBuffs(unitBuff, speed);
                return speed;
            };
        }

        private static string LastLog, LastLog2;
        private static void LogBuffs(UnitBuff unitBuff, bool speed)
        {
            string log = unitBuff.name;
            if (LastLog2 != log)
            {
                LastLog2 = log;
                Plugin.Log.LogDebug(log);
            }
            if (speed && unitBuff.limitedToTypes.Any())
            {
                log = unitBuff.limitedToTypes.Select(t => t.name).Aggregate("super types:", (a, b) => a + " " + b);
                if (LastLog != log)
                {
                    LastLog = log;
                    Plugin.Log.LogWarning(log);
                }
            }
        }
        private static readonly Dictionary<string, float> expMap = new();
        private static string LogExp(string name, float exp, bool super)
        {
            string key = name;
            if (super)
                key += " (super)";
            if (!expMap.ContainsKey(key))
            {
                expMap.Add(key, exp);
                Plugin.Log.LogInfo(expMap.OrderBy(p => p.Value).ThenBy(p => p.Key)
                    .Select(p => $"{p.Key}:{p.Value:0.00}")
                    .Aggregate("xpBaseOnKill:", (a, b) => a + Environment.NewLine + b));
            }
            return key;
        }
    }
}
