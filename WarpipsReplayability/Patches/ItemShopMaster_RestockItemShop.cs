﻿using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using GameUI;
using System.Runtime.CompilerServices;
using WarpipsReplayability.Mod;
using LevelGeneration;
using System.Drawing.Printing;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(ItemShopMaster))]
    [HarmonyPatch(nameof(ItemShopMaster.RestockItemShop))]
    internal class ItemShopMaster_RestockItemShop
    {
        public static void Postfix(Reward[] ___itemShopStock, FloatReadonlyStat ___saleDiscountValue)
        {
            try
            {
                Plugin.Log.LogDebug("ItemShopMaster_RestockItemShop Postfix");
                Plugin.Log.LogDebug($"{___saleDiscountValue.Value}");

                //note that if DifficultMode is true, this fix is irrelevant due to the underlying code fix in Reward_SetStackCount
                if (Config.FixArmsDealer)
                    foreach (var r in ___itemShopStock)
                    {
                        Plugin.Log.LogDebug($"{r.item.name} - {r.stackSize}, cost {r.item.techNode.purchaseCost}");

                        //if (r.stackCount > 1)
                        //    Plugin.Log.LogInfo($"{r.stackCount}");
                        //if (r.item.name == "Marine")
                        //    r.stackCount = 2;

                        r.stackCount = Plugin.Rand.RangeInt(r.stackSize.x, r.stackSize.y);
                    }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
