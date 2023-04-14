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
using DynamicEnums;
using System.Collections;
using UnityEngine;
using System.Reflection;
using static UnityEngine.Random;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(UnitDeathController))]
    [HarmonyPatch("OnDeath")]
    internal class UnitDeathController_OnDeath
    {
        public static void Prefix(UnitStatController ___statController, ref float __state)
        {
            try
            {
                if (Plugin.DifficultMode)
                {
                    Plugin.Log.LogDebug("UnitDeathController_OnDeath Prefix");

                    __state = ___statController.UnitData.xpBaseOnKill;

                    const float superEnemyMult = 5f;
                    Dictionary<string, float> mappings = new() {
                        { "PistolPip",  1 / 4f },
                        { "Warfighter", 1 / 3f },
                        { "Shotgunner", 1 / 3f },
                    };

                    string unit = ___statController.UnitData.name;
                    if (!mappings.TryGetValue(unit, out float exp))
                        exp = __state;
                    else
                    {
                        if (IsSuperEnemy(___statController.gameObject))
                        {
                            exp *= superEnemyMult;
                            Plugin.Log.LogDebug("SuperEnemy detected");
                        }
                        ___statController.UnitData.xpBaseOnKill = Plugin.Rand.GaussianOE(exp, .13f / (float)Math.Sqrt(exp), 0.052f / exp, .05f);
                    }

                    //exp *= 1.01f;
                    //___statController.UnitData.xpBaseOnKill = Plugin.Rand.GaussianOE(exp, .13f / (float)Math.Sqrt(exp), 0.052f / exp, .05f);
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
        public static void Postfix(UnitStatController ___statController, float __state)
        {
            ___statController.UnitData.xpBaseOnKill = __state;
        }


        private static readonly FieldInfo unitBuffsField = AccessTools.Field(AccessTools.TypeByName(
            "UpgradeController+BuffSource"), "unitBuffs");
        private static readonly FieldInfo buffsField = AccessTools.Field(typeof(UnitUpgradeController), "buffs");
        private static bool IsSuperEnemy(GameObject gameObject)
        {
            return gameObject.GetComponentsInChildren<UnitUpgradeController>()
                .SelectMany(unitUpgradeController =>
                    ((IDictionary)buffsField.GetValue(unitUpgradeController)).Values.Cast<object>())
                .SelectMany(buffSource => (List<UnitBuff>)unitBuffsField.GetValue(buffSource))
                .Any(unitBuff => unitBuff.name == "SuperEnemy_Speed");
        }
    }
}
