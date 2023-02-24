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
    [HarmonyPatch(nameof(DefaultResourceSetter.SetNewConquestModeResources))]
    internal class DefaultResourceSetter_SetNewConquestModeResources
    {
        //static bool Prefix(DefaultResourceSetter __instance, ref ResourceController ___resourceController)
        //{
        //    Plugin.Log.LogInfo("DefaultResourceSetter_SetNewConquestModeResources Prefix"); 
        //    return true;
        //}

        public static void Postfix(ref ResourceController ___resourceController)
        {
            try
            {
                Plugin.Log.LogDebug("DefaultResourceSetter_SetNewConquestModeResources Postfix");

                ___resourceController.HardSetPlayerLives(2, 3);
                Plugin.Log.LogInfo("HardSetPlayerLives(2, 3)");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
