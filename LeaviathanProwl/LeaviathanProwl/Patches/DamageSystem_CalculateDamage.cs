using System;
using System.Collections.Generic;
using Harmony;
using UnityEngine;

namespace LeaviathanProwl.Patches
{
    [HarmonyPatch(typeof(DamageSystem))]
    [HarmonyPatch(nameof(DamageSystem.CalculateDamage))]
    class DamageSystem_CalculateDamage
    {
        public static Creature attacker;

        private static readonly DamageType[] ignoreDamageTypes = new DamageType[] {
            //DamageType.Acid,
            //DamageType.Cold,
            DamageType.Collide,
            //DamageType.Drill,
            //DamageType.Electrical,
            DamageType.Explosive, // TODO
            DamageType.Fire,
            DamageType.Heat,
            //DamageType.LaserCutter,
            //DamageType.Normal,
            DamageType.Poison,
            DamageType.Pressure,
            //DamageType.Puncture,
            DamageType.Radiation,
            DamageType.Smoke,
            DamageType.Starve,
            DamageType.Undefined,
        };

        private static Dictionary<Type, float> creatureDamage = null;

        public static void Postfix(ref float __result, DamageType type, GameObject target, GameObject dealer)
        {
            //attacker-based damage modification logic from QCreatureConfig mod
            if (creatureDamage == null)
                creatureDamage = new Dictionary<Type, float>() {
                    { typeof(ReaperLeviathan), Config.difficulty.leviathanDamage },
                    { typeof(GhostLeviathan), Config.difficulty.leviathanDamage },
                    { typeof(CrabSnake),    1.25f },
                    { typeof(CrabSquid),    1.25f },
                    { typeof(Warper),       0.80f },
                };

            bool isPlayer = (target.GetComponent<Player>() != null);

            float mult = 1.0f;
            if (Array.IndexOf(ignoreDamageTypes, type) < 0)
            {
                bool isVehicle = (target.GetComponent<Vehicle>() != null);
                if (isPlayer)
                    mult = Config.difficulty.playerDamage;
                else if (isVehicle)
                    mult = Config.difficulty.vehicleDamage;

                if (isPlayer || isVehicle)
                {
                    if (dealer != null)
                        attacker = dealer.GetComponent<Creature>() ?? attacker;
                    if (attacker != null && creatureDamage.TryGetValue(attacker.GetType(), out float creatureMult))
                        mult *= creatureMult;
                }
            }

            __result *= mult;

            float max = (isPlayer ? (__result < 100 ? 99 : 198) : 3000);
            if (__result > 1 && __result < max)
                __result = Config.rand.GaussianCapped(__result, .15f, Math.Max(1, 2 * __result - max));

            if (!Mathf.Approximately(mult, 1f))
                Logger.LogInfo("damage multiplier: {0} hit {1} for {2} (x{3})", attacker, target, __result, mult);
        }
    }
}
