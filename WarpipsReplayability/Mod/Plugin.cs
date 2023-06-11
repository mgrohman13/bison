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
        public static readonly HashSet<string>[] DifficultTechs = new HashSet<string>[] {
            new string[] { "Hind", "Rocket", }.ToHashSet(),
            new string[] { "Bubba", "Predator", "T92", }.ToHashSet(),
            new string[] { "Tanya", "Gruz", "DuneBuggy", }.ToHashSet(),
            new string[] { "RPGSoldier", "GasPip", "Sharpshooter", }.ToHashSet(),
        };
        //tech types that can be hidden from a full panel of enemies
        public static readonly HashSet<string> WeakTechs =
            new string[] { "Warmule", "UAZ", "Shotgunner", "Warfighter", "PistolPip", }.ToHashSet();
        //ensure every operation picks at least one primary attack unit
        public static readonly HashSet<string> PrimaryTechs = WeakTechs.Concat(
            new string[] { "T92", "Gruz", "DuneBuggy", }).ToHashSet();
        //especially impactful tech types, in order of impact (high -> low)
        public static readonly string[] HeroTechs =
            new string[] { "Hind", "Bubba", "Rocket", "Predator", "Tanya", };

        public void Awake()
        {
            Rand = new MTRandom();
            Rand.StartTick();
            Log = Logger;

            Harmony harmony = new("WarpipsReplayability.mod");
            harmony.PatchAll();

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Log.LogInfo(MTRandom.GAUSSIAN_FLOAT_MAX);
            Log.LogInfo(MTRandom.GetOEFlaotMax());
        }

        public static string GetSeedString(uint[] seed) =>
            seed.Select(s => s.ToString("X").Trim('0')).Aggregate("", (a, b) => a + b);

        public static void LogAtLevel(string data, bool warning) =>
            ((Action<object>)(warning ? Log.LogWarning : Log.LogInfo))(data);
    }
}
