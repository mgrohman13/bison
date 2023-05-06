using DynamicEnums;
using HarmonyLib;
using LevelGeneration;
using LevelGeneration.WorldMap;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WarpipsReplayability.Patches;
using static LevelGeneration.SpawnWaveProfile;
using static LevelGeneration.WorldMap.TerritoryInstance;

namespace WarpipsReplayability.Mod
{
    internal class Operations
    {
        public static WorldMapUIController WorldMapUIController { get; private set; }
        public static TerritoryInstance SelectedTerritory { get; set; }

        //used to track when we need to re-randomize a failed mission
        public static TerritoryInstance FailedMission { get; set; }

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
        private static readonly FieldInfo roundDuration = AccessTools.Field(typeof(SpawnWaveProfile), "roundDuration");

        //especially impactful tech types, in order of impact
        private static readonly string[] heroTypes = new string[] { "Hind", "Bubba", "Rocket", "Predator", "Tanya" };

        public static bool[] RollHiddenRewards()
        {
            var hiddenRewards = Map.Territories.SelectMany(t =>
            {
                //each territory has a variable chance for each reward to remain hidden
                int chance = Plugin.Rand.GaussianOEInt(6.5f, .26f, .21f, 1);
                Plugin.Log.LogDebug($"hidden reward chance {chance}");
                //each reward has an individual chance to remain hidden
                return t.operation.itemRewards.Select(r => Plugin.Rand.Next(chance) == 0);
            }).ToArray();
            Plugin.Log.LogInfo($"generated HiddenRewards");
            return hiddenRewards;
        }

