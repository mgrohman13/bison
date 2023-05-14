using DynamicEnums;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TechTree;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(ResourceController))]
    [HarmonyPatch(nameof(ResourceController.AddXpPoints))]
    internal class ResourceController_AddXpPoints
    {
        private static float xpModifierPerUpgrade, xpModifierPerTier, xpModifierOnKill;
        private static bool runPrefix = true;

        private static readonly HashSet<string> LoggedInfo = new();

        private static readonly FieldInfo _gameStat_int = AccessTools.Field(typeof(IntReadonlyStat), "gameStat");
        private static readonly FieldInfo _gameStat_float = AccessTools.Field(typeof(FloatReadonlyStat), "gameStat");

        public const double xpMult = .85;
        public const double xpExponent = 2.5;

        public static bool Prefix(ResourceController __instance, TechTreeInstance ___playerTechTree, List<TechType> ___xpAddons, IntResource ___techTier,
            ref IntReadonlyStat ___xpModifierPerUpgrade, ref IntReadonlyStat ___xpModifierPerTier, ref FloatReadonlyStat ___xpModifierOnKill,
            float addValue, ref int __result)
        {
            try
            {
                Plugin.Log.LogDebug("ResourceController_AddXpPoints Prefix");

                if (___xpModifierPerUpgrade.Value > 0 && ___xpModifierPerTier.Value > 0)
                {
                    SetValues(ref ___xpModifierPerUpgrade, ref ___xpModifierPerTier, ref ___xpModifierOnKill);
                    ////swap?
                    //(xpModifierPerUpgrade, xpModifierPerTier) = (xpModifierPerTier, xpModifierPerUpgrade);
                }

                if (runPrefix)
                {
                    float upgrades = 0;
                    if (___playerTechTree != null)
                        upgrades = ___xpAddons.Where(___playerTechTree.ReturnIsTechUnlocked).Count();
                    upgrades *= xpModifierPerUpgrade;
                    float tier = ___techTier.value * xpModifierPerTier;

                    ////game algorithm
                    //addValue = Plugin.Rand.Round((addValue * ___xpModifierOnKill.Value) + upgrades + tier);

                    //rebalance experience by giving less for basic units and more for better ones
                    float value = (float)Math.Pow(addValue * xpMult, xpExponent);
                    //apply bonuses as percentages, not additions 
                    value *= xpModifierOnKill + upgrades + tier;

                    float dev = (float)(.39 / Math.Sqrt(value));
                    Log(value, upgrades, tier, dev);
                    int xp = Plugin.Rand.GaussianCappedInt(value, dev, 1);

                    //there is a bug in AddXpPoints if you try to add more experience than needed for a single level
                    //you will be under-charged for subsequent levels after the first
                    //to fix this we call it manually in chunks but return the total amount added from this first call
                    __result = xp;
                    runPrefix = false;
                    while (xp > 0)
                    {
                        int level = __instance.ReturnXPNeededToLevel();
                        int add = Math.Min(xp, level);
                        Plugin.Log.LogDebug($"AddXpPoints {add} ({level})");
                        __instance.AddXpPoints(add);
                        xp -= add;

                        if (xp > 0)
                            Plugin.Log.LogWarning($"AddXpPoints loop ({xp}, {level})");
                    }
                    runPrefix = true;

                    return false;
                }
                else
                {
                    Plugin.Log.LogDebug($"skipping Prefix for loop");
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
            return true;
        }

        private static void SetValues(ref IntReadonlyStat ___xpModifierPerUpgrade, ref IntReadonlyStat ___xpModifierPerTier, ref FloatReadonlyStat ___xpModifierOnKill)
        {
            xpModifierPerUpgrade = ___xpModifierPerUpgrade.Value;
            xpModifierPerTier = ___xpModifierPerTier.Value;
            xpModifierOnKill = ___xpModifierOnKill.Value;

            //we are computing multiplier and bonuses with our own algorithm and don't want the game to apply any
            _gameStat_int.SetValue(___xpModifierPerUpgrade, 0);
            _gameStat_int.SetValue(___xpModifierPerTier, 0);
            _gameStat_float.SetValue(___xpModifierOnKill, 1);
        }

        private static void Log(float addValue, float upgrades, float tier, float dev)
        {
            string info = $"experience {addValue:0.00} (deviation {addValue * dev:0.00}, upgrades {upgrades}, tier {tier})";
            if (!LoggedInfo.Contains(info))
            {
                LoggedInfo.Add(info);
                Plugin.Log.LogInfo(info);
            }
        }
    }
}
