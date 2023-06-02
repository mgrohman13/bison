using HarmonyLib;
using System;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(ResourceController))]
    [HarmonyPatch(nameof(ResourceController.TryAddPlayerLife))]
    internal class ResourceController_TryAddPlayerLife
    {
        public const int BonusMaxLifeTokens = 2;

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
                    __instance.AddTokens(BonusMaxLifeTokens);
                    Plugin.Log.LogInfo($"BonusMaxLifeTokens: {BonusMaxLifeTokens}");
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
