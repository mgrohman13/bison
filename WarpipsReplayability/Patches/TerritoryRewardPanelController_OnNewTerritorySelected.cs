using GameUI;
using HarmonyLib;
using LevelGeneration.WorldMap;
using System;
using System.Runtime.CompilerServices;
using TMPro;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(TerritoryRewardPanelController))]
    [HarmonyPatch(nameof(TerritoryRewardPanelController.OnNewTerritorySelected))]
    internal class TerritoryRewardPanelController_OnNewTerritorySelected
    {
        public static void Prefix(TerritoryDetailsUIBuilder __instance)
        {
            try
            {
                Plugin.Log.LogDebug("TerritoryRewardPanelController_OnNewTerritorySelected Prefix");

                TerritoryInstance territory = __instance.InspectedTerritoryInstance;
                //needed for ItemRewardGridController_RefreshItemDisplay
                Operations.SelectedTerritory = territory;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
        public static void Postfix(TextMeshProUGUI ___tokenText, TextMeshProUGUI ___techText)
        {
            try
            {
                Plugin.Log.LogDebug("TerritoryRewardPanelController_OnNewTerritorySelected Postfix");

                if (!Operations.ShowEnemies())
                {
                    ___tokenText.text = string.Empty;
                    ___techText.text = string.Empty;
                }

                Operations.SelectedTerritory = null;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
