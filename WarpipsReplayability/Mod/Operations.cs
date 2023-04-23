using DynamicEnums;
using HarmonyLib;
using LevelGeneration;
using LevelGeneration.WorldMap;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Profiling;
using WarpipsReplayability.Patches;
using static LevelGeneration.SpawnWaveProfile;
using static LevelGeneration.WorldMap.TerritoryInstance;
using static TechTreeMaster;
using static WarpipsReplayability.Mod.Operations;

namespace WarpipsReplayability.Mod
{
    internal class Operations
    {
        public static WorldMapUIController WorldMapUIController { get; private set; }
        public static TerritoryInstance SelectedTerritory { get; set; }

        private static readonly FieldInfo spawnCountMin = AccessTools.Field(typeof(SpawnerData), "spawnCountMin");
        private static readonly FieldInfo spawnCountMax = AccessTools.Field(typeof(SpawnerData), "spawnCountMax");
        private static readonly FieldInfo spawnCapMin = AccessTools.Field(typeof(SpawnerData), "spawnCapMin");
        private static readonly FieldInfo spawnCapMax = AccessTools.Field(typeof(SpawnerData), "spawnCapMax");
        private static readonly FieldInfo startAtDifficulty = AccessTools.Field(typeof(SpawnerData), "startAtDifficulty");
        private static readonly FieldInfo spawnDelayMin = AccessTools.Field(typeof(SpawnerData), "spawnDelayMin");
        private static readonly FieldInfo spawnDelayMax = AccessTools.Field(typeof(SpawnerData), "spawnDelayMax");
        private static readonly FieldInfo cooldownAfterSpawn = AccessTools.Field(typeof(SpawnerData), "cooldownAfterSpawn");
        private static readonly FieldInfo timeBetweenClusters = AccessTools.Field(typeof(SpawnerData), "timeBetweenClusters");
        private static readonly FieldInfo difficultyCurve = AccessTools.Field(typeof(SpawnWaveProfile), "difficultyCurve");

        public static bool[] RollHiddenRewards()
        {
            var hiddenRewards = Map.Territories.SelectMany(t =>
            {
                //each territory has a variable chance for each reward to remain hidden
                int chance = Plugin.Rand.GaussianOEInt(6.5, .26, .21, 1);
                Plugin.Log.LogDebug($"hidden reward chance {chance}");
                //each reward has an individual chance to remain hidden
                return t.operation.itemRewards.Select(r => Plugin.Rand.Next(chance) == 0);
            }).ToArray();
            Plugin.Log.LogInfo($"generated HiddenRewards");
            return hiddenRewards;
        }
        public static void UpdateShroud(WorldMapUIController worldMapUIController)
        {
            WorldMapUIController = worldMapUIController;

            int rewardIndex = 0;
            foreach (TerritoryInstance territory in Map.Territories)
            {
                bool hideEnemies = HideEnemies(territory);
                bool hideRewards = HideRewards(territory) || territory.specialTag == TerritoryInstance.SpecialTag.HighValueReward;

                Operation operation = territory.operation;
                //revealEnemyIcons needs to be high enough to reveal all icons
                //the field is only used for integer comparison so it doesn't matter if it's arbitrarily large
                int revealEnemyIcons = hideEnemies ? 0 : 99;
                operation.revealEnemyIcons = revealEnemyIcons;

                foreach (Reward reward in operation.itemRewards)
                {
                    reward.isMysteryItem = (Persist.Instance.HiddenRewards[rewardIndex] || hideRewards) && !reward.item.extraLife;
                    rewardIndex++;
                }
            }
        }

        public static bool ShowRewards() =>
            ShowInfo(SelectedTerritory, HideRewards);
        public static bool ShowEnemies() =>
            ShowEnemies(SelectedTerritory);
        public static bool ShowEnemies(TerritoryInstance territory) =>
            ShowInfo(territory, HideEnemies);
        private static bool ShowInfo(TerritoryInstance territory, Func<TerritoryInstance, bool> Hide) =>
            WorldMapUIController == null || territory == null || !Hide(territory);

