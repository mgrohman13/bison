using HarmonyLib;
using LevelGeneration;
using System;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(DefaultResourceSetter))]
    [HarmonyPatch(nameof(DefaultResourceSetter.SetNewConquestModeResources))]
    internal class DefaultResourceSetter_SetNewConquestModeResources
    {
        public static void Postfix(MissionManagerAsset ___missionManagerAsset, ResourceController ___resourceController)
        {
            try
            {
                Plugin.Log.LogDebug("DefaultResourceSetter_SetNewConquestModeResources Postfix");

                int value = 3;
                int difficulty = ___missionManagerAsset.GameDifficultyIndex;
                if (difficulty == 3 || difficulty == 1)
                    value++;

                ___resourceController.HardSetPlayerLives(value, value);
                ___resourceController.SetTokens(value);
                Plugin.Log.LogInfo($"HardSetPlayerLives to {value}");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
