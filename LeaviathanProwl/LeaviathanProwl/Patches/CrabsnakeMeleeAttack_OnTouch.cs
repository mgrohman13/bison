using Harmony;

namespace LeaviathanProwl.Patches
{
    [HarmonyPatch(typeof(CrabsnakeMeleeAttack))]
    [HarmonyPatch(nameof(CrabsnakeMeleeAttack.OnTouch))]
    class CrabsnakeMeleeAttack_OnTouch
    {
        public static void Prefix(CrabsnakeMeleeAttack __instance)
        {
            DamageSystem_CalculateDamage.attacker = __instance.gameObject.GetComponent<Creature>();
        }

        public static void Postfix()
        {
            DamageSystem_CalculateDamage.attacker = null;
        }
    }
}
