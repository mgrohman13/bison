using HarmonyLib;
using System;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(ItemShopMaster))]
    [HarmonyPatch(nameof(ItemShopMaster.RestockItemShop))]
    internal class ItemShopMaster_RestockItemShop
    {
        public static void Postfix(ItemShopMaster __instance)
        {
            try
            {
                Plugin.Log.LogDebug("ItemShopMaster_RestockItemShop Postfix");

                ////note that if DifficultMode is true, this fix is irrelevant due to the underlying code fix in Reward_SetStackCount
                //if (Config.FixArmsDealer)
                foreach (var r in __instance.ItemShopStock)
                {
                    r.stackCount = Plugin.Rand.RangeInt(r.stackSize.x, r.stackSize.y);
                    Plugin.Log.LogInfo($"{r.item.name}: {r.stackCount} ({r.stackSize.x}-{r.stackSize.y}), cost {r.item.techNode.purchaseCost}");
                }

                int saleIndex = __instance.SaleIndex;
                Plugin.Log.LogInfo("Set SaleIndex " + saleIndex);
                Persist.Instance.SaleIndex = saleIndex;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
