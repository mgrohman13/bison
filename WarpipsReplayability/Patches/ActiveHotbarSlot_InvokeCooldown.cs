using DynamicEnums;
using GameUI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TechTree;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(ActiveHotbarSlot))]
    [HarmonyPatch("InvokeCooldown")]
    internal class ActiveHotbarSlot_InvokeCooldown
    {
        public static StatTypeMaster StatTypeMaster { get; private set; }

        public static void Prefix(TechTreeInstance ___playerTechTree, StatTypeMaster ___statTypeMaster, TechNodeData ___cachedNodeData)
        {
            try
            {
                Plugin.Log.LogDebug("ActiveHotbarSlot_InvokeCooldown Prefix");

                ActiveHotbarSlot_InvokeCooldown.StatTypeMaster = ___statTypeMaster;

                float modifierInvoke = ___playerTechTree.ReturnTechTreeUpgradeModifier(___statTypeMaster.invokeCooldown, ___cachedNodeData.techType, false);
                float modifierRefill = ___playerTechTree.ReturnTechTreeUpgradeModifier(___statTypeMaster.refillCooldown, ___cachedNodeData.techType, false);

                LogCooldowns(___cachedNodeData, modifierInvoke, modifierRefill);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }

        //private static readonly string[] fixedTechs = new string[] { };// "Stimpack" };
        private static readonly SortedDictionary<string, string> cooldowns = new();
        private static void LogCooldowns(TechNodeData techNode, float modifierInvoke, float modifirRefill)
        {
            string key = techNode.techType.name;
            string value = $"({techNode.baseCooldown}{Modifier(modifierInvoke)}, {techNode.refillCountCooldown}{Modifier(modifirRefill)})";

            bool contains = cooldowns.TryGetValue(key, out string previous);
            cooldowns[key] = value;
            if (!contains || previous != value)
                Plugin.LogAtLevel(cooldowns
                        .Select(p => $"{p.Key}: {p.Value}")
                        .Aggregate("Cooldown:", (a, b) => a + Environment.NewLine + b),
                    contains && previous != value);

            if (((techNode.baseCooldown == 0 && modifierInvoke != 0)
                        || (techNode.refillCountCooldown == 0 && modifirRefill != 0))
                    && modifierInvoke != modifirRefill)
                Plugin.Log.LogError($"{key} cooldown bug {value}");

            static string Modifier(float modifier) => modifier == 0 ? "" : " " + (modifier > 0 ? "+" : "") + modifier.ToString();
        }
    }
}
