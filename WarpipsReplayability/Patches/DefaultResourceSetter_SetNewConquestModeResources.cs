using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using GameUI;
using System.Runtime.CompilerServices;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(DefaultResourceSetter))]
    [HarmonyPatch(nameof(DefaultResourceSetter.SetNewConquestModeResources))]
    internal class DefaultResourceSetter_SetNewConquestModeResources
    {
        public static void Postfix(ref ResourceController ___resourceController)
        {
            try
            {
                Plugin.Log.LogDebug("DefaultResourceSetter_SetNewConquestModeResources Postfix");

                ___resourceController.HardSetPlayerLives(3, 3);
                Plugin.Log.LogInfo("HardSetPlayerLives");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
