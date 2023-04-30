using DynamicEnums;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TechTree;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(ResourceController))]
    [HarmonyPatch(nameof(ResourceController.AddXpPoints))]
    internal class ResourceController_AddXpPoints
    {
        private static string LastLog = string.Empty;
        public static void Prefix(IntReadonlyStat ___xpModifierPerUpgrade, IntReadonlyStat ___xpModifierPerTier, FloatReadonlyStat ___xpModifierOnKill,
            TechTreeInstance ___playerTechTree, List<TechType> ___xpAddons, IntResource ___techTier)
        {
            try
            {
                Plugin.Log.LogDebug("ResourceController_AddXpPoints Prefix");

                int num = ___xpAddons.Count(___playerTechTree.ReturnIsTechUnlocked);
                int num3 = ___techTier.value;
                string log = $"xpAddons: {num}, techTier: {num3}"
                    + $"xpModifierPerUpgrade: {___xpModifierPerUpgrade.Value}, "
                    + $"xpModifierPerTier: {___xpModifierPerTier.Value}, "
                    + $"xpModifierOnKill: {___xpModifierOnKill.Value}, ";
                if (LastLog != log)
                {
                    LastLog = log;
                    Plugin.Log.LogInfo(log);
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
