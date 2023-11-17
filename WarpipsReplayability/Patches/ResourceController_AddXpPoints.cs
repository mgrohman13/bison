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
        private static int xpModifierPerUpgrade, xpModifierPerTier;
        private static float xpModifierOnKill;
        private static bool runPrefix = true;

        private static readonly HashSet<string> LoggedInfo = new();

        private static readonly FieldInfo _gameStat_int = AccessTools.Field(typeof(IntReadonlyStat), "gameStat");
        private static readonly FieldInfo _gameStat_float = AccessTools.Field(typeof(FloatReadonlyStat), "gameStat");

        public const double xpMult = 5 / 6.0;
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
                    //let recursive ResourceController.AddXpPoints calls run the base game code
                    runPrefix = false;

                    int upgrades = GetUpgrades(___playerTechTree, ___xpAddons);
                    int tier = GetTier(___techTier);
                    __result = AddXpPoints(__instance, upgrades, tier, addValue);

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

        private static int AddXpPoints(ResourceController instance, int upgrades, int tier, float addValue)
        {
            ////for comparison, game algorithm:
            //int xp = (int)Math.Round((addValue * xpModifierOnKill) + upgrades + tier);

            //rebalance experience by giving less for basic units and more for better ones
            float value = (float)Math.Pow(addValue * xpMult, xpExponent);
            //apply bonuses as percentages, not additions 
            value *= xpModifierOnKill + upgrades + tier;

            float dev = (float)(.39 / Math.Sqrt(value));
            int xp = Plugin.Rand.GaussianCappedInt(value, dev, 1);
            Log(xp, value, dev, upgrades, tier);

            //there is a bug in AddXpPoints if you try to add more experience than needed for a single level
            //you will be under-charged for subsequent levels after the first
            //to fix this we call it manually in chunks but return, from this first call, the total amount added 
            int result = xp;
            while (xp > 0)
            {
                int level = instance.ReturnXPNeededToLevel();
                int add = Math.Min(xp, level);
                Plugin.Log.LogDebug($"AddXpPoints {add} ({level})");
                instance.AddXpPoints(add);
                xp -= add;

                if (xp > 0)
                    Plugin.Log.LogWarning($"AddXpPoints loop ({xp}, {level})");
            }

            return result;
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
        private static int GetUpgrades(TechTreeInstance playerTechTree, List<TechType> xpAddons)
        {
            int upgrades = 0;
            if (playerTechTree is not null)
                upgrades = xpAddons.Where(playerTechTree.ReturnIsTechUnlocked).Count();
            return upgrades * xpModifierPerUpgrade;
        }
        private static int GetTier(IntResource techTier)
        {
            return techTier.value * xpModifierPerTier;
        }
        private static void Log(int xp, float value, float dev, int upgrades, int tier)
        {
            string info = $"({value:0.00},{value * dev:0.00}), upgrades {upgrades}, tier {tier})";
            if (!LoggedInfo.Contains(info))
            {
                LoggedInfo.Add(info);
                Plugin.Log.LogInfo($"experience {xp} {info}");
            }
        }
    }
}
