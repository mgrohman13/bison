using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using WarpipsReplayability.Mod;

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
                Plugin.Log.LogDebug("UnitDeathController_OnDeath Prefix");
                __state = ___statController.UnitData.xpBaseOnKill;

                //___statController.UnitData.

                string name = ___statController.UnitData.name;
                LogExp(name, __state);

                ////if (Config.DifficultMode)
                ////{
                ////    const float superEnemyMult = 5f;
                //Dictionary<string, float> mappings = new() {
                //    { "PistolPip", 1 / 2f },
                //    //{ "PistolPip", 1 / 3f },
                //    //{ "Warfighter", 2 / 3f },
                //    //{ "Shotgunner", 2 / 3f },
                //};

                //if (!mappings.TryGetValue(name, out float mult))
                //    mult = 1f;
                ////if (IsSuperEnemy(___statController.gameObject))
                ////{
                ////    mult *= superEnemyMult;
                ////    Plugin.Log.LogDebug("SuperEnemy detected");
                ////}

                ////if (mult != 1f)
                ////{
                //float exp = __state * mult;
                //___statController.UnitData.xpBaseOnKill = Plugin.Rand.GaussianCapped(exp, .1f / exp, .1f);
                ////___statController.UnitData.xpBaseOnKill = Plugin.Rand.GaussianOE(exp, .13f / (float)Math.Sqrt(exp), 0.052f / exp, .05f);
                ////}
                ////}
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }

        //public static void Postfix(UnitStatController ___statController, float __state)
        //{
        //    Plugin.Log.LogDebug("UnitDeathController_OnDeath Postfix");
        //    ___statController.UnitData.xpBaseOnKill = __state;
        //}

        //private static readonly FieldInfo unitBuffsField = AccessTools.Field(AccessTools.TypeByName(
        //    "UpgradeController+BuffSource"), "unitBuffs");
        //private static readonly FieldInfo buffsField = AccessTools.Field(typeof(UnitUpgradeController), "buffs");
        //private static bool IsSuperEnemy(GameObject gameObject)
        //{
        //    return gameObject.GetComponentsInChildren<UnitUpgradeController>()
        //        .SelectMany(unitUpgradeController =>
        //            ((IDictionary)buffsField.GetValue(unitUpgradeController)).Values.Cast<object>())
        //        .SelectMany(buffSource => (List<UnitBuff>)unitBuffsField.GetValue(buffSource))
        //        .Any(unitBuff => unitBuff.name == "SuperEnemy_Speed");
        //}

        private static readonly Dictionary<string, float> expMap = new();
        private static string LogExp(string name, float exp)
        {
            if (!expMap.ContainsKey(name))
            {
                expMap.Add(name, exp);
                Plugin.Log.LogInfo(expMap
                    .Select(p => $"{p.Key}:{p.Value}")
                    .Aggregate("xpBaseOnKill:",
                        (a, b) => a + Environment.NewLine + b));
            }
            return name;
        }
    }
}
