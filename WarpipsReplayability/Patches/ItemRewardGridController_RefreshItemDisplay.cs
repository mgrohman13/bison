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
        public static void Postfix(ItemRewardController[] ___itemRewards, Operation inspectedOperation)
        {
            try
            {
                Plugin.Log.LogDebug("ItemRewardGridController_RefreshItemDisplay Postfix");

                if (!Operations.ShowRewards())
                {
                    List<Reward> itemRewards = inspectedOperation.itemRewards;
                    for (int a = 0; a < ___itemRewards.Length; a++)
                        if (a >= itemRewards.Count || !itemRewards[a].item.extraLife)
                        {
                            ItemRewardController controller = ___itemRewards[a];
                            controller.gameObject.SetActive(true);
                            controller.SetImageToMysteryItem();
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
