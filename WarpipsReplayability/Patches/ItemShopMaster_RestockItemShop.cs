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

                //note that if DifficultMode is true, this fix is irrelevant due to the underlying code fix in Reward_SetStackCount
                //if (Config.FixArmsDealer)
                foreach (var r in __instance.ItemShopStock)
                {
                    Plugin.Log.LogInfo($"{r.item.name} - {r.stackSize}, cost {r.item.techNode.purchaseCost}");

                    //if (r.stackCount > 1)
                    //    Plugin.Log.LogInfo($"{r.stackCount}");
                    //if (r.item.name == "Marine")
                    //    r.stackCount = 2;

                    r.stackCount = Plugin.Rand.RangeInt(r.stackSize.x, r.stackSize.y);
                }

                int saleIndex = __instance.SaleIndex;
                Plugin.Log.LogInfo("Set SaleIndex " + saleIndex);
                if (Persist.Instance != null)
                    Persist.Instance.SaleIndex = saleIndex;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
