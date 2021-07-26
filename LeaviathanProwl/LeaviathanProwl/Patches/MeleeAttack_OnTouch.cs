using Harmony;
using UnityEngine;

namespace LeaviathanProwl.Patches
{
    [HarmonyPatch(typeof(MeleeAttack))]
    [HarmonyPatch(nameof(MeleeAttack.OnTouch))]
    class MeleeAttack_OnTouch
    {
        public static void Prefix(MeleeAttack __instance)
        {
            DamageSystem_CalculateDamage.attacker = __instance.gameObject.GetComponent<Creature>();
        }

        public static void Postfix()
        {
            DamageSystem_CalculateDamage.attacker = null;
        }
    }
}