using HarmonyLib;
using System;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(DefaultResourceSetter))]
    [HarmonyPatch(nameof(DefaultResourceSetter.SetNewConquestModeResources))]
    internal class DefaultResourceSetter_SetNewConquestModeResources
    {
        public static void Postfix(ResourceController ___resourceController)
        {
            try
            {
                Plugin.Log.LogDebug("DefaultResourceSetter_SetNewConquestModeResources Postfix");

                int value = 3;
                int difficulty = Map.MissionManagerAsset.GameDifficultyIndex;
                if (difficulty == 3 || difficulty == 1)
                    value++;

                ___resourceController.HardSetPlayerLives(value, value);
                Plugin.Log.LogInfo($"HardSetPlayerLives to {value}");

                ___resourceController.SetTokens(value);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
