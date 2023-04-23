using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using GameUI;
using System.Runtime.CompilerServices;
using LevelGeneration.WorldMap;
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

                if (Map.WorldMapAsset != null && Map.OriginalConnections != null)
                {
                    AccessTools.Field(typeof(WorldMapAsset), "territoryConnections").SetValue(Map.WorldMapAsset, Map.OriginalConnections);
                    Plugin.Log.LogInfo("restored connections");
                }

                Map.DoShuffle = true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
