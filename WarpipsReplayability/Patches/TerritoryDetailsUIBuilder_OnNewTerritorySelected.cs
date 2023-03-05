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
using UnityEngine;
using static UnityEngine.Random;
using System.Reflection;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(TerritoryDetailsUIBuilder))]
    [HarmonyPatch(nameof(TerritoryDetailsUIBuilder.OnNewTerritorySelected))]
    internal class TerritoryDetailsUIBuilder_OnNewTerritorySelected
    {
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
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
