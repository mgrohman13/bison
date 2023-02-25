//using System;
//using System.Collections.Generic;
//using System.Linq;
//using BepInEx;
//using HarmonyLib;
//using GameUI;
//using System.Runtime.CompilerServices;
//using LevelGeneration.WorldMap;

//namespace WarpipsReplayability.Patches
//{
//    [HarmonyPatch(typeof(SaveLoadMaster))]
//    [HarmonyPatch(nameof(SaveLoadMaster.LoadGameData))]
//    internal class SaveLoadMaster_LoadGameData
//    {
//        public static void Postfix()
//        {
//            try
//            {
//                Plugin.Log.LogInfo("SaveLoadMaster_LoadGameData Postfix");

//                Map.Load();
//            }
//            catch (Exception e)
//            {
//                Plugin.Log.LogError(e);
//            }
//        }
//    }
//}
