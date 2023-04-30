using GameUI;
using HarmonyLib;
using System;
using WarpipsReplayability.Mod;

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

                Plugin.Log.LogDebug($"select {Operations.SelectedTerritory.operation.spawnWaveProfile.name}");
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
