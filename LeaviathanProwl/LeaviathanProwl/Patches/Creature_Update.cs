using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Harmony;
using UnityEngine;

namespace LeaviathanProwl.Patches
{
    [HarmonyPatch(typeof(Creature))]
    [HarmonyPatch(nameof(Creature.Update))]
    class Creature_Update
    {
        public static void Prefix(Creature __instance)
        {
            if (__instance is GhostLeviathan)
                LeviathanPool.instance.Update((GhostLeviathan)__instance);
        }
    }

    [HarmonyPatch(typeof(ReaperLeviathan))]
    [HarmonyPatch(nameof(ReaperLeviathan.Update))]
    class ReaperLeviathan_Update
    {
        public static void Prefix(ReaperLeviathan __instance)
        {
            LeviathanPool.instance.Update(__instance);
        }
    }
}
