using LevelGeneration;
using LevelGeneration.WorldMap;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarpipsReplayability.Mod
{
    internal class Operations
    {
        public static bool[] RollHiddenRewards()
        {
            var hiddenRewards = Map.Territories.SelectMany(t =>
            {
                //each territory has a variable chance for each reward to remain hidden
                int chance = Plugin.Rand.GaussianOEInt(5.2, .21, .26, 1);
                //each reward has an individual chance to remain hidden
                return t.operation.itemRewards.Select(r => Plugin.Rand.Next(chance) == 0);
            }).ToArray();
            Plugin.Log.LogInfo($"generated HiddenRewards");
            return hiddenRewards;
        }
        public static void UpdateShroud(WorldMapUIController worldMapUIController)
        {
            int rewardIndex = 0;
            foreach (var territory in Map.Territories)
            {
                bool shrouded = !worldMapUIController.IsTerritoryAttackable(territory.index);
                bool hideEnemies = shrouded && territory.specialTag == TerritoryInstance.SpecialTag.None;
                bool hideRewards = shrouded || territory.specialTag == TerritoryInstance.SpecialTag.HighValueReward;

                Operation operation = territory.operation;
                //revealEnemyIcons needs to be high enough to reveal all icons
                //the field is only used for integer comparison so it doesn't matter if it's arbitrarily large
                int revealEnemyIcons = hideEnemies ? 0 : 99;
                operation.revealEnemyIcons = revealEnemyIcons;

                foreach (var reward in operation.itemRewards)
                {
                    reward.isMysteryItem = (Persist.Instance.HiddenRewards[rewardIndex] || hideRewards) && !reward.item.extraLife;
                    rewardIndex++;
                }
            }
        }
    }
}
