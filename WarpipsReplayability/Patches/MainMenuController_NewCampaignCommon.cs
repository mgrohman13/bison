﻿using GameUI;
using HarmonyLib;
using LevelGeneration.WorldMap;
using System;
using System.Reflection;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(MainMenuController))]
    [HarmonyPatch("NewCampaignCommon")]
    internal class MainMenuController_NewCampaignCommon
    {
        public static void Prefix()
        {
            try
            {
                Plugin.Log.LogDebug("MainMenuController_NewCampaignCommon Prefix");

                Map.DoShuffle = true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }

        public static void Postfix()
        {
            try
            {
                Plugin.Log.LogDebug("MainMenuController_NewCampaignCommon Postfix");

                //save SaleIndex
                Persist.SaveCurrent();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
