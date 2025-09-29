using HarmonyLib;
using System;
using System.Reflection;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(ItemShopMaster))]
    [HarmonyPatch(nameof(ItemShopMaster.SetItemShopStock))]
    internal class ItemShopMaster_SetItemShopStock
    {
        private static readonly MethodInfo _saleIndex = AccessTools.PropertySetter(typeof(ItemShopMaster), "SaleIndex");

        public static void Postfix(ItemShopMaster __instance)
        {
            try
            {
                Plugin.Log.LogDebug("ItemShopMaster_SetItemShopStock Postfix");

                int saleIndex = Persist.Instance.SaleIndex;
                Plugin.Log.LogInfo($"loaded SaleIndex {__instance.SaleIndex} -> {saleIndex}");
                _saleIndex.Invoke(__instance, [saleIndex]);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
