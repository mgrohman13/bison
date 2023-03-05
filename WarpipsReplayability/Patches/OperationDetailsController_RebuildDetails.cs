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
        public static void Postfix(TerritoryInspectorController ___territoryInspector, TextMeshProUGUI ___mapLength, Image ___mapIcon, TextMeshProUGUI ___roundDuration)
        {
            try
            {
                Plugin.Log.LogDebug("OperationDetailsController_RebuildDetails Postfix");

                if (!Operations.ShowEnemies())
                {
                    ___mapLength.text = string.Empty;
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
