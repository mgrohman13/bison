using GameUI;
using HarmonyLib;
using LevelGeneration;
using System;
using System.Collections.Generic;
using WarpipsReplayability.Mod;

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

                if (!Operations.ShowRewardCount())
                {
                    //hide the total number of rewards
                    List<Reward> itemRewards = Operations.SelectedTerritory.operation.itemRewards;
                    for (int a = 0; a < ___itemRewards.Length; a++)
                        if (a >= itemRewards.Count || !itemRewards[a].item.extraLife)
                        {
                            ItemRewardController controller = ___itemRewards[a];
                            controller.gameObject.SetActive(true);
                            controller.SetImageToMysteryItem();
                        }
                    Plugin.Log.LogDebug($"hiding rewards {___itemRewards.Length} {itemRewards.Count}");
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
