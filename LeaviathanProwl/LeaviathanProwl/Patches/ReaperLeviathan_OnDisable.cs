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
    [HarmonyPatch(typeof(ReaperLeviathan))]
    [HarmonyPatch(nameof(ReaperLeviathan.OnDisable))]
    class ReaperLeviathan_OnDisable
    {
        public static void Postfix(ReaperLeviathan __instance)
        {
            Logger.LogInfo("OnDisable");
            LeviathanPool.instance.Remove(__instance);
        }
    }
}
