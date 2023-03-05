using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using GameUI;
using System.Runtime.CompilerServices;
using LevelGeneration.WorldMap;
using LevelGeneration;
using WarpipsReplayability.Mod;
using DynamicEnums;
using UnityEngine;
using I2.Loc;
using TMPro;
using UnityEngine.UI;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(TerritoryRewardPanelController))]
    [HarmonyPatch(nameof(TerritoryRewardPanelController.OnNewTerritorySelected))]
    internal class TerritoryRewardPanelController_OnNewTerritorySelected
    {
        public static void Prefix(TerritoryInspector __instance)
        {
            try
            {
                Plugin.Log.LogDebug("TerritoryRewardPanelController_OnNewTerritorySelected Prefix");

                //needed for ItemRewardGridController_RefreshItemDisplay
                Operations.SelectedTerritory = __instance.InspectedTerritoryInstance;
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
                Plugin.Log.LogDebug("TerritoryRewardPanelController_OnNewTerritorySelected Postfix");

                Operations.SelectedTerritory = null;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
