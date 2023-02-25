//using System;
//using System.Collections.Generic;
//using System.Linq;
//using BepInEx;
//using HarmonyLib;
//using GameUI;
//using System.Runtime.CompilerServices;
//using LevelGeneration.WorldMap;
//using LevelGeneration;

//namespace WarpipsReplayability.Patches
//{
//    [HarmonyPatch(typeof(Operation))]
//    [HarmonyPatch(nameof(Operation.RestoreOperationState))]
//    internal class Operation_RestoreOperationState
//    {
//        public static void Prefix()
//        {
//            try
//            {
//                Plugin.Log.LogInfo("Operation_RestoreOperationState Prefix");


//            }
//            catch (Exception e)
//            {
//                Plugin.Log.LogError(e);
//            }
//        }
//    }
//}
