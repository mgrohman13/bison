using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MattUtil;
using Oculus.Newtonsoft.Json;

namespace LeaviathanProwl
{
    public class LeviathanPool
    {
        public static LeviathanPool instance = new LeviathanPool();

        [JsonIgnore]
        private readonly Dictionary<Creature, LeviathanInfo> active = new Dictionary<Creature, LeviathanInfo>();
        [JsonIgnore]
        private double logTime = 0;

        [JsonProperty]
        private double lastSpawnTime = -1, reaperTimeMult, ghostTimeMult;
        [JsonProperty]
        private BiomeType lastBiomeType = BiomeType.Unassigned;
        [JsonProperty]
        private string lastBiomeString = null;

        public void Add(Creature creature)
        {
            Logger.LogInfo(creature, "Add");

            LeviathanInfo info = new LeviathanInfo(creature);
            active.Add(info.creature, info);
            RefreshPool();
            Logger.LogInfo("count {0}", active.Count);

            if (active.Count > Config.rand.GaussianOEInt(Config.difficulty.activeCount * GetRate((info.creature is ReaperLeviathan), 1), .2f, .25f))
                info.KillCreature();

            SetSpawnTime();
        }

        public void Remove(Creature creature)
        {
            Logger.LogInfo(creature, "Remove");

            active.Remove(creature);
            RefreshPool();
            Logger.LogInfo("count {0}", active.Count);
        }

        internal void Update(Creature instance)
        {
            RefreshPool();

            if (LeviathanInfo.IsActive(instance))
            {
                if (!active.ContainsKey(instance))
                    Add(instance);
                active[instance].Update();
            }
        }

        //private static bool flag = false;
        private static HashSet<String> biomes2 = new HashSet<string>();
        public TechType DoSpawn(BiomeType biome, bool isNew)
        {
            //Logger.LogInfo("DoSpawn {0}", biome);

            // TODO: better way of getting biome?
            if (biome != BiomeType.Unassigned)
            {
                lastBiomeType = biome;
                lastBiomeString = null;
            }
            if (lastSpawnTime == -1)
                SetSpawnTime();

            RefreshPool();

            TechType tech = TechType.None;
            if (active.Count == 0 && Config.rand.Bool())
                if (GetRate(true, 0) > 0 && CheckSpawnTime(true, isNew))
                    tech = TechType.ReaperLeviathan;
                else if (GetRate(false, 0, out Config.BiomeSpawn b) > 0 && CheckSpawnTime(false, isNew))
                    tech = (b.ghostJuvenile ? TechType.GhostLeviathanJuvenile : TechType.GhostLeviathan);

            if (DayNightCycle.main.timePassed > logTime)
            {
                Logger.LogInfo("timePassed {0}, reaper {1}, ghost {2}", DayNightCycle.main.timePassed, GetSpawnTime(true, isNew), GetSpawnTime(false, isNew));
                Logger.LogInfo("lastBiomeType {0}, lastBiomeString {1}", lastBiomeType, lastBiomeString);
                logTime = DayNightCycle.main.timePassed + 60;

                //if (flag && biomes2.Count == 0)
                //{
                //    Int3 size = LargeWorldStreamer.main.worldSize;
                //    Logger.LogInfo(size.ToString());
                //    for (int x = 0; x < size.x; ++x)
                //    {
                //        Logger.LogInfo("x: {0}", x);
                //        for (int y = 0; y < size.y; ++y)
                //            for (int z = 0; z < size.z; ++z)
                //                biomes2.Add(LargeWorld.main.GetBiome(new Int3(x, y, z)));
                //    }
                //    foreach (string b in biomes2)
                //        Logger.LogInfo(b);
                //}
                //flag = true;
            }

            if (tech != TechType.None)
                SetSpawnTime(.02);

            return tech;
        }

        private static readonly HashSet<string> biomes = new HashSet<string>();
        public void SetBiome(string biome)
        {
            if (!string.IsNullOrWhiteSpace(biome))
            {
                if (!biomes.Contains(biome))
                {
                    biomes.Add(biome);

                    Logger.LogInfo("");
                    Logger.LogInfo("biomes:");
                    foreach (string b in biomes)
                        Logger.LogInfo(b);
                    Logger.LogInfo("");
                }

                string newBiome = Config.GetBiome(biome);
                if (lastBiomeString != newBiome)
                {
                    Logger.LogInfo("new biome {0}", newBiome);
                    lastBiomeString = newBiome;
                }
            }
        }

        private void RefreshPool()
        {
            foreach (var pair in active.ToList())
                if (!LeviathanInfo.IsActive(pair.Key))
                    Remove(pair.Key);
        }

        private void SetSpawnTime(double value = 1)
        {
            lastSpawnTime = DayNightCycle.main.timePassed;
            double randTime() => Config.rand.GaussianOE(value, .35, .5);
            reaperTimeMult = randTime();
            ghostTimeMult = randTime();
            Logger.LogInfo("SetSpawnTime {0}, reaper {1}, ghost {2}", lastSpawnTime, reaperTimeMult, ghostTimeMult);
        }

        private bool CheckSpawnTime(bool isReaper, bool isNew)
        {
            return (DayNightCycle.main.timePassed > GetSpawnTime(isReaper, isNew));
        }

        private double GetSpawnTime(bool isReaper, bool isNew)
        {
            float mult = (isNew ? .75f : 1.25f);
            return lastSpawnTime + mult * Config.difficulty.spawnTime * (isReaper ? reaperTimeMult : ghostTimeMult) / GetRate(isReaper, 1);
        }

        private float GetRate(bool isReaper, float otherwise)
        {
            return GetRate(isReaper, otherwise, out _);
        }
        private float GetRate(bool isReaper, float otherwise, out Config.BiomeSpawn b)
        {
            float rate = 0;
            if (Config.GetBiome(lastBiomeType, lastBiomeString, out b))
                rate = (isReaper ? b.reaperRate : b.ghostRate);
            if (rate <= 0)
                rate = otherwise;
            return rate;
        }
    }
}
