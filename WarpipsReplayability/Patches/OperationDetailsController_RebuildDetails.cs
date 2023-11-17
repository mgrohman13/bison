using GameUI;
using HarmonyLib;
using System;
using TMPro;
using UnityEngine.UI;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(OperationDetailsController))]
    [HarmonyPatch(nameof(OperationDetailsController.RebuildDetails))]
    internal class OperationDetailsController_RebuildDetails
    {
        public static void Prefix(TerritoryInspectorController ___territoryInspector)
        {
            try
            {
                Plugin.Log.LogDebug("OperationDetailsController_RebuildDetails Prefix");

                //needed for DifficultyBar_BuildDifficultyBar
                Operations.SelectedTerritory = ___territoryInspector.InspectedTerritoryInstance;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
        public static void Postfix(TextMeshProUGUI ___mapLength, Image ___mapIcon, TextMeshProUGUI ___roundDuration)
        {
            try
            {
                Plugin.Log.LogDebug("OperationDetailsController_RebuildDetails Postfix");

                if (!Operations.ShowEnemies() && Operations.MapType is not null)
                {
                    ___mapLength.text = string.Empty;
                    ___mapIcon.sprite = Operations.MapType.MapOverview;
                    ___mapIcon.SetNativeSize();
                    ___mapIcon.sprite = null;
                    ___roundDuration.text = string.Empty;

                    Plugin.Log.LogDebug("hiding operation details");
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
