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
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("CalculateBiome")]
    class Player_CalculateBiome
    {
        public static void Postfix(Player __instance, ref string __result)
        {
            LeviathanPool.instance.SetBiome(__result);
        }
    }
}
