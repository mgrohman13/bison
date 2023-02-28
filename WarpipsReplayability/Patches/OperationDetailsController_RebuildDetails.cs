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

                Operations.SelectedTerritory = ___territoryInspector.InspectedTerritoryInstance;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
        public static void Postfix(TerritoryInspectorController ___territoryInspector)
        {
            try
            {
                Plugin.Log.LogDebug("OperationDetailsController_RebuildDetails Postfix");

                Operations.SelectedTerritory = null;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
