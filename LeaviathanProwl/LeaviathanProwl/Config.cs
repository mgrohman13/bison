using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MattUtil;
using Oculus.Newtonsoft.Json;

namespace LeaviathanProwl
{
    public class Config
    {
        public const string path = "./QMods/LeaviathanProwl/";

        public static readonly MTRandom rand = new MTRandom();
        static Config()
        {
            rand.StartTick();
        }

        public static Difficulty difficulty;
        public static Dictionary<string, BiomeSpawn> biomeConfig = new Dictionary<string, BiomeSpawn>();
        public static readonly Dictionary<BiomeType, BiomeSpawn> allBiomes = new Dictionary<BiomeType, BiomeSpawn>();

        public static void Load(string path = path)
        {
            difficulty = JsonConvert.DeserializeObject<Difficulty>(File.ReadAllText(path + "Difficulty.json"));
            biomeConfig.Clear();
            allBiomes.Clear();

            var all = JsonConvert.DeserializeObject<Dictionary<BiomeType, bool>>(File.ReadAllText(path + "AllBiomes.json"));
            var config = JsonConvert.DeserializeObject<Dictionary<string, BiomeSpawn>>(File.ReadAllText(path + "BiomeConfig.json"));
            foreach (var pair in all)
                if (pair.Value)
                {
                    string biomeStr = GetBiome(pair.Key);
                    BiomeSpawn b = config[biomeStr];
                    if (b.reaperRate > 0 || b.ghostRate > 0)
                    {
                        biomeConfig[biomeStr] = b;
                        allBiomes.Add(pair.Key, b);
                    }
                }

            Logger.LogAllFields(difficulty);
            Logger.LogInfo("biomeConfig: {0}", biomeConfig.Count);
            Logger.LogInfo("allBiomes: {0}", allBiomes.Count);

            //foreach (var pair in config)
            //{
            //    Logger.LogInfo(pair.Key.ToString());
            //    Logger.LogAllFields(pair.Value);
            //}
        }

        public static string GetBiome(BiomeType biome)
        {
            string name = biome.AsString().ToLower();
            const string fragment = "fragment";
            if (name.StartsWith(fragment))
                return fragment;
            return name.Split('_')[0];
        }

        internal static bool GetBiome(BiomeType lastBiomeType, string lastBiomeString, out BiomeSpawn b)
        {
            if (!string.IsNullOrWhiteSpace(lastBiomeString))
                return biomeConfig.TryGetValue(lastBiomeString, out b);
            return allBiomes.TryGetValue(lastBiomeType, out b);
        }

        private static HashSet<string> biomeSet;
        private static int c1 = -1, c2 = -1;
        public static string GetBiome(string biome)
        {
            if (LargeWorldStreamer.main.batch2root.Count > c1)
            {
                c1 = LargeWorldStreamer.main.batch2root.Count;
                Logger.LogInfo("batch2root {0}", c1);
                HashSet<string> temp = new HashSet<string>();
                foreach (var p in LargeWorldStreamer.main.batch2root)
                    temp.Add(p.Value.overrideBiome);
                if (temp.Count > c2)
                {
                    c2 = temp.Count;
                    Logger.LogInfo("overrideBiomes {0}", c2);
                    foreach (var s in temp)
                        Logger.LogInfo(s);
                }
            }

            if (biomeSet == null)
            {
                biomeSet = new HashSet<string>();
                foreach (BiomeType b in typeof(BiomeType).GetEnumValues())
                    biomeSet.Add(GetBiome(b));
            }

            const string lostRiver = "lostriver_";
            Dictionary<string, string> replacements = new Dictionary<string, string>() {
                { "bloodkelp", "bloodkelp" },
                { "corridor", "lostrivercorridor" },
                { "grassyplateaus", "grassyplateaus" },
                { "ilz", "inactivelavazone" },
                { "junction", "lostriverjunction" },
                { "kelpforest", "kelp" },
                { "lava", "activelavazone" },
                { "lifepod", "safeshallows" },
                { "crashedship", "unassigned" },
                { "generatorroom", "unassigned" },
                { "mountains_island", "unassigned" },
                { "observatory", "unassigned" },
                { "precursor", "unassigned" },
                { "prison", "unassigned" },
                { "void", "unassigned" },
                { "<unknown>", "unassigned" },
            };

            string result = biome.ToLower();
            if (result.StartsWith(lostRiver))
                result = result.Substring(lostRiver.Length);
            foreach (var pair in replacements)
                if (result.StartsWith(pair.Key))
                {
                    result = pair.Value;
                    break;
                }
            result = result.Split('_')[0];

            if (!biomeSet.Contains(result))
                Logger.LogInfo(true, "missing biome {0}", result);

            if (biome.Contains("cave") && !biome.Contains("jellyshroomcaves") && !biome.Contains("skeletoncave"))
            {
                Logger.LogInfo(false, "preventing cave spawn {0}", biome);
                result = null;
            }

            return result;
        }

        public class Difficulty
        {
            public float spawnTime;
            public float activeCount;
            public float leashRoam;
            public float leviathanDamage;
            public float playerDamage;
            public float vehicleDamage;

            public override string ToString()
            {
                return string.Format("spawnTime {0} activeCount {1} leashRoam {2} leviathanDamage {5} playerDamage {3} vehicleDamage {4}",
                        spawnTime, activeCount, leashRoam, playerDamage, vehicleDamage, leviathanDamage);
            }
        }

        public class BiomeSpawn
        {
            public float reaperRate;
            public float ghostRate;
            public bool ghostJuvenile;

            public override string ToString()
            {
                return string.Format("reaperRate {0} ghostRate {1} {2}", reaperRate, ghostRate, ghostJuvenile ? "(juv)" : "(adlt)");
            }
        }
    }
}
