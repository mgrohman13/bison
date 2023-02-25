using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using GameUI;
using System.Runtime.CompilerServices;
using LevelGeneration.WorldMap;
using LevelGeneration;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(ExtraLifeItemRewardController))]
    [HarmonyPatch(nameof(ExtraLifeItemRewardController.TryPushExtraLife))]
    internal class ExtraLifeItemRewardController_TryPushExtraLife
    {
        private static int lives = -1;

        public static void Prefix(ref ResourceController ___resourceController)
        {
            try
            {
                Plugin.Log.LogInfo("ExtraLifeItemRewardController_TryPushExtraLife Prefix");

                lives = ___resourceController.PlayerLives.value;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
        public static void Postfix(ref ResourceController ___resourceController)
        {
            try
            {
                Plugin.Log.LogInfo("ExtraLifeItemRewardController_TryPushExtraLife Postfix");

                if (lives == ___resourceController.PlayerLives.value)
                {
                    int points = Plugin.Rand.RangeInt(6, 9);
                    ___resourceController.AddTechPoints(points);
                    lives = -1;

                    Plugin.Log.LogInfo($"Bonus max life tech points {points}");
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
