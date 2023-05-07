using GameUI;
using HarmonyLib;
using LevelGeneration;
using System;
using UnityEngine;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(TerritoryDetailsUIBuilder))]
    [HarmonyPatch(nameof(TerritoryDetailsUIBuilder.OnNewTerritorySelected))]
    internal class TerritoryDetailsUIBuilder_OnNewTerritorySelected
    {
        public static void Prefix(TerritoryDetailsUIBuilder __instance)
        {
            try
            {
                Plugin.Log.LogDebug("TerritoryDetailsUIBuilder_OnNewTerritorySelected Prefix");

                //needed for ItemRewardGridController_RefreshItemDisplay
                LevelGeneration.WorldMap.TerritoryInstance territory = __instance.InspectedTerritoryInstance;
                Operations.SelectedTerritory = territory;

                LevelGeneration.WorldMap.Operation operation = territory.operation;
                SpawnWaveProfile spawnWaveProfile = operation.spawnWaveProfile;
                Plugin.Log.LogInfo($"select {territory.index} {spawnWaveProfile.name} ({spawnWaveProfile.GetInstanceID()} {operation.map.MapLength})");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
        public static void Postfix(TerritoryDetailsUIBuilder __instance, ref ReconNodeController[] ___unitIntel, RectTransform ___rootRect)
        {
            try
            {
                Plugin.Log.LogDebug("TerritoryDetailsUIBuilder_OnNewTerritorySelected Postfix");

                if (!Operations.ShowEnemies(__instance.InspectedTerritoryInstance))
                {
                    bool visible = true;
                    foreach (var recon in ___unitIntel)
                    {
                        recon.SetVisiblityState(visible);
                        if (visible)
                            recon.SetToMysteryIcon();
                        visible = false;
                    }
                    ___rootRect.ForceUpdateRectTransforms();

                    Plugin.Log.LogDebug("hiding recon");
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