        private static bool IsShrouded(TerritoryInstance territory) =>
            !WorldMapUIController.IsTerritoryAttackable(territory.index);
        private static bool HideEnemies(TerritoryInstance territory) =>
            IsShrouded(territory) && territory.specialTag != TerritoryInstance.SpecialTag.EnemyObjective;
        private static bool HideRewards(TerritoryInstance territory) =>
            IsShrouded(territory);

        public static SpawnerInfo[] Randomize()
        {
            Dictionary<string, SpawnAverages> spawnAverages = AggregateSpawnInfo();
            List<SpawnerInfo> saveSpawnInfo = new();
            foreach (TerritoryInstance territory in Plugin.Rand.Iterate(Map.Territories))
                GenerateSpawnData(spawnAverages, territory, saveSpawnInfo);

            SpawnerInfo[] save = saveSpawnInfo.ToArray();
            Load(save, true);
            return save;
        }
        private static Dictionary<string, SpawnAverages> AggregateSpawnInfo()
        {
            //aggregate map spawner data 
            Dictionary<string, SpawnAverages> spawnAverages = new();
            foreach (TerritoryInstance territory in Plugin.Rand.Iterate(Map.Territories))
            {
                Plugin.Log.LogDebug(Environment.NewLine);

                SpawnWaveProfile spawnWaveProfile = territory.operation.spawnWaveProfile;
                Plugin.Log.LogDebug($"{spawnWaveProfile.name}:");
                //frequently have more than one spawner for the same tech type, group and sum their counts together
                List<EnemySpawnProfile> profiles = GetProfiles(spawnWaveProfile);
                foreach (var group in Plugin.Rand.Iterate(profiles
                        .GroupBy(enemySpawnProfile => enemySpawnProfile.UnitSpawnData.SpawnTech.name)))
                {
                    string name = group.Key;
                    if (!spawnAverages.TryGetValue(name, out SpawnAverages values))
                        spawnAverages.Add(name, values = new());

                    foreach (EnemySpawnProfile profile in Plugin.Rand.Iterate(group))
                    {
                        values.info.Add(new(territory.index, profiles.IndexOf(profile), profile));
                        SpawnerData data = profile.UnitSpawnData;
                        if (data.SpawnTech is CalldownType)
                            Plugin.Log.LogDebug($"CalldownType: {data.SpawnTech.name}");

                        int countMin = (int)spawnCountMin.GetValue(data);
                        int countMax = (int)spawnCountMax.GetValue(data);
                        int capMin = (int)spawnCapMin.GetValue(data);
                        int capMax = (int)spawnCapMax.GetValue(data);

                        Plugin.Log.LogDebug($"{profile.ReturnTechType().name} ({profile.GetInstanceID()}): {countMin}-{countMax} ({capMin}-{capMax})");

                        values.countMin += countMin;
                        values.countMax += countMax;
                        values.capMin += capMin;
                        values.capMax += capMax;
                        values.number++;
                    }
                    values.territories++;
                }
            }

            Plugin.Log.LogDebug(Environment.NewLine);
            foreach (var pair in Plugin.Rand.Iterate(spawnAverages))
            {
                SpawnAverages values = pair.Value;
                values.techType = pair.Key;

                //average per territory it is present in
                double div = values.territories;
                values.countMin /= div;
                values.countMax /= div;
                values.capMin /= div;
                values.capMax /= div;
                values.number /= div;

                Plugin.Log.LogInfo($"{pair.Key} ({values.territories}, {values.info.Count}): {values.countMin:0.00}-{values.countMax:0.00} ({values.capMin:0.00}-{values.capMax:0.00}), {values.number:0.00}");

                //pad number of territories (used for selection probability)
                values.territories++;
            }
            Plugin.Log.LogDebug(Environment.NewLine);

            return spawnAverages;
        }
        private static void GenerateSpawnData(Dictionary<string, SpawnAverages> spawnAverages, TerritoryInstance territory, List<SpawnerInfo> saveSpawnInfo)
        {
            SpawnWaveProfile spawnWaveProfile = territory.operation.spawnWaveProfile;

            ////we need to know the highest difficultyCurve values to ensure we only select units that can actually spawn
            //AnimationCurve difficultyCurve = (AnimationCurve)Operations.difficultyCurve.GetValue(spawnWaveProfile);
            //float maxDifficulty = difficultyCurve.keys.Max(k => k.value);
            //Plugin.Log.LogInfo("maxDifficulty: " + maxDifficulty);

            //only select units that can actually spawn with this difficulty curve
            //spawnAverages = spawnAverages.Where(pair => pair.Value.startAtDifficulty < maxDifficulty)
            //    .ToDictionary(pair => pair.Key, pair => pair.Value);

            foreach (var s in spawnAverages.Values)
                s.GenStartAtDifficulty();

            CountTechTypes(spawnWaveProfile, territory.specialTag, spawnAverages.Count, out int spawnTechs, out int totalTechs);

            //especially impactful tech types, in order of impact
            string[] heroTypes = new string[] { "Hind", "Bubba", "Rocket", "Predator", "Tanya" };

            //keep techTypes distinct
            HashSet<string> techTypes = new();

            if (spawnTechs > 0)
                foreach (int idx in Plugin.Rand.Iterate(spawnTechs))
                {
                    //increase spawn strength for special missions
                    double mult = territory.specialTag switch
                    {
                        SpecialTag.EnemyObjective => 2.0,
                        SpecialTag.HighValueReward => 1.5,
                        _ => 1.0,
                    };

                    SpawnAverages values = SelectTechType(spawnAverages, heroTypes, techTypes, mult);

                    //randomize spawn amounts, based on island-wide averages
                    const double deviation = .13;
                    int countMin = Plugin.Rand.GaussianCappedInt((values.countMin) * mult, deviation, values.countMin > 1 ? 1 : 0);
                    int countMax = Plugin.Rand.GaussianCappedInt((values.countMax - values.countMin + 1) * mult + countMin, deviation, countMin);
                    //if (territory.specialTag == SpecialTag.None && Plugin.Rand.Bool())
                    //{
                    //    //chance to widen range
                    //    countMax += countMin;
                    //    countMin = 0;
                    //}
                    int capMin = Plugin.Rand.GaussianCappedInt((values.capMin) * mult + countMax, deviation, countMax);
                    //minimum caps are generally quite a bit higher, so bring down the max cap somewhat
                    int capMax = Plugin.Rand.GaussianCappedInt((values.capMax - values.capMin + 2) * mult / 2.0 + capMin, deviation, capMin);

                    //because we add a little bit at each step above the averages, we need to be careful not to overdo it with certain tech types 
                    int heroIndex = Array.IndexOf(heroTypes, values.techType) + 1;
                    bool baseAlwaysOne = values.countMin == values.number && values.countMax == values.number;
                    if (heroIndex > 0 && baseAlwaysOne)
                    {
                        //allow more for higher heroIndex, difficult missions
                        mult *= values.number * heroIndex;
                        double lowMult = Math.Sqrt(mult) + Plugin.Rand.Range(0, countMin);
                        mult = Math.Max(mult + Plugin.Rand.Range(0, countMax), lowMult + 1);
                        //chance to temper down extreme values
                        int min = Math.Min(Plugin.Rand.Round(Plugin.Rand.Range(0, lowMult)), countMin);
                        int max = Math.Min(Plugin.Rand.Round(Plugin.Rand.Range(min, mult)), countMax);
                        Plugin.Log.LogInfo($"reducing {values.techType} to number: {min}-{max} ({values.number:0.00},{lowMult:0.00},{mult:0.00}), was {countMin}-{countMax}");
                        countMin = min;
                        countMax = max;
                    }
                    if (heroIndex > 0 || baseAlwaysOne)
                    {
                        Plugin.Log.LogInfo($"reducing {values.techType}, was {countMin}-{countMax} ({capMin}-{capMax})");
                        //invert heroIndex 
                        if (heroIndex > 0)
                            heroIndex = Plugin.Rand.RangeInt(0, heroTypes.Length - heroIndex + 1);
                        //increasing chance to reduce each
                        countMin = Math.Max(countMin - Plugin.Rand.RangeInt(0, ++heroIndex), 0);
                        countMax = Math.Max(countMax - Plugin.Rand.RangeInt(0, ++heroIndex), Math.Max(1, countMin));
                        capMin = Math.Max(capMin - Plugin.Rand.RangeInt(0, ++heroIndex), Math.Max(1, countMax));
                        capMax = Math.Max(capMax - Plugin.Rand.RangeInt(0, ++heroIndex), Math.Max(1, capMin));
                    }

                    SpawnerInfo copyFrom = Plugin.Rand.SelectValue(values.info);
                    SpawnerInfo info = new(copyFrom, territory.index, idx, countMin, countMax, capMin, capMax, values.startAtDifficulty);

                    //chance to hide certain techs when total is over 10, so that more build sites will show up
                    if (totalTechs > 10 && Plugin.Rand.Bool()
                            && new HashSet<string>() { "PistolPip", "Shotgunner", "Warfighter", "UAZ", "Warmule", "DuneBuggy" }.Contains(values.techType))
                    {
                        Plugin.Log.LogInfo($"hiding from recon lineup: {values.techType} ({totalTechs})");
                        info.displayInReconLineup = false;
                        totalTechs--;
                    }

                    Plugin.Log.LogInfo($"{spawnWaveProfile.name} {values.techType}: {info.countMin}-{info.countMax} ({info.capMin}-{info.capMax}), {info.difficulty:0.00}");

                    //Plugin.Log.LogInfo($"difficulty {values.techType}: {copyFrom.difficulty} ({copyFrom.profile.UnitSpawnData.StartAtDifficulty})");

                    //EnemySpawnProfile profile = info.profile;
                    //LogInfo(spawnWaveProfile, values, info, profile, profile.UnitSpawnData);

                    saveSpawnInfo.Add(info);
                }
        }
        private static void CountTechTypes(SpawnWaveProfile spawnWaveProfile, SpecialTag special, int availableTypes, out int spawnTechs, out int totalTechs)
        {
            //keep roughly the same count of tech types
            spawnTechs = GetProfileNames(spawnWaveProfile)
                .Distinct().Count();
            var allDistinctTechTypes = GetProfileNames(spawnWaveProfile)
                .Concat(spawnWaveProfile.enemyBuildSites
                   .Where(enemyBuildSite => enemyBuildSite.icon != null)
                   .Select(enemyBuildSite => TrimAfter(enemyBuildSite.name, "_")))
                .Distinct();
            totalTechs = allDistinctTechTypes.Count();
            if (totalTechs > 10)
                Plugin.Log.LogInfo(allDistinctTechTypes.Aggregate(" ", (a, b) => a + " " + b).Trim());

            bool logFlag = false;
            Plugin.Log.LogInfo($"{spawnWaveProfile.name} count: {spawnTechs} (total: {totalTechs})");

            //randomize count somewhat 
            while (spawnTechs > 1 && spawnTechs < availableTypes && Plugin.Rand.Bool()
                    && (totalTechs != 10 || Plugin.Rand.Bool())
                    && (special == SpecialTag.None || Plugin.Rand.Bool()))
            {
                int mod = Plugin.Rand.Bool() ? -1 : 1;
                spawnTechs += mod;
                totalTechs += mod;
                logFlag = true;
            }

            if (logFlag)
                Plugin.Log.LogInfo($"{spawnWaveProfile.name} count: {spawnTechs} (total: {totalTechs})");

            static IEnumerable<string> GetProfileNames(SpawnWaveProfile spawnWaveProfile) =>
                GetProfiles(spawnWaveProfile).Select(enemySpawnProfile => enemySpawnProfile.ReturnTechType().name);
            static string TrimAfter(string name, string trimDelimiter)
            {
                int idx = name.IndexOf(trimDelimiter);
                if (idx > 0)
                    name = name.Substring(0, idx);
                return name;
            }
        }
        private static SpawnAverages SelectTechType(Dictionary<string, SpawnAverages> spawnAverages, string[] heroTypes, HashSet<string> techTypes, double mult)
        {
            //ensure we get at elast one unit that isn't a rocket
            const string skipTech = "Rocket";
            bool doSkip = !techTypes.Any();
            if (doSkip)
                techTypes.Add(skipTech);

            //ensure we can always select something
            var choices = spawnAverages.Keys.Where(techType => !techTypes.Contains(techType));
            if (!choices.Any())
            {
                choices = spawnAverages.Keys;
                Plugin.Log.LogError("allowing non-distinct techType");
            }
            string alwaysAllow = Plugin.Rand.SelectValue(choices);
            //pick a random type, weighted by count of territories
            SpawnAverages values = Plugin.Rand.SelectValue(spawnAverages.Values, spawnAverages =>
            {
                string techType = spawnAverages.techType;
                bool hasType = techTypes.Contains(techType);
                int chance = (hasType ? 0 : spawnAverages.territories) + (alwaysAllow == techType ? 1 : 0);
                //certain tech types more likely to appear in special missions
                if (!hasType && heroTypes.Contains(techType))
                    chance = Plugin.Rand.Round(chance * mult + mult - 1);
                Plugin.Log.LogInfo($"{techType}: {chance}");
                return chance;
            });
            techTypes.Add(values.techType);

            //reduce probability of being selected in the future 
            if (values.territories > 0)
                values.territories--;

            if (doSkip)
                techTypes.Remove(skipTech);
            return values;
        }

