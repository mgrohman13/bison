//using HarmonyLib;
//using LevelGeneration;
//using System;
//using WarpipsReplayability.Mod;

//namespace WarpipsReplayability.Patches
//{
//    [HarmonyPatch(typeof(Reward))]
//    [HarmonyPatch(nameof(Reward.SetStackCount))]
//    internal class Reward_SetStackCount
//    {
//        public static bool Prefix(Reward __instance)
//        {
//            try
//            {
//                Plugin.Log.LogDebug("Reward_SetStackCount Prefix");

//                if (Config.DifficultMode)
//                {
//                    __instance.stackCount = Plugin.Rand.RangeInt(__instance.stackSize.x, __instance.stackSize.y);
//                    Plugin.Log.LogDebug($"{__instance.item.name}: {__instance.stackCount} ({__instance.stackSize.x}-{__instance.stackSize.y})");
//                    return false;
//                }
//            }
//            catch (Exception e)
//            {
//                Plugin.Log.LogError(e);
//            }
//            return true;
//        }
//    }
//}
