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

        public static void UpdateShroud(WorldMapUIController worldMapUIController, MissionManagerAsset missionManagerAsset)
        {
            TerritoryInstance[] territories = missionManagerAsset.CurrentWorldMap.territories;
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
                    reward.isMysteryItem = (GameRandom.HiddenRewards[rewardIndex] || hideRewards) && !reward.item.extraLife;
                    rewardIndex++;
                }
            }
        }
    }
}