        // operations only need to be reloaded if the application was restarted, otherwise it persists
        private static bool loadFlag = true;
        public static void Load(SpawnerInfo[] save, bool force = false)
        {
            if (loadFlag || force)
            {
                //seed a deterministic PRNG so we can procedurally randomize some additional things in here
                //without having to store every single detail in our save file
                MTRandom deterministic = new(GenerateSeed(save));

                foreach (SpawnerInfo info in deterministic.Iterate(save))
                {
                    TerritoryInstance territory = Map.Territories.Where(territory => territory.index == info.copyFromTerritoryIdx).Single();
                    EnemySpawnProfile profile = GetProfiles(territory.operation.spawnWaveProfile)[info.copyFromProfileIdx];
                    info.AssignSpawnData(profile);
                }

                float displayThreshold = 0, displayThresholdCount = 0;

                var territorySpawns = save.GroupBy(info => info.territoryIdx).ToDictionary(group => group.Key,
                    group => group.OrderBy(info => info.profileIdx).Select(info => info.profile).ToArray());
                foreach (TerritoryInstance territory in deterministic.Iterate(Map.Territories.OrderBy(t => t.index)))
                {
                    SpawnWaveProfile spawnWaveProfile = UnityEngine.Object.Instantiate(territory.operation.spawnWaveProfile);
                    territory.operation.spawnWaveProfile = spawnWaveProfile;
                    if (territorySpawns.TryGetValue(territory.index, out EnemySpawnProfile[] enemySpawnProfiles))
                    {
                        Plugin.Log.LogInfo(spawnWaveProfile.name);

                        spawnWaveProfile.enemySpawnProfiles = enemySpawnProfiles;
                        spawnWaveProfile.enemySpawnProfilesAtDifficulty = new SpawnWaveProfileAtDifficulty[0];

                        RandEnemySpawnProfiles(deterministic, enemySpawnProfiles);
                        RandAnimationCurve(deterministic, enemySpawnProfiles, spawnWaveProfile);
                        RandEnemyBuildSites(deterministic, spawnWaveProfile);
                        AddTokens(deterministic, enemySpawnProfiles, territory);

                        displayThreshold += enemySpawnProfiles.Max(p => p.UnitSpawnData.StartAtDifficulty);
                        displayThresholdCount++;

                        //spawnWaveProfile.
                    }
                    else
                    {
                        Plugin.Log.LogWarning($"No SpawnerInfo for {territory.index} - {spawnWaveProfile.name}");
                    }
                }

                DifficultyBar_BuildDifficultyBar.DisplayThreshold = (displayThreshold / displayThresholdCount);
            }
            loadFlag = false;

            LogInfo();
        }
        private static uint[] GenerateSeed(SpawnerInfo[] save)
        {
            uint[] seed = save.SelectMany(info => new object[] {
                info.displayInReconLineup, info.countMax, info.profileIdx, info.territoryIdx,
                info.copyFromProfileIdx, info.copyFromTerritoryIdx, info.capMin, info.capMax, info.countMin,
            }).Select(obj => (uint)obj.GetHashCode()).ToArray();
            if (seed.Length > MTRandom.MAX_SEED_SIZE)
            {
                Plugin.Log.LogInfo(seed.Length);
                uint[] copy = new uint[MTRandom.MAX_SEED_SIZE];
                for (uint a = 0; a < seed.Length; a++)
                    copy[a % MTRandom.MAX_SEED_SIZE] += seed[a] * (1 + a);
                seed = copy;
            }
            Plugin.Log.LogInfo("Operations.Load seed:" + seed.Select(s => s.ToString("X")).Aggregate(" ", (a, b) => a + b));
            return seed;
        }
        private static void RandEnemySpawnProfiles(MTRandom deterministic, EnemySpawnProfile[] enemySpawnProfiles)
        {
            foreach (var p in enemySpawnProfiles)
            {
                var data = p.UnitSpawnData;

                float delayMin = (float)spawnDelayMin.GetValue(data);
                float delayMax = (float)spawnDelayMax.GetValue(data);
                float timeBetween = (float)timeBetweenClusters.GetValue(data);
                float cooldown = (float)cooldownAfterSpawn.GetValue(data);

                delayMin = deterministic.GaussianCapped(delayMin, .13f);
                delayMax = deterministic.GaussianCapped(delayMax, .13f);
                timeBetween = deterministic.GaussianCapped(timeBetween, .13f);
                cooldown = deterministic.GaussianCapped(cooldown, .13f);

                Plugin.Log.LogInfo($"SpawnDelay: {data.SpawnDelay(1):0.0}-{data.SpawnDelay(0):0.0}, TimeBetweenClusters: {data.TimeBetweenClusters:0.00}, CooldownAfterSpawn: {data.CooldownAfterSpawn:0.00}");

                spawnDelayMin.SetValue(data, delayMin);
                spawnDelayMax.SetValue(data, delayMax);
                timeBetweenClusters.SetValue(data, timeBetween);
                cooldownAfterSpawn.SetValue(data, cooldown);

                Plugin.Log.LogInfo($"SpawnDelay: {data.SpawnDelay(1):0.0}-{data.SpawnDelay(0):0.0}, TimeBetweenClusters: {data.TimeBetweenClusters:0.00}");
            }
        }
        private static void RandAnimationCurve(MTRandom deterministic, EnemySpawnProfile[] enemySpawnProfiles, SpawnWaveProfile spawnWaveProfile)
        {
            AnimationCurve difficultyCurve = (AnimationCurve)Operations.difficultyCurve.GetValue(spawnWaveProfile);
            float maxStartAtDifficulty = enemySpawnProfiles.Max(p => p.UnitSpawnData.StartAtDifficulty);
            bool fix = maxStartAtDifficulty > difficultyCurve.keys.Max(k => k.value);
            int numKeys = difficultyCurve.keys.Length;
            int changes = deterministic.GaussianOEInt(2.6 + numKeys / 13.0, .65f, .39f, fix ? 1 : 0);
            Plugin.Log.LogInfo($"key changes {changes} ({fix})");
            var keyEnumerator = Enumerable.Empty<int>().GetEnumerator();
            for (int a = 0; a < changes; a++)
            {
                do
                {
                    Plugin.Log.LogInfo("MoveNext");
                    if (!keyEnumerator.MoveNext())
                    {
                        Plugin.Log.LogInfo("Iterate");
                        keyEnumerator = deterministic.Iterate(numKeys).GetEnumerator();
                    }
                }
                while (deterministic.Next(numKeys) >= keyEnumerator.Current);
                int b = keyEnumerator.Current;

                Keyframe key = difficultyCurve.keys[b];
                float time = key.time;
                float value = key.value;
                if (!fix && value > maxStartAtDifficulty && difficultyCurve.keys.Count(k => k.value > maxStartAtDifficulty) < 2)
                {
                    //ensure we don't lower the only key above the maxStartAtDifficulty
                    Plugin.Log.LogInfo("triggering fix");
                    fix = true;
                }

                //standard deviation for time is in minutes
                float dev = (.021f + .39f / spawnWaveProfile.RoundDuration) / time;
                Plugin.Log.LogInfo($"time deviation: {time * dev * spawnWaveProfile.RoundDuration * 60:0.0} ({spawnWaveProfile.RoundDuration:0.0})");
                time = deterministic.GaussianCapped(time, dev);
                value = .1f + .8f * Mathf.Clamp01(value);
                if (fix)
                    value = maxStartAtDifficulty + deterministic.Weighted(1 - maxStartAtDifficulty, value);
                else
                    value = deterministic.Weighted(value);

                Plugin.Log.LogInfo($"key {b} {key.time:0.000}:{key.value:0.000} -> {time:0.000}:{value:0.000} ({fix})");
                key.time = time;
                key.value = value;

                if (deterministic.Bool())
                    fix = false;

                difficultyCurve.RemoveKey(b);
                difficultyCurve.AddKey(key);

                //TODO: add/remove keys?
            }

            if (difficultyCurve.keys[difficultyCurve.keys.Length - 1].time < 1)
            {
                Plugin.Log.LogInfo("inserting key > 1");
                Keyframe newKey = deterministic.SelectValue(difficultyCurve.keys);
                newKey.time = 1 + deterministic.Weighted(.026f);
                newKey.value = 0;
                difficultyCurve.AddKey(newKey);
            }

            foreach (var k in difficultyCurve.keys)
                Plugin.Log.LogInfo($"{k.time:0.000}:{k.value:0.000}");
            Plugin.Log.LogInfo(difficultyCurve.Evaluate(1).ToString("0.000"));
        }
        private static void RandEnemyBuildSites(MTRandom deterministic, SpawnWaveProfile spawnWaveProfile)
        {
            //randomize build site display order (end of list may wind up getting cut off in display panel) 
            deterministic.Shuffle(spawnWaveProfile.enemyBuildSites);
            if (spawnWaveProfile.enemyBuildSites.Any())
                Plugin.Log.LogInfo("enemyBuildSites: " +
                    spawnWaveProfile.enemyBuildSites.Select(b => b.name).Aggregate("", (a, b) => a + ' ' + b).Trim());
        }
        private static void AddTokens(MTRandom deterministic, EnemySpawnProfile[] enemySpawnProfiles, TerritoryInstance territory)
        {
            Operation operation = territory.operation;
            SpawnWaveProfile spawnWaveProfile = operation.spawnWaveProfile;
            float difficulty = enemySpawnProfiles.Sum(p => p.UnitSpawnData.StartAtDifficulty);
            Plugin.Log.LogInfo($"{spawnWaveProfile.name} startAtDifficulty total {difficulty:0.00}");
            while (deterministic.OE(difficulty) > (territory.specialTag == SpecialTag.None ? 5 : 10))
            {
                operation.tokenReward++;
                Plugin.Log.LogInfo($"adding token {operation.tokenReward}");
            }
        }
        private static void LogInfo()
        {
            foreach (TerritoryInstance territory in Map.Territories)
            {
                SpawnWaveProfile spawnWaveProfile = territory.operation.spawnWaveProfile;
                foreach (EnemySpawnProfile profile in spawnWaveProfile.enemySpawnProfiles.Cast<EnemySpawnProfile>())
                {
                    SpawnerData data = profile.UnitSpawnData;

                    Plugin.Log.LogInfo($"{spawnWaveProfile.name} {data.SpawnTech.name} ({profile.GetInstanceID()}): {data.SpawnCount(0)}-{data.SpawnCount(1)} ({data.SpawnCap(0)}-{data.SpawnCap(1)}), {data.StartAtDifficulty:0.00}");

                    foreach (var c in data.SpawnConditions)
                        Plugin.Log.LogError("SpawnCondition: " + c);

                    Plugin.Log.LogDebug("SpawnCapCycleMultipler: " + data.SpawnCapCycleMultipler);
                    Plugin.Log.LogDebug("SpawnDelay: " + data.SpawnDelay(1) + "-" + data.SpawnDelay(0));
                    Plugin.Log.LogDebug("TimeBetweenClusters: " + data.TimeBetweenClusters);
                    if (data.SpawnTech is CalldownType)
                        Plugin.Log.LogDebug($"CalldownType: {data.SpawnTech.name}");
                }
            }
        }

