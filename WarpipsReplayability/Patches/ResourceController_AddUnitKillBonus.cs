using HarmonyLib;
using System;
using System.Collections.Generic;
using TechTree;
using UnityEngine;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(ResourceController))]
    [HarmonyPatch(nameof(ResourceController.AddUnitKillBonus))]
    internal class ResourceController_AddUnitKillBonus
    {
        public static readonly HashSet<float> LoggedInfo = new();

        public static void Prefix(FloatDynamicStat ___behaviorModeFuel, ref float __state)
        //, FloatReadonlyStat ___behvaiorButtonFuelOnKillUpgraded, FloatGameModeModifiedValue ___behvaiorButtonFuelOnKill)
        {
            try
            {
                Plugin.Log.LogDebug("ResourceController_AddUnitKillBonus Prefix");
                ////0.03, 0.0325
                //Plugin.Log.LogInfo($"{___behvaiorButtonFuelOnKill.Value}, {___behvaiorButtonFuelOnKillUpgraded.Value}");

                __state = ___behaviorModeFuel.Value;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
        public static void Postfix(float baseXpValue, ref float __state, ref FloatDynamicStat ___behaviorModeFuel,
            TechTreeInstance ___playerTechTree, Upgrade ___behvaiorButtonFuelOnKillUpgrade,
            FloatReadonlyStat ___behvaiorButtonFuelOnKillUpgraded, FloatGameModeModifiedValue ___behvaiorButtonFuelOnKill)
        {
            try
            {
                Plugin.Log.LogDebug("ResourceController_AddUnitKillBonus Postfix");

                if (___behaviorModeFuel.Value > __state)
                {
                    float mult = ___playerTechTree.ReturnHasUpgrade(___behvaiorButtonFuelOnKillUpgrade)
                        ? ___behvaiorButtonFuelOnKillUpgraded.Value : ___behvaiorButtonFuelOnKill.Value;

                    //use a tempered down exponent compared to what we apply to experience
                    double exponent = Math.Sqrt(ResourceController_AddXpPoints.xpExponent);// (1 + ResourceController_AddXpPoints.xpExponent) / 2.0;
                    float avg = (float)Math.Pow(baseXpValue * ResourceController_AddXpPoints.xpMult, exponent);

                    float inc = Plugin.Rand.Gaussian(avg * mult, .13f);
                    ___behaviorModeFuel.Value = Mathf.Clamp01(__state + inc);
                    if (!LoggedInfo.Contains(avg))
                    {
                        LoggedInfo.Add(avg);
                        Plugin.Log.LogInfo($"behaviorModeFuel: {___behaviorModeFuel.Value:0.0000} (+{inc:0.0000})");
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
