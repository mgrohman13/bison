using HarmonyLib;
using System;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(ResourceController))]
    [HarmonyPatch(nameof(ResourceController.TryAddPlayerLife))]
    internal class ResourceController_TryAddPlayerLife
    {
        public static void Prefix(ResourceController __instance, ref int __state)
        {
            try
            {
                Plugin.Log.LogDebug("ResourceController_TryAddPlayerLife Prefix");

                __state = __instance.PlayerLives.value;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
        public static void Postfix(ResourceController __instance, int __state)
        {
            try
            {
                Plugin.Log.LogDebug("ResourceController_TryAddPlayerLife Postfix");

                if (__state == __instance.PlayerLives.value)
                {
                    int tokens = 2;
                    __instance.AddTokens(tokens);
                    Plugin.Log.LogInfo($"Bonus max life tokens: {tokens}");
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
