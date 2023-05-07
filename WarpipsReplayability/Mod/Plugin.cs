using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MattUtil;
using System.Linq;

namespace WarpipsReplayability.Mod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Warpips.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static MTRandom Rand { get; private set; }
        public static ManualLogSource Log { get; private set; }

        public void Awake()
        {
            Rand = new MTRandom();
            Rand.StartTick();
            Log = Logger;

            Harmony harmony = new("WarpipsReplayability.mod");
            harmony.PatchAll();

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        public static string GetSeedString(uint[] seed) =>
            seed.Select(s => s.ToString("X").Trim('0')).Aggregate("", (a, b) => a + b);
    }
}
