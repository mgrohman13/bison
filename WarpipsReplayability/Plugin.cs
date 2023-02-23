using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Transactions;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace WarpipsReplayability
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Warpips.exe")]

    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Log = base.Logger;

            Harmony harmony = new Harmony("WarpipsReplayability.mod");
            harmony.PatchAll();

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        public static ManualLogSource Log { get; private set; }
    }
}
