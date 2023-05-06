using GameIO;
using GameUI;
using HarmonyLib;
using LevelGeneration;
using LevelGeneration.WorldMap;
using System;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(MapCycleElementController))]
    [HarmonyPatch("StartWrapper")]
    internal class MapCycleElementController_StartWrapper
    {
        public static bool Prefix(MapCycleElementController __instance, ref int ___mapIndex)
        {
            try
            {
                Plugin.Log.LogInfo("MapCycleElementController_StartWrapper Prefix");

                //there is a bug in the base game when you lose an operation
                //MapCycleElementController mistakenly sets the map of the failed operation to the first map (mapIndex 0)
                //I utilize this bug to set it to a random map
                if (Operations.FailedMission != null)
                {
                    ___mapIndex = -1;
                    __instance.RandomMap();

                    //also randomize StartAtDifficulty and difficultyCurve
                    Operations.RandOnLoss();

                    return true;
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
            return false;
        }
    }
}
