using Harmony;

namespace LeaviathanProwl.Patches
{
    [HarmonyPatch(typeof(ReaperLeviathan))]
    [HarmonyPatch("DamageVehicle")]
    class ReaperLeviathan_DamageVehicle
    {
        public static void Prefix(ReaperLeviathan __instance)
        {
            DamageSystem_CalculateDamage.attacker = __instance;
        }

        public static void Postfix()
        {
            DamageSystem_CalculateDamage.attacker = null;
        }
    }
}
