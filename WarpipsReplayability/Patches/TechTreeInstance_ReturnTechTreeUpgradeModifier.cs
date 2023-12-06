using DynamicEnums;
using HarmonyLib;
using System;
using TechTree;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(TechTreeInstance))]
    [HarmonyPatch(nameof(TechTreeInstance.ReturnTechTreeUpgradeModifier))]
    internal class TechTreeInstance_ReturnTechTreeUpgradeModifier
    {
        public static void Postfix(TechTreeInstance __instance, StatType pollUpgradeType, TechType pollTechType, bool returnAsInt, ref float __result)
        {
            try
            {
                Plugin.Log.LogDebug("TechTreeInstance_ReturnTechTreeUpgradeModifier Postfix");

                StatTypeMaster statTypeMaster = ActiveHotbarSlot_InvokeCooldown.StatTypeMaster;
                string techType = pollTechType.name;
                //the stimpack cooldown buff for the first unit rank is mistakenly applied to refillCooldown instead of invokeCooldown
                if (statTypeMaster != null && techType == "Stimpack" && pollUpgradeType == statTypeMaster.invokeCooldown)
                {
                    float result = __instance.ReturnTechTreeUpgradeModifier(statTypeMaster.refillCooldown, pollTechType, returnAsInt);
                    if (__result != result)
                    {
                        Plugin.Log.LogInfo($"Stimpack cooldown bugfix {__result} -> {result}");
                        __result = result;
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
