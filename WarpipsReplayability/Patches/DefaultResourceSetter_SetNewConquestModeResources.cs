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

                if (Config.PlayerLives.HasValue)
                {
                    int value = Config.PlayerLives.Value;
                    ___resourceController.HardSetPlayerLives(value, value);
                    Plugin.Log.LogInfo($"HardSetPlayerLives to {value}");
                }

                ___resourceController.SetTokens(3);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
