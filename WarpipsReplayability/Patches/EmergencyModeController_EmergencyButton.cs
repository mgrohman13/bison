//using GameUI;
//using HarmonyLib;
//using System;
//using WarpipsReplayability.Mod;

//namespace WarpipsReplayability.Patches
//{
//    [HarmonyPatch(typeof(EmergencyModeController))]
//    [HarmonyPatch(nameof(EmergencyModeController.EmergencyButton))]
//    internal class EmergencyModeController_EmergencyButton
//    {
//        public static void Prefix(FloatReadonlyStat ___invalidSupplyRevokeDuration)
//        {
//            try
//            {
//                Plugin.Log.LogDebug("EmergencyModeController_EmergencyButton Prefix: " + ___invalidSupplyRevokeDuration.Value);
//            }
//            catch (Exception e)
//            {
//                Plugin.Log.LogError(e);
//            }
//        }
//    }
//}
