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
//    [HarmonyPatch("AssignBaselineData")]
//    internal class Operation_AssignBaselineData
//    {
//        public static void Postfix()
//        {
//            try
//            {
//                Plugin.Log.LogInfo("Operation_AssignBaselineData Postfix");


//            }
//            catch (Exception e)
//            {
//                Plugin.Log.LogError(e);
//            }
//        }
//    }
//}