        public static void DrawMap(WorldMapUIController worldMapUIController) => UpdateShroud(worldMapUIController);
        private static void UpdateShroud(WorldMapUIController worldMapUIController)
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
            Load(save);//, true);
            return save;
        }
        private static Dictionary<string, SpawnAverages> AggregateSpawnInfo()
        {
            //aggregate map spawner data 
            Dictionary<string, SpawnAverages> spawnAverages = new();
            foreach (TerritoryInstance territory in Plugin.Rand.Iterate(Map.Territories))
            {
                Plugin.Log.LogDebug(Environment.NewLine);

                Operation operation = territory.operation;
                SpawnWaveProfile spawnWaveProfile = operation.spawnWaveProfile;
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
                        values.info.Add(new(territory.index, profiles.IndexOf(profile), operation.map.MapLength, profile));
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
                float div = values.territories;
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

            ////we need to know the highest curve values to ensure we only select units that can actually spawn
            //AnimationCurve curve = (AnimationCurve)Operations.curve.GetValue(spawnWaveProfile);
            //float maxDifficulty = curve.curve.keys.Max(k => k.value);
            //Plugin.Log.LogInfo("maxDifficulty: " + maxDifficulty);

            //only select units that can actually spawn with this difficulty curve
            //spawnAverages = spawnAverages.Where(pair => pair.Value.startAtDifficulty < maxDifficulty)
            //    .ToDictionary(pair => pair.Key, pair => pair.Value);

            foreach (var s in spawnAverages.Values)
                s.GenStartAtDifficulty();

            CountTechTypes(spawnWaveProfile, territory.specialTag, spawnAverages.Count, out int spawnTechs, out int totalTechs);

            //keep techTypes distinct
            HashSet<string> techTypes = new();

            if (spawnTechs > 0)
                foreach (int idx in Plugin.Rand.Iterate(spawnTechs))
                {
                    //increase spawn strength for special missions
                    float mult = territory.specialTag switch
                    {
                        SpecialTag.EnemyObjective => (float)Math.Sqrt(3),
                        SpecialTag.HighValueReward => (float)Math.Sqrt(2),
                        _ => 1.0f,
                    };

                    SpawnAverages values = SelectTechType(spawnAverages, heroTypes, techTypes, mult);

                    //randomize spawn amounts, based on island-wide averages
                    const float deviation = .13f;
                    int countMin = Plugin.Rand.GaussianCappedInt((values.countMin) * mult, deviation, values.countMin > 1 ? 1 : 0);
                    int countMax = Plugin.Rand.GaussianCappedInt((values.countMax - values.countMin + 1) * mult + countMin, deviation, Math.Max(1, countMin));
                    //if (territory.specialTag == SpecialTag.None && Plugin.Rand.Bool())
                    //{
                    //    //chance to widen range
                    //    countMax += countMin;
                    //    countMin = 0;
                    //}
                    int capMin = Plugin.Rand.GaussianCappedInt((values.capMin) * mult + countMax, deviation, countMax);
                    //minimum caps are generally quite a bit higher, so bring down the max cap somewhat
                    int capMax = Plugin.Rand.GaussianCappedInt((values.capMax - values.capMin + 2) / 2f * mult + capMin, deviation, capMin);

                    int heroIndex = Array.IndexOf(heroTypes, values.techType) + 1;
                    bool baseAlwaysOne = values.countMax == values.number;
                    if (techTypes.Count > 1)
                    {
                        //because we add a little bit at each step above the averages, we need to be careful not to overdo it with certain tech types                    
                        if (heroIndex > 0 && baseAlwaysOne)
                        {
                            //allow more for higher heroIndex, difficult missions
                            float highMult = mult * values.number * (1f + heroIndex / (float)heroTypes.Length);
                            float lowMult = (float)(Math.Sqrt(highMult) + Plugin.Rand.Range(0, countMin));
                            highMult = Math.Max(highMult + Plugin.Rand.Range(0, countMax), lowMult + 1);
                            //chance to temper down extreme values
                            int min = Math.Min(Plugin.Rand.Round(Plugin.Rand.Range(0, lowMult)), countMin);
                            int max = Math.Min(Plugin.Rand.Round(Plugin.Rand.Range(min, highMult)), countMax);
                            Plugin.Log.LogInfo($"reducing {values.techType} to number: {min}-{max} ({values.number:0.00},{lowMult:0.00},{mult:0.00}), was {countMin}-{countMax}");
                            countMin = min;
                            countMax = max;
                        }
                        if (heroIndex > 0 || baseAlwaysOne)
                        {
                            Plugin.Log.LogInfo($"reducing {values.techType}, was {countMin}-{countMax} ({capMin}-{capMax})");
                            //invert heroIndex 
                            if (heroIndex > 0)
                                heroIndex = Plugin.Rand.Round(1f + 2f * (heroTypes.Length - heroIndex) / (heroTypes.Length - 1f));
                            //increasing chance to reduce each
                            countMin = Math.Max(countMin - Plugin.Rand.RangeInt(0, ++heroIndex), 0);
                            countMax = Math.Max(countMax - Plugin.Rand.RangeInt(0, ++heroIndex), Math.Max(1, countMin));
                            capMin = Math.Max(capMin - Plugin.Rand.RangeInt(0, ++heroIndex), Math.Max(1, countMax));
                            capMax = Math.Max(capMax - Plugin.Rand.RangeInt(0, ++heroIndex), Math.Max(1, capMin));
                        }
                    }
                    else
                    {
                        Plugin.Log.LogInfo($"ensuring range for first unit {values.techType} ({heroIndex},{baseAlwaysOne})");
                        countMax += 1;
                        capMin += 1;
                        capMax += 2;
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

            Plugin.Log.LogInfo("Generated random spawns");
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
            //if (totalTechs > 10)
            //    Plugin.Log.LogInfo(allDistinctTechTypes.Aggregate(" ", (a, b) => a + " " + b).Trim());

            bool logFlag = false;
            Plugin.Log.LogInfo($"{spawnWaveProfile.name} count: {spawnTechs} (total: {totalTechs})");

            //randomize count somewhat 
            while (spawnTechs > 1 && spawnTechs < availableTypes && Plugin.Rand.Bool()
                    && (spawnTechs < 9 || Plugin.Rand.Bool())
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
        private static SpawnAverages SelectTechType(Dictionary<string, SpawnAverages> spawnAverages, string[] heroTypes, HashSet<string> techTypes, float mult)
        {
            Dictionary<string, SpawnAverages> temp;
            if (!techTypes.Any())
            {
                //ensure we pick at least one primary attack unit
                string[] forceTech = new string[] { "PistolPip", "Warfighter", "Shotgunner", "UAZ", "Warmule", "DuneBuggy", "Gruz", "T92" };
                spawnAverages = Filter(p => forceTech.Contains(p.Key));

                //ensure we pick at least one unit that spawns at low difficulty
                do
                    temp = Filter(p => p.Value.startAtDifficulty < Plugin.Rand.Gaussian(1f / 6, 1f / 3));
                while (!temp.Any());
                spawnAverages = temp;
            }
            else
            {
                //ensure we can always select something
                temp = Filter(p => !techTypes.Contains(p.Key));
                if (temp.Any())
                    spawnAverages = temp;
                else
                    Plugin.Log.LogError("allowing non-distinct techType");
            }

            string alwaysAllow = Plugin.Rand.SelectValue(spawnAverages.Keys);
            //pick a random type, weighted by count of territories
            SpawnAverages values = Plugin.Rand.SelectValue(spawnAverages.Values, spawnAverages =>
            {
                string techType = spawnAverages.techType;
                int chance = spawnAverages.territories;
                if (alwaysAllow == techType)
                    chance++;
                //certain tech types more likely to appear in special missions
                if (heroTypes.Contains(techType))
                    chance = Plugin.Rand.Round((chance + mult - 1) * mult);
                Plugin.Log.LogInfo($"{techType}: {chance}");
                return chance;
            });
            techTypes.Add(values.techType);

            //reduce probability of being selected in the future 
            if (values.territories > 0)
                values.territories--;
            return values;

            Dictionary<string, SpawnAverages> Filter(Func<KeyValuePair<string, SpawnAverages>, bool> predicate) =>
                spawnAverages.Where(predicate).ToDictionary(p => p.Key, p => p.Value);
        }

        public static void RandOnLoss()
        {
            if (FailedMission != null)
            {
                RandOnLoss(FailedMission, false, null);
                FailedMission = null;
            }
        }
        private static void RandOnLoss(TerritoryInstance territory, bool load, int? failure)
        {
            Plugin.Log.LogInfo($"RandOnLoss {territory.index} {territory.operation.operationName} {territory.operation.spawnWaveProfile.name}");

            Operation operation = territory.operation;
            SpawnWaveProfile spawnWaveProfile = operation.spawnWaveProfile;
            IEnumerable<EnemySpawnProfile> enemySpawnProfiles = spawnWaveProfile.enemySpawnProfiles.Cast<EnemySpawnProfile>();

            //the game changes the map on every loss, and the mapLength may change, which we use to randomize the difficultyCurve
            //so we use a spawnerInfo array for this territory to store the mapLength used for each regeneration step
            //this maintains our deterministic difficultyCurve randomization
            SpawnerInfo[] spawnerInfo = Persist.Instance.SpawnerInfo.Where(i => i.territoryIdx == territory.index).ToArray();
            int idx;
            if (load)
            {
                //index 0 reserved for initial generation
                idx = failure.Value + 1;
                Plugin.Log.LogInfo($"RandOnLoss loading mapLength from {idx % spawnerInfo.Length} ({idx}, total {spawnerInfo.Length})");
                //reuse values if necessary
                idx %= spawnerInfo.Length;
            }
            else
            {
                //doesnt matter where we store the failures count as long as it's always at the same index
                idx = ++spawnerInfo[0].failures;

                //store the new map length if we have space (index 0 reserved for initial generation)
                if (idx < spawnerInfo.Length)
                    spawnerInfo[idx].mapLength = operation.map.MapLength;
                else
                    idx %= spawnerInfo.Length;

                //this will rarely happen in practice, if ever... but we may run out of space to store map lengths
                //this will only happen if you lose an operation as many times as there are enemy units present in it
                //it could be fixed by storing these in a separate collection, but that seems like overkill for something so unlikely
                if (spawnerInfo[idx].mapLength != operation.map.MapLength)
                    Plugin.Log.LogWarning($"RandOnLoss incorrect map length (using {spawnerInfo[idx].mapLength}, actual {operation.map.MapLength})");

                int a = 0;
                Plugin.Log.LogInfo($"RandOnLoss saving failures and mapLength ({idx}): " + Environment.NewLine +
                    spawnerInfo.Select(i => $"{a++}: {i.failures}, {i.mapLength}")
                        .Aggregate("", (a, b) => a + Environment.NewLine + b));
                Persist.SaveCurrent();
            }
            int mapLength = spawnerInfo[idx].mapLength;

            //new seed based on previous values
            uint[] seed = GetDifficultyCurve(spawnWaveProfile).keys
                .SelectMany(k => new float[] { k.time, k.value })
                .Append(mapLength)
                .Concat(enemySpawnProfiles.Select(p => p.UnitSpawnData.StartAtDifficulty))
                .Select(f => (uint)f.GetHashCode())
                .ToArray();
            Plugin.Log.LogInfo($"RandOnLoss seed " + Plugin.GetSeedString(seed));
            MTRandom deterministic = new(seed);

            foreach (EnemySpawnProfile enemySpawnProfile in deterministic.Iterate(enemySpawnProfiles))
            {
                SpawnerData data = enemySpawnProfile.UnitSpawnData;
                float difficulty = data.StartAtDifficulty;
                difficulty = SpawnAverages.GenStartAtDifficulty(deterministic, data.SpawnTech.name, difficulty, difficulty);
                Plugin.Log.LogInfo($"RandOnLoss modifying {data.SpawnTech.name} StartAtDifficulty {data.StartAtDifficulty} -> {difficulty}");
                startAtDifficulty.SetValue(data, difficulty);
            }

            Plugin.Log.LogInfo($"RandOnLoss mapLength {mapLength}");
            RandAnimationCurve(deterministic, operation, mapLength);
        }

        public static void Load(SpawnerInfo[] save)
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
                group => group.OrderBy(info => info.profileIdx).ToArray());
            foreach (TerritoryInstance territory in deterministic.Iterate(Map.Territories.OrderBy(t => t.index)))
            {
                SpawnWaveProfile spawnWaveProfile = UnityEngine.Object.Instantiate(territory.operation.spawnWaveProfile);
                territory.operation.spawnWaveProfile = spawnWaveProfile;
                if (territorySpawns.TryGetValue(territory.index, out SpawnerInfo[] info))
                {
                    EnemySpawnProfile[] enemySpawnProfiles = info.Select(i => i.profile).ToArray();
                    Plugin.Log.LogInfo(spawnWaveProfile.name);

                    spawnWaveProfile.enemySpawnProfiles = enemySpawnProfiles;
                    spawnWaveProfile.enemySpawnProfilesAtDifficulty = new SpawnWaveProfileAtDifficulty[0];

                    RandEnemySpawnProfiles(deterministic, enemySpawnProfiles);
                    RandAnimationCurve(deterministic, territory.operation, info[0].mapLength);
                    RandEnemyBuildSites(deterministic, spawnWaveProfile);
                    AddTokens(deterministic, enemySpawnProfiles, territory);

                    displayThreshold += MaxStartAtDifficulty(enemySpawnProfiles);
                    displayThresholdCount++;
                }
                else
                {
                    Plugin.Log.LogWarning($"No SpawnerInfo for {territory.index} - {spawnWaveProfile.name}");
                }
            }

            DifficultyBar_BuildDifficultyBar.DisplayThreshold = (float)Math.Sqrt(displayThreshold / displayThresholdCount) * .9f;

            Plugin.Log.LogInfo("Loaded random operations");
            LogInfo();

            //deterministically replay through the changes from lost operations 
            foreach (SpawnerInfo info in save)
                for (int a = 0; a < info.failures; a++)
                    RandOnLoss(Map.Territories.Where(t => t.index == info.territoryIdx).Single(), true, a);
        }

        private static uint[] GenerateSeed(SpawnerInfo[] save)
        {
            uint[] seed = save.SelectMany(info => new object[] {
                info.displayInReconLineup, info.countMax, info.profileIdx, info.territoryIdx,
                info.copyFromProfileIdx, info.copyFromTerritoryIdx, info.capMin, info.capMax, info.countMin,
            }).Select(obj => (uint)obj.GetHashCode()).ToArray();
            if (seed.Length > MTRandom.MAX_SEED_SIZE)
            {
                Plugin.Log.LogInfo("seed.Length: " + seed.Length);
                uint[] copy = new uint[MTRandom.MAX_SEED_SIZE];
                for (uint a = 0; a < seed.Length; a++)
                    copy[a % MTRandom.MAX_SEED_SIZE] += seed[a] * (1 + a);
                seed = copy;
            }
            Plugin.Log.LogInfo("Operations.Load seed:" + Plugin.GetSeedString(seed));
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

                const float deviation = .13f;
                delayMin = deterministic.GaussianCapped(delayMin, deviation);
                delayMax = deterministic.GaussianCapped(delayMax, deviation);
                if (delayMin < delayMax)
                    (delayMin, delayMax) = (delayMax, delayMin);
                timeBetween = deterministic.GaussianCapped(timeBetween, deviation);
                cooldown = deterministic.GaussianCapped(cooldown, deviation);

                Plugin.Log.LogInfo($"{p.ReturnTechType().name} SpawnDelay: {data.SpawnDelay(1):0.0}-{data.SpawnDelay(0):0.0}, " +
                    $"TimeBetweenClusters: {data.TimeBetweenClusters:0.00}, CooldownAfterSpawn: {data.CooldownAfterSpawn:0.00}");

                spawnDelayMin.SetValue(data, delayMin);
                spawnDelayMax.SetValue(data, delayMax);
                timeBetweenClusters.SetValue(data, timeBetween);
                cooldownAfterSpawn.SetValue(data, cooldown);

                Plugin.Log.LogInfo($"{p.ReturnTechType().name} SpawnDelay: {data.SpawnDelay(1):0.0}-{data.SpawnDelay(0):0.0}, " +
                    $"TimeBetweenClusters: {data.TimeBetweenClusters:0.00}, CooldownAfterSpawn: {data.CooldownAfterSpawn:0.00}");
            }
        }

        private static void RandAnimationCurve(MTRandom deterministic, Operation operation, int mapLength)
        {
            SpawnWaveProfile spawnWaveProfile = operation.spawnWaveProfile;
            AnimationCurve curve = GetDifficultyCurve(spawnWaveProfile);

            float duration = deterministic.GaussianOEInt(spawnWaveProfile.RoundDuration, .13f, .13f, 4);
            roundDuration.SetValue(spawnWaveProfile, duration);
            float maxStartAtDifficulty = MaxStartAtDifficulty(spawnWaveProfile.enemySpawnProfiles);
            Plugin.Log.LogInfo($"maxStartAtDifficulty {maxStartAtDifficulty:0.000}, duration: {duration}, mapLength: {mapLength}");

            //mapLength ranges from 70-320
            //this results in 30.6-79.1 seconds considered early 
            float earlyTime = (float)(Math.Sqrt(mapLength - 26) / 13.0);
            earlyTime = deterministic.GaussianCapped(earlyTime / duration, .13f);

            RandKeys(deterministic, curve, duration, maxStartAtDifficulty);
            IncreaseMax(deterministic, curve);
            EasyEarly(deterministic, curve, ref earlyTime);
            if (spawnWaveProfile.bombsOnCycle == 0)
            {
                BombGap(deterministic, curve, duration, earlyTime);
            }
            else
            {
                int a = 1 + deterministic.Next(curve.keys.Length - 1);
                ReduceKey(deterministic, curve, a, curve.keys[a], "random easy");
            }
            CheckEarly(deterministic, curve, duration, earlyTime);
            EnsureSpawn(deterministic, curve, duration, maxStartAtDifficulty);
            TaperEnd(deterministic, curve);

            foreach (var k in curve.keys)
                Plugin.Log.LogInfo($"{k.time:0.000}:{k.value:0.000}");
            Plugin.Log.LogInfo(curve.Evaluate(1).ToString("0.000"));
        }
        private static void RandKeys(MTRandom deterministic, AnimationCurve curve, float duration, float maxStartAtDifficulty)
        {
            //randomize some curve.keys  
            float maxDifficultyCurve = GetMaxKey(curve);
            int lowerCap = maxStartAtDifficulty > maxDifficultyCurve ? 1 : 0;
            int changes = deterministic.GaussianOEInt(Math.E + curve.keys.Length / 13f, .65f, .39f, lowerCap);
            RandKeys(deterministic, curve, duration, maxStartAtDifficulty, changes);
        }
        private static void IncreaseMax(MTRandom deterministic, AnimationCurve curve)
        {
            //increase max key to a high value
            int maxKey = -1;
            float maxValue = -1f;
            foreach (int a in deterministic.Iterate(curve.keys.Length))
            {
                Keyframe key = curve.keys[a];
                float value = key.value;
                if (key.time < 1 && maxValue < value)
                {
                    maxKey = a;
                    maxValue = value;
                }
            }
            Keyframe max = curve.keys[maxKey];
            max.value = deterministic.Weighted(.9f + .1f * Clamp19(maxValue));
            Plugin.Log.LogInfo($"max key {maxKey} {max.time:0.000}:{maxValue:0.000} -> {max.value:0.000}");
            curve.MoveKey(maxKey, max);
        }
        private static void EasyEarly(MTRandom deterministic, AnimationCurve curve, ref float earlyTime)
        {
            //typically don't ramp up too high by 1 minute mark
            for (int a = 1; a < curve.keys.Length && deterministic.Bool(); a++)
            {
                Keyframe key = ReduceKey(deterministic, curve, a, curve.keys[a], "early");
                if (key.time > earlyTime)
                {
                    earlyTime = key.time + deterministic.OE(.001f);
                    break;
                }
            }
        }
        private static void BombGap(MTRandom deterministic, AnimationCurve curve, float duration, float earlyTime)
        {
            //if bombs present, ensure there is a gap of weakness somewhere 
            int numKeys = curve.keys.Length;
            float center = curve.keys[Math.Max(GetIdx(), GetIdx())].time;
            float span = deterministic.GaussianCapped(2f / duration, .13f);
            float before = span;
            float after = deterministic.DoubleHalf(before);
            before -= after;
            before = center - before;
            after = center + after;

            Plugin.Log.LogInfo($"bomb gap {before:0.000}-{after:0.000} ({center:0.000}, {span:0.000}, {earlyTime:0.000})");
            if (before < earlyTime)
            {
                after += earlyTime - before;
                before = earlyTime;
                Plugin.Log.LogInfo($"bomb gap oneMinute {before:0.000}-{after:0.000}");
            }
            if (after > 1)
            {
                before -= after - 1;
                after = 1;
                Plugin.Log.LogInfo($"bomb gap 1 {before:0.000}-{after:0.000}");
            }

            for (int a = 1; a < numKeys; a++)
            {
                Keyframe key = curve.keys[a];
                float time = key.time;
                if (time > after)
                    break;
                if (time > before)
                    ReduceKey(deterministic, curve, a, key, "bomb");
            }

            int GetIdx() => 2 + deterministic.Next(numKeys - 2);
        }
        private static void CheckEarly(MTRandom deterministic, AnimationCurve curve, float duration, float earlyTime)
        {
            //ensure difficulty ramps up to at least around .2 early on
            int numKeys = curve.keys.Length;
            float cutoff = deterministic.GaussianCapped(earlyTime + 1f / duration, .13f, earlyTime);
            float max = curve.Evaluate(cutoff);
            for (int a = 1; a < numKeys; a++)
            {
                Keyframe key = curve.keys[a];
                if (key.time > cutoff)
                    break;
                max = Math.Max(max, key.value);
            }
            if (max < deterministic.Gaussian(.4f, .169f))
            {
                Keyframe key = curve.keys[deterministic.Next(numKeys)];
                key.time = deterministic.Range(earlyTime, cutoff);
                key.value = .2f + deterministic.Weighted(.8f, 1 / 4f);
                curve.AddKey(key);
                Plugin.Log.LogInfo($"inserting early spawn key {key.time:0.000}:{key.value:0.000} ({earlyTime:0.000}-{cutoff:0.000})");
            }
        }
        private static void EnsureSpawn(MTRandom deterministic, AnimationCurve curve, float duration, float maxStartAtDifficulty)
        {
            //apply final fix if necessary so all units can spawn  
            int changes = deterministic.OEInt();
            if (maxStartAtDifficulty > GetMaxKey(curve))
                changes++;
            RandKeys(deterministic, curve, duration, maxStartAtDifficulty, changes);
        }
        private static void TaperEnd(MTRandom deterministic, AnimationCurve curve)
        {
            //ensure a key at time >= 1 
            if (curve.keys[curve.keys.Length - 1].time < 1)
            {
                Plugin.Log.LogInfo("inserting key > 1");
                Keyframe newKey = deterministic.SelectValue(curve.keys);
                newKey.time = 1 + deterministic.Weighted(.1f);
                newKey.value = 0;
                curve.AddKey(newKey);
            }
        }
        private static Keyframe ReduceKey(MTRandom deterministic, AnimationCurve curve, int index, Keyframe key, string log)
        {
            float value = key.value;
            key.value = deterministic.Weighted(Clamp19(value) / 2f);
            Plugin.Log.LogInfo($"{log} key {index} {key.time:0.000}:{value:0.000} -> {key.value:0.000}");
            curve.MoveKey(index, key);
            return key;
        }
        private static void RandKeys(MTRandom deterministic, AnimationCurve curve, float duration, float maxStartAtDifficulty, int changes)
        {
            bool fix = maxStartAtDifficulty > GetMaxKey(curve);

            Plugin.Log.LogInfo($"key changes {changes} ({fix})");
            var keyEnumerator = Enumerable.Empty<int>().GetEnumerator();
            for (int a = 0; a < changes; a++)
            {
                int numKeys = curve.keys.Length;
                do
                {
                    Plugin.Log.LogInfo("MoveNext");
                    if (!keyEnumerator.MoveNext())
                    {
                        Plugin.Log.LogInfo("Iterate");
                        keyEnumerator = deterministic.Iterate(numKeys + 1).GetEnumerator();
                    }
                }
                while (keyEnumerator.Current >= numKeys || keyEnumerator.Current <= deterministic.Next(numKeys + 1));
                int b = keyEnumerator.Current;

                Keyframe key = curve.keys[b];
                float time = key.time;
                float value = key.value;
                if (!fix && time < 1 && value > maxStartAtDifficulty && curve.keys.Where(k => k.time < 1).Count(k => k.value > maxStartAtDifficulty) < 2)
                {
                    //ensure we don't lower the only key above the maxStartAtDifficulty
                    Plugin.Log.LogInfo("triggering fix");
                    fix = true;
                }

                //chance to delete key entirely
                if (!fix && deterministic.OEInt(deterministic.Next(numKeys - 3)) > 13)
                {
                    Plugin.Log.LogInfo($"RemoveKey {b} {time:0.000}:{value:0.000}");
                    curve.RemoveKey(b);
                    continue;
                }

                //standard deviation for time is in minutes
                float dev = (.021f + .39f / duration) / time;
                Plugin.Log.LogInfo($"time deviation: {time * dev * duration * 60:0.0} ({duration:0.0})");
                time = deterministic.GaussianCapped(time, dev);
                value = Clamp19(value);
                if (fix)
                    value = maxStartAtDifficulty + deterministic.Weighted(1 - maxStartAtDifficulty, value);
                else
                    value = deterministic.Weighted(value);

                Plugin.Log.LogInfo($"key {b} {key.time:0.000}:{key.value:0.000} -> {time:0.000}:{value:0.000} ({fix})");
                key.time = time;
                key.value = value;

                if (deterministic.Bool())
                    fix = false;

                //chance to duplicate modified key at random time
                if (deterministic.Next(numKeys) < deterministic.OEInt())
                {
                    key.time = deterministic.Weighted(Clamp19(key.time));
                    Plugin.Log.LogInfo($"AddKey {key.time:0.000}:{key.value:0.000}");
                }
                else
                {
                    curve.RemoveKey(b);
                }
                curve.AddKey(key);
            }
        }
        private static float Clamp19(float value) =>
            .1f + .8f * Mathf.Clamp01(value);
        private static float MaxStartAtDifficulty(IEnumerable<SpawnProfile> enemySpawnProfiles) =>
            enemySpawnProfiles.Cast<EnemySpawnProfile>().Max(p => p.UnitSpawnData.StartAtDifficulty);
        private static float GetMaxKey(AnimationCurve curve) =>
            curve.keys.Where(k => k.time < 1).Max(k => k.value);

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
            while (deterministic.OEInt(difficulty) > (territory.specialTag == SpecialTag.None ? 3 : 5))
            {
                operation.tokenReward++;
                Plugin.Log.LogInfo($"adding token {operation.tokenReward}");
                difficulty -= deterministic.OEFloat();
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
        private static AnimationCurve GetDifficultyCurve(SpawnWaveProfile spawnWaveProfile) =>
            (AnimationCurve)difficultyCurve.GetValue(spawnWaveProfile);

        private class SpawnAverages
        {
            public string techType;
            public List<SpawnerInfo> info = new();
            public float number, countMin, countMax, capMin, capMax;
            public int territories;

            public float startAtDifficulty = float.NaN;
            public void GenStartAtDifficulty()
            {
                DifficultyRange(out float min, out float max);
                this.startAtDifficulty = GenStartAtDifficulty(Plugin.Rand, techType, min, max);
            }
            public static float GenStartAtDifficulty(MTRandom rand, string techType, float min, float max)
            {
                float startAtDifficulty;
                float avg = rand.Range(min, max);
                //if ((Array.IndexOf(heroTypes, techType) < 0 || min < .5) && rand.Next(26) == 0)
                //{
                //    startAtDifficulty = rand.Weighted(avg);
                //    Plugin.Log.LogInfo($"{techType} startAtDifficulty: {startAtDifficulty:0.00} ({min:0.00}-{max:0.00}), Weighted({avg:0.00})");
                //}
                if (rand.Next(21) == 0)
                {
                    //rare chance to allow full range of startAtDifficulty values (except for heroTypes which enforce a minimum)
                    int heroIdx = Array.IndexOf(heroTypes, techType);
                    float range = heroIdx < 0 ? 1 : (heroIdx + 1) / (heroTypes.Length + 1f);
                    avg *= range;
                    startAtDifficulty = (1 - range) + rand.Weighted(range, avg);
                    Plugin.Log.LogInfo($"{techType} startAtDifficulty: {startAtDifficulty:0.000} ({min:0.00}-{max:0.00}), {1 - range:0.00}+Weighted({range:0.00},{avg:0.00})");
                }
                else
                {
                    float dev = (1f - max + min);
                    dev *= dev * .169f;
                    if (avg > .5f)
                        dev *= (1 - avg) / avg;
                    float cap = Math.Max(0, 2 * avg - 1);
                    startAtDifficulty = rand.GaussianCapped(avg, dev, cap);
                    Plugin.Log.LogInfo($"{techType} startAtDifficulty: {startAtDifficulty:0.00} ({min:0.00}-{max:0.00}), Gaussian({avg:0.00},{dev:0.000},{cap:0.00})");
                }
                return startAtDifficulty;
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
            public SpawnerInfo(int copyFromTerritoryIdx, int copyFromProfileIdx, int mapLength, EnemySpawnProfile profile)
            {
                if (copyFromProfileIdx < 0)
                    throw new ArgumentException("invalid copyFromProfileIdx");

                this.copyFromTerritoryIdx = copyFromTerritoryIdx;
                this.copyFromProfileIdx = copyFromProfileIdx;
                this.mapLength = mapLength;
                this.profile = profile;
            }

            public SpawnerInfo(SpawnerInfo copyFrom, int territoryIdx, int profileIdx, int countMin, int countMax, int capMin, int capMax, float difficulty)
            {
                this.copyFromTerritoryIdx = copyFrom.copyFromTerritoryIdx;
                this.copyFromProfileIdx = copyFrom.copyFromProfileIdx;

                this.territoryIdx = territoryIdx;
                this.profileIdx = profileIdx;

                this.mapLength = copyFrom.mapLength;

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


            //randomly generated spawn data
            public int countMin, countMax, capMin, capMax;
            public float difficulty;
            public bool displayInReconLineup = true;

            //set later on if you lose operations
            public int failures = 0;
            //store off mapLength values when re-randomizing on loss 
            public int mapLength;

            [NonSerialized]
            public EnemySpawnProfile profile;
        }
    }
}
