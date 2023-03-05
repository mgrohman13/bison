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
    [HarmonyPatch(typeof(ItemRewardGridController))]
    [HarmonyPatch(nameof(ItemRewardGridController.RefreshItemDisplay))]
    internal class ItemRewardGridController_RefreshItemDisplay
    {
        public static void Postfix(ItemRewardController[] ___itemRewards)
        {
            try
            {
                Plugin.Log.LogDebug("ItemRewardGridController_RefreshItemDisplay Postfix");

                if (!Operations.ShowRewards() && Operations.SelectedTerritory.specialTag == TerritoryInstance.SpecialTag.None)
                {
                    foreach (var reward in ___itemRewards)
                    {
                        reward.gameObject.SetActive(true);
                        reward.SetImageToMysteryItem();
                    }

                    Plugin.Log.LogDebug("hiding rewards");
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
