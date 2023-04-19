using DynamicEnums;
using HarmonyLib;
using LevelGeneration;
using LevelGeneration.WorldMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Profiling;
using static LevelGeneration.SpawnWaveProfile;
using static LevelGeneration.WorldMap.TerritoryInstance;
using static TechTreeMaster;

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
                bool hideRewards = HideRewards(territory);

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

        public static void Randomize()
        {
            Dictionary<string, SpawnAverages> spawnAverages = AggregateSpawnInfo();
            List<SpawnerInfo> saveSpawnInfo = new();
            foreach (TerritoryInstance territory in Plugin.Rand.Iterate(Map.Territories))
                GenerateSpawnData(spawnAverages, territory, saveSpawnInfo);
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
            CountTechTypes(spawnWaveProfile, spawnAverages.Count, out int spawnTechs, out int totalTechs);

            //especially impactful tech types, in order of impact
            string[] heroTypes = new string[] { "Hind", "Bubba", "Rocket", "Predator" };

            //keep techTypes distinct
            HashSet<string> techTypes = new();

            //we need to know the highest difficultyCurve values to ensure we only select units that can actually spawn
            AnimationCurve difficultyCurve = (AnimationCurve)Operations.difficultyCurve.GetValue(spawnWaveProfile);
            float maxDifficulty = difficultyCurve.keys.Max(k => k.value);
            Plugin.Log.LogInfo("maxDifficulty: " + maxDifficulty);

            SpawnProfile[] enemySpawnProfiles = new SpawnProfile[spawnTechs];
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

                    SpawnAverages values = SelectTechType(spawnAverages, heroTypes, techTypes, maxDifficulty, mult);

                    //randomize spawn amounts, based on island-wide averages
                    const double deviation = .13;
                    int countMin = Plugin.Rand.GaussianCappedInt((values.countMin) * mult, deviation, values.countMin > 1 ? 1 : 0);
                    int countMax = Plugin.Rand.GaussianCappedInt((values.countMax - values.countMin + 1) * mult + countMin, deviation, countMin);
                    if (territory.specialTag == SpecialTag.None && Plugin.Rand.Bool())
                    {
                        //chance to widen range
                        countMax += countMin;
                        countMin = 0;
                    }
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

                    SpawnerInfo info = Plugin.Rand.SelectValue(values.info
                        .Where(spawnerInfo => spawnerInfo.StartAtDifficulty < maxDifficulty));
                    info.SetInfo(territory.index, idx, countMin, countMax, capMin, capMax);
                    AssignSpawnData(info);
                    saveSpawnInfo.Add(info);

                    EnemySpawnProfile profile = info.profile;
                    SpawnerData data = profile.UnitSpawnData;

                    LogInfo(spawnWaveProfile, values, info, profile, data);

                    enemySpawnProfiles[idx] = profile;

                    //chance to hide certain techs when total is over 10, so that more build sites will show up
                    if (totalTechs > 10 && Plugin.Rand.Bool()
                            && new HashSet<string>() { "PistolPip", "Shotgunner", "Warfighter", "UAZ", "Warmule", "DuneBuggy" }.Contains(values.techType))
                    {
                        Plugin.Log.LogInfo($"hiding from recon lineup: {values.techType} ({totalTechs})");
                        profile.displayInReconLineup = false;
                        totalTechs--;
                    }
                }

            //reassign to territory
            spawnWaveProfile.enemySpawnProfiles = enemySpawnProfiles;
            //difficulty add-ons were already included in the aggregation step
            spawnWaveProfile.enemySpawnProfilesAtDifficulty = new SpawnWaveProfileAtDifficulty[0];
            //randomize build site display order (end may wind up getting cut off)
            Plugin.Rand.Shuffle(spawnWaveProfile.enemyBuildSites);
        }
        private static void CountTechTypes(SpawnWaveProfile spawnWaveProfile, int spawnTypes, out int spawnTechs, out int totalTechs)
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
            //randomize count somewhat 
            while (spawnTechs > 1 && spawnTechs < spawnTypes && (totalTechs != 10 || Plugin.Rand.Bool()) && Plugin.Rand.Bool())
            {
                int mod = Plugin.Rand.Bool() ? -1 : 1;
                spawnTechs += mod;
                totalTechs += mod;
            }

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
        private static List<EnemySpawnProfile> GetProfiles(SpawnWaveProfile spawnWaveProfile) =>
            spawnWaveProfile.ReturnSpawnProfilesPerDifficulty(Map.MissionManagerAsset.GameDifficultyIndex)
            .Cast<EnemySpawnProfile>().ToList();
        private static SpawnAverages SelectTechType(Dictionary<string, SpawnAverages> spawnAverages, string[] heroTypes, HashSet<string> techTypes, float maxDifficulty, double mult)
        {
            //only select units that can actually spawn with this difficulty curve
            spawnAverages = spawnAverages.Where(pair => pair.Value.MinStartAtDifficulty < maxDifficulty)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

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
                bool hasType = techTypes.Contains(spawnAverages.techType);
                int chance = (hasType ? 0 : spawnAverages.territories) + (alwaysAllow == spawnAverages.techType ? 1 : 0);
                //certain tech types more likely to appear in special missions
                if (!hasType && heroTypes.Contains(spawnAverages.techType))
                    chance = Plugin.Rand.Round((1 + chance) * mult);
                Plugin.Log.LogDebug($"{spawnAverages.techType}: {chance}");
                return chance;
            });
            techTypes.Add(values.techType);

            //reduce probability of being selected in the future 
            if (values.territories > 0)
                values.territories--;

            return values;
        }
        private static void AssignSpawnData(SpawnerInfo info)
        {
            EnemySpawnProfile profile = UnityEngine.Object.Instantiate(info.profile);
            SpawnerData data = profile.UnitSpawnData;
            spawnCountMin.SetValue(data, info.countMin);
            spawnCountMax.SetValue(data, info.countMax);
            spawnCapMin.SetValue(data, info.capMin);
            spawnCapMax.SetValue(data, info.capMax);
            info.profile = profile;
        }
        private static void LogInfo(SpawnWaveProfile spawnWaveProfile, SpawnAverages values, SpawnerInfo info, EnemySpawnProfile profile, SpawnerData data)
        {
            Plugin.Log.LogInfo($"{spawnWaveProfile.name} {values.techType} ({profile.GetInstanceID()}): {info.countMin}-{info.countMax} ({info.capMin}-{info.capMax})");

            Plugin.Log.LogInfo("StartAtDifficulty: " + data.StartAtDifficulty);
            foreach (var c in data.SpawnConditions)
                Plugin.Log.LogError("SpawnCondition: " + c);

            Plugin.Log.LogDebug("SpawnCapCycleMultipler: " + data.SpawnCapCycleMultipler);
            Plugin.Log.LogDebug("SpawnDelay: " + data.SpawnDelay(1) + "-" + data.SpawnDelay(0));
            Plugin.Log.LogDebug("TimeBetweenClusters: " + data.TimeBetweenClusters);
            if (data.SpawnTech is CalldownType)
                Plugin.Log.LogDebug($"CalldownType: {data.SpawnTech.name}");
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
            IsShrouded(territory) && territory.specialTag == TerritoryInstance.SpecialTag.None;
        private static bool HideRewards(TerritoryInstance territory) =>
            IsShrouded(territory) || territory.specialTag == TerritoryInstance.SpecialTag.HighValueReward;

        private class SpawnAverages
        {
            public string techType;
            public List<SpawnerInfo> info = new();
            public double number, countMin, countMax, capMin, capMax;
            public int territories;

            public float MinStartAtDifficulty => info.Min(profile => profile.StartAtDifficulty);
        }
        [Serializable]
        private class SpawnerInfo
        {
            public SpawnerInfo(int copyFromTerritoryIdx, int copyFromProfileIdx, EnemySpawnProfile profile)
            {
                if (copyFromProfileIdx < 0)
                    throw new ArgumentException("invalid copyFromProfileIdx");

                this.copyFromTerritoryIdx = copyFromTerritoryIdx;
                this.copyFromProfileIdx = copyFromProfileIdx;
                this.profile = profile;
            }
            internal void SetInfo(int territoryIdx, int profileIdx, int countMin, int countMax, int capMin, int capMax)
            {
                this.territoryIdx = territoryIdx;
                this.profileIdx = profileIdx;
                this.countMin = countMin;
                this.countMax = countMax;
                this.capMin = capMin;
                this.capMax = capMax;
            }

            //territory index after shuffle
            public int territoryIdx, copyFromTerritoryIdx;
            //index into ReturnSpawnProfilesPerDifficulty list
            public int profileIdx, copyFromProfileIdx;

            public int countMin, countMax, capMin, capMax;

            [NonSerialized]
            public EnemySpawnProfile profile;

            public float StartAtDifficulty => profile?.UnitSpawnData.StartAtDifficulty ?? 2;
        }
    }
}
