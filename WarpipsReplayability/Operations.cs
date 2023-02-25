using LevelGeneration;
using LevelGeneration.WorldMap;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarpipsReplayability
{
    internal class Operations
    {
        //hiddenRewards is reset whenever the worldMapIndex changes 
        private static int WorldMapIndex { get; set; }
        private static bool[] HiddenRewards { get; set; }

        static Operations()
        {
            Reset();
        }
        public static void Reset()
        {
            Plugin.Log.LogInfo("Reset Operations");

            WorldMapIndex = -1;
            HiddenRewards = null;
        }

        public static void UpdateShroud(WorldMapUIController worldMapUIController, MissionManagerAsset missionManagerAsset)
        {
            TerritoryInstance[] territories = missionManagerAsset.CurrentWorldMap.territories;
            InitHiddenRewards(Map.MissionManagerAsset.WorldMapIndex, territories);
            int rewardIndex = 0;

            foreach (var territory in territories)
            {
                bool shrouded = !worldMapUIController.IsTerritoryAttackable(territory.index);
                bool hideEnemies = shrouded && territory.specialTag == TerritoryInstance.SpecialTag.None;
                bool hideRewards = shrouded || territory.specialTag == TerritoryInstance.SpecialTag.HighValueReward;

                Operation operation = territory.operation;
                //revealEnemyIcons needs to be high enough to reveal all icons
                //the field is only used for integer comparison so it doesn't matter if it's arbitrarily large
                int revealEnemyIcons = hideEnemies ? 0 : 99;
                operation.revealEnemyIcons = revealEnemyIcons;

                int seed = operation.operationName.GetHashCode();

                foreach (var reward in operation.itemRewards)
                {
                    reward.isMysteryItem = (HiddenRewards[rewardIndex] || hideRewards) && !reward.item.extraLife;
                    rewardIndex++;
                }
            }
        }

        private static void InitHiddenRewards(int mapIndex, TerritoryInstance[] territories)
        {
            if (WorldMapIndex != mapIndex)
            {
                WorldMapIndex = mapIndex;
                var operations = territories.Select(t => t.operation);

                //we need to seed a constant PRNG that will persist through saving/loading but be different for each playthrough
                //the humorous randomly generated operation names are the perfect cadidate for this
                uint[] seed = operations.Select(o => (uint)o.operationName.GetHashCode()).ToArray();
                Plugin.Log.LogInfo($"PRNG seed: {seed.Select(s => s.ToString("X8")).Aggregate("", (a, b) => a + b)}");

                MTRandom seededRand = new(seed);
                //each reward has a small chance to remain hidden
                HiddenRewards = operations.SelectMany(o => o.itemRewards).Select(r => seededRand.Bool(.169)).ToArray();
            }
        }

        // TODO: need to be able to save state to accomplish this
        //private static HashSet<Reward>[] hiddenRewards = new HashSet<Reward>[4];
        //private static HashSet<Reward> GetHidden(TerritoryInstance[] territories)
        //{
        //    int mapIndex = Map.MissionManagerAsset.WorldMapIndex;
        //    HashSet<Reward> hidden = hiddenRewards[mapIndex];
        //    return hidden ??= hiddenRewards[mapIndex] =
        //        territories.SelectMany(t => t.operation.itemRewards).Where(r => r.isMysteryItem).ToHashSet();
        //}
    }
}
