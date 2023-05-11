using HarmonyLib;
using LevelGeneration;
using System;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(Reward))]
    [HarmonyPatch(nameof(Reward.SetStackCount))]
    internal class Reward_SetStackCount
    {
        public static void Postfix(Reward __instance)
        {
            try
            {
                Plugin.Log.LogDebug("Reward_SetStackCount Postfix");

                //if (Config.DifficultMode)
                //{
                //    __instance.stackCount = Plugin.Rand.RangeInt(__instance.stackSize.x, __instance.stackSize.y);
                Plugin.Log.LogInfo($"{__instance.item.name}: {__instance.stackCount} ({__instance.stackSize.x}-{__instance.stackSize.y})");
                //    return false;
                //}
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
            //return true;
        }
    }
}
