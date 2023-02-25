using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using GameUI;
using System.Runtime.CompilerServices;
using LevelGeneration.WorldMap;

namespace WarpipsReplayability
{
    [HarmonyPatch(typeof(MainMenuController))]
    [HarmonyPatch("NewCampaignCommon")]
    internal class MainMenuController_NewCampaignCommon
    {
        public static void Prefix()
        {
            try
            {
                Plugin.Log.LogInfo("MainMenuController_NewCampaignCommon Prefix");

                Map.DoShuffle = true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
