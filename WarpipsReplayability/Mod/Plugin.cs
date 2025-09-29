using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WarpipsReplayability.Mod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Warpips.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static MTRandom Rand { get; private set; }
        public static ManualLogSource Log { get; private set; }

        //tiers for difficulty bar warnings, outer array in order of difficulty (high -> low)
        public static readonly HashSet<string>[] DifficultTechs = [
            ["Hind", "Rocket",],
            ["Bubba", "Predator", "T92",],
            ["Tanya", "Gruz", "DuneBuggy",],
            ["RPGSoldier", "GasPip", "Sharpshooter",],
        ];
        //tech types that can be hidden from a full panel of enemies
        public static readonly HashSet<string> WeakTechs =
            ["Warmule", "UAZ", "Shotgunner", "Warfighter", "PistolPip",];
        //ensure every operation picks at least one primary attack unit
        public static readonly HashSet<string> PrimaryTechs = [.. WeakTechs,
            "T92", "Gruz", "DuneBuggy",];
        //especially impactful tech types, in order of impact (high -> low)
        public static readonly string[] HeroTechs =
            ["Hind", "Bubba", "Rocket", "Predator", "Tanya",];

        public void Awake()
        {
            Rand = new MTRandom();
            Rand.StartTick();
            Log = Logger;

            Harmony harmony = new("WarpipsReplayability.mod");
            harmony.PatchAll();

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Log.LogDebug(MTRandom.GAUSSIAN_FLOAT_MAX);
            Log.LogDebug(MTRandom.GetOEFlaotMax());
        }

        public static string GetSeedString(uint[] seed) =>
            seed.Select(s => s.ToString("X")
                    .Trim('0').PadLeft(1, '0')
                ).Aggregate("", (a, b) => a + b);

        public static void LogAtLevel(string data, bool warning) =>
            ((Action<object>)(warning ? Log.LogWarning : Log.LogInfo))(data);
    }
}
