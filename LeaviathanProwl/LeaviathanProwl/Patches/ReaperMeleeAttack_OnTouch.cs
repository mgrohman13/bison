using Harmony;

namespace LeaviathanProwl.Patches
{
    [HarmonyPatch(typeof(ReaperMeleeAttack))]
    [HarmonyPatch(nameof(ReaperMeleeAttack.OnTouch))]
    class ReaperMeleeAttack_OnTouch
    {
        public static void Prefix(ReaperMeleeAttack __instance)
        {
            DamageSystem_CalculateDamage.attacker = __instance.gameObject.GetComponent<Creature>();
        }

        public static void Postfix()
        {
            DamageSystem_CalculateDamage.attacker = null;
        }
    }
}