        private static List<EnemySpawnProfile> GetProfiles(SpawnWaveProfile spawnWaveProfile) =>
            spawnWaveProfile.ReturnSpawnProfilesPerDifficulty(Map.MissionManagerAsset.GameDifficultyIndex)
            .Cast<EnemySpawnProfile>().ToList();

        private class SpawnAverages
        {
            public string techType;
            public List<SpawnerInfo> info = new();
            public double number, countMin, countMax, capMin, capMax;
            public int territories;

            public float startAtDifficulty = float.NaN;
            public void GenStartAtDifficulty()
            {
                DifficultyRange(out float min, out float max);
                float avg = Plugin.Rand.Range(min, max);
                float dev = (1f - max + min);
                dev *= dev * .169f;
                if (avg > .5f)
                    dev *= (1 - avg) / avg;
                float cap = (float)Math.Max(0, 2 * avg - 1);
                startAtDifficulty = Plugin.Rand.GaussianCapped(avg, dev, cap);
                Plugin.Log.LogInfo($"{techType} startAtDifficulty: {startAtDifficulty:0.00} ({min}-{max}), Gaussian({avg:0.00},{dev:0.000},{cap:0.00})");
            }

            public void DifficultyRange(out float min, out float max)
            {
                if (info.Count > 0)
                {
                    min = float.MaxValue;
                    max = float.MinValue;
                    foreach (var e in info)
                    {
                        var d = e.profile.UnitSpawnData.StartAtDifficulty;
                        min = Math.Min(min, d);
                        max = Math.Max(max, d);
                    }
                }
                else
                {
                    min = max = float.NaN;
                }
            }
        }
        [Serializable]
        public class SpawnerInfo
        {
            public SpawnerInfo(int copyFromTerritoryIdx, int copyFromProfileIdx, EnemySpawnProfile profile)
            {
                if (copyFromProfileIdx < 0)
                    throw new ArgumentException("invalid copyFromProfileIdx");

                this.copyFromTerritoryIdx = copyFromTerritoryIdx;
                this.copyFromProfileIdx = copyFromProfileIdx;
                this.profile = profile;
            }

