using System.IO;
using System.Linq;
using System.Collections.Generic;
using LeaviathanProwl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oculus.Newtonsoft.Json;

namespace LeaviathanProwlTests
{
    [TestClass]
    public class RecipeInformationParsingTests
    {
        private const string path = "../../../LeaviathanProwl/";

        [TestMethod]
        public void TestDeserialize()
        {
            Config.Load(path);
            Assert.IsTrue(Config.allBiomes.Count > 50);
            Assert.IsTrue(Config.biomeConfig.Count > 10);
        }

        [TestMethod]
        public void TestGenerateAllBiomes()
        {
            string path = RecipeInformationParsingTests.path + "AllBiomes.json";
            if (!File.Exists(path))
            {
                var config = new SortedDictionary<string, bool>();
                foreach (BiomeType biome in typeof(BiomeType).GetEnumValues())
                    config.Add(biome.AsString(), true);

                File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
            }
        }

        [TestMethod]
        public void TestGenerateBiomeConfig()
        {
            string path = RecipeInformationParsingTests.path + "BiomeConfig.json";
            if (!File.Exists(path))
            {
                var config = new SortedDictionary<string, Config.BiomeSpawn>();
                foreach (BiomeType biome in typeof(BiomeType).GetEnumValues())
                    config[Config.GetBiome(biome)] = new Config.BiomeSpawn() { reaperRate = 1, ghostRate = 1, ghostJuvenile = false, };

                File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
            }
        }
    }
}
