using GameUI;
using HarmonyLib;
using LevelGeneration;
using LevelGeneration.WorldMap;
using System;
using System.Linq;
using System.Reflection;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(MainMenuController))]
    [HarmonyPatch("NewCampaignCommon")]
    internal class MainMenuController_NewCampaignCommon
    {
        private static readonly FieldInfo _territoryConnections = AccessTools.Field(typeof(WorldMapAsset), "territoryConnections");

        public static void Prefix()
        {
            try
            {
                Plugin.Log.LogDebug("MainMenuController_NewCampaignCommon Prefix");

                var originalConnections = Map.OriginalConnections;
                if (Map.WorldMapAsset != null && originalConnections != null)
                {
                    _territoryConnections.SetValue(Map.WorldMapAsset, originalConnections);
                    Plugin.Log.LogInfo("restored connections " + originalConnections.Sum(c => c.connection.Count) / 2);
                }

                Map.DoShuffle = true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }

        public static void Postfix()
        {
            try
            {
                Plugin.Log.LogDebug("MainMenuController_NewCampaignCommon Postfix");

                //save SaleIndex
                Persist.SaveCurrent();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