            public SpawnerInfo(SpawnerInfo copyFrom, int territoryIdx, int profileIdx, int countMin, int countMax, int capMin, int capMax, float difficulty)
            {
                this.copyFromTerritoryIdx = copyFrom.copyFromTerritoryIdx;
                this.copyFromProfileIdx = copyFrom.copyFromProfileIdx;

                this.territoryIdx = territoryIdx;
                this.profileIdx = profileIdx;

                this.countMin = countMin;
                this.countMax = countMax;
                this.capMin = capMin;
                this.capMax = capMax;

                this.difficulty = difficulty;
            }

            public void AssignSpawnData(EnemySpawnProfile enemySpawnProfile)
            {
                profile = UnityEngine.Object.Instantiate(enemySpawnProfile);
                SpawnerData data = profile.UnitSpawnData;

                startAtDifficulty.SetValue(data, difficulty);
                spawnCountMin.SetValue(data, countMin);
                spawnCountMax.SetValue(data, countMax);
                spawnCapMin.SetValue(data, capMin);
                spawnCapMax.SetValue(data, capMax);
            }

            //territory index after shuffle
            public int territoryIdx, copyFromTerritoryIdx;
            //index into ReturnSpawnProfilesPerDifficulty list
            public int profileIdx, copyFromProfileIdx;

            public int countMin, countMax, capMin, capMax;
            public float difficulty;
            public bool displayInReconLineup = true;

            [NonSerialized]
            public EnemySpawnProfile profile;
        }
    }
}
