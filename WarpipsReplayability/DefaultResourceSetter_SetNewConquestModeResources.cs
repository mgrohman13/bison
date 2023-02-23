using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using GameUI;
using System.Runtime.CompilerServices;

namespace WarpipsReplayability
{
    [HarmonyPatch(typeof(DefaultResourceSetter))]
    [HarmonyPatch("SetNewConquestModeResources")] // if possible use nameof() here
    class DefaultResourceSetter_SetNewConquestModeResources
    {
        //static bool Prefix(DefaultResourceSetter __instance, ref ResourceController ___resourceController)
        //{
        //    Plugin.Log.LogInfo("Prefix"); 
        //    return true;
        //}

        static void Postfix(ref ResourceController ___resourceController)
        {
            ___resourceController.HardSetPlayerLives(2, 3);
            Plugin.Log.LogInfo("HardSetPlayerLives(2, 3)");
        }
    }
}
