//using HarmonyLib;
//using System;
//using WarpipsReplayability.Mod;

//namespace WarpipsReplayability.Patches
//{
//    [HarmonyPatch(typeof(ResourceController))]
//    [HarmonyPatch(nameof(ResourceController.SetCash))]
//    internal class ResourceController_SetCash
//    {
//        public static void Prefix(ref int newValue)
//        {
//            try
//            {
//                Plugin.Log.LogDebug("ResourceController_SetCash Prefix");

//                if (Config.DifficultMode)
//                    if (Map.MissionManagerAsset != null)
//                        newValue = 5 * Plugin.Rand.GaussianOEInt((40 + 5 * Map.MissionManagerAsset.WorldMapIndex) / 3f, .169f, .078f);

//                //Map.MissionManagerAsset.WorldMapIndex switch
//                //        {
//                //            0 => 40,
//                //            1 => 60,
//                //            2 => 80,
//                //            3 => 100,
//                //            _ => throw new Exception($"Map.MissionManagerAsset.WorldMapIndex {Map.MissionManagerAsset.WorldMapIndex}"),
//                //        };
//            }
//            catch (Exception e)
//            {
//                Plugin.Log.LogError(e);
//            }
//        }
//    }
//}
