using GameUI;
using HarmonyLib;
using System;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(CooldownController))]
    [HarmonyPatch(nameof(CooldownController.StartCooldown))]
    internal class CooldownController_StartCooldown
    {
        private static string LastLog;
        public static void Prefix(float duration)
        {
            try
            {
                Plugin.Log.LogDebug("CooldownController_StartCooldown Prefix");

                string log = $"StartCooldown {duration}";
                if (LastLog != log)
                {
                    LastLog = log;
                    Plugin.Log.LogInfo(log);
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
