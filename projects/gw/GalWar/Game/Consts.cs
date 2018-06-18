using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;

namespace GalWar
{
    public static class Consts
    {
        public const double WinPointsMult = 130;
        public const double LosePointsMult = -6.5;
        public const double PointsTilesPower = .39;
        //ResearchVictoryMult is a multiple of the second place players research
        public const double ResearchVictoryMult = 1.69;
        public const double ResearchVictoryMin = 1.39;
        public const double ResearchVictoryPow = Math.PI;

        //StartAnomalies is the number of turn-rounds for which we immediately create anomalies
        public const double StartAnomalies = 13;
        public const double StartPopulation = 130;
        //StartGold will be divided by the number of planets per player and by each indivual player's homeworld quality
        public const double StartGold = ( AverageQuality + Consts.PlanetConstValue ) * 910;
        //StartGoldProdPct is the percent of total starting gold that will be converted into starting production
        public const double StartGoldProdPct = .21;
        public const double StartResearch = 390;
        public const double StartRndm = .13;
        public const double StartMinMult = .65;
        public const double MoveOrderGold = AverageQuality * Income;
        //a higher MoveOrderShuffle makes the move order change faster
        public const double MoveOrderShuffle = .13;

        public const int PlanetQualityMin = 0;
        public const int PlanetQualityMax = 390;
        public const double PlanetQualityOE = 65;
        public const double AverageQuality = ( PlanetQualityMin + PlanetQualityMax ) / 2.0 + PlanetQualityOE;
        public const double PlanetConstValue = .52 / PopulationGrowth;
        //as minimum number of hexes in between planets
        public const int PlanetDistance = 3;
        public const int HomeworldDistance = 8;

        //as pcts of population
        public const double PopulationGrowth = Math.E / 130.0;
        public const double Income = .13;
        //emphasizing a single value gives on average precisely double the income of when emphasizing the other two
        public static readonly double EmphasisValue = 8 / ( Math.Sqrt(33) - 1 );

        public const double SwitchBuildTypeLossPct = .39;       //  2.6
        public const double SwitchBuildLossPct = .3;            //  3.3
        public const double ManualObsoleteLossPct = .21;        //  4.8
        public const double StoreProdLossPct = .169;            //  5.9
        public const double CarryProductionLossPct = .091;      // 11.0
        public const double AutomaticObsoleteLossPct = .065;    // 15.4

        public const double CostMult = .104;
        public const double CostUpkeepPct = .21;
        public const double ProdUpkeepMult = 1 / ( 1 / CostUpkeepPct - 1 ) / 1.3;
        public const double BaseDesignHPMult = .3;
        //percent of upkeep returned when a ship doesnt move
        public const double UpkeepUnmovedReturn = .169;
        //as a multiple of upkeep payoff
        public const double MinCostMult = .91;
        //as a multiple and a power of research
        public const double MaxCostMult = 65;
        public const double MaxCostPower = 0.21;

        //higher value makes research less useful
        public const double ResearchFactor = 1690;
        //inverse chance of a new design
        public const double NewDesignFactor = 13;
        //minimum stored research mult
        public const double MinStoredResearchFactor = 5.2;
        //mult and power of turn research income
        public const double NewResearchMult = 1.3;
        public const double NewResearchPower = .52;
        //mult and power of number of existing designs
        public const double NumDesignsFactor = 13;
        public const double NumDesignsPower = .65;
        //research for upgrading a design
        public const int UpgDesignResearch = 520;
        public const double UpgDesignRndm = .117;
        public const int UpgDesignMin = 260;
        public const int UpgDesignAbsMin = 169;
        //research income and display randomness
        public const double ResearchRndm = .39;
        public const double ResearchDisplayRndm = .169;

        //trade rates
        public const double ProductionForGold = 1 / .3;
        public static readonly double GoldProductionForGold = Math.Sqrt(ProductionForGold);
        public const double GoldForProduction = 2;
        public const double PopulationForGoldLow = 1 / Income / 2.6;
        public const double PopulationForGoldMid = 1 / Income / 5.2;
        public const double PopulationForGoldHigh = 1 / Income / 13.0;
        public const double ProductionForSoldiers = .91;
        public const double SoldierUpkeepMult = .26;
        public const double MoveSoldiersCost = .39;
        public const double ExpForSoldiers = ProductionForSoldiers / 1.3;
        public const double SoldiersForGold = ProductionForGold / ProductionForSoldiers;
        //ExpForGold will be increased by the players most recent research
        public const double ExpForGold = 1 / DisbandPct;

        public const double MovePopulationCost = Income / 2.0;
        public const double MoveSoldiersMult = 2.1;
        //rate for losing troops when a transport is damaged
        public const double TransLossPctPower = 1.3;
        public const double TransLossMult = .65;
        public const double TransLossRndm = .3;

        //value of exp gained as a pct of ship value
        public const double ExperienceMult = .13;
        //extra bonus for destroying a ship, as a pct of HP
        public const double ExperienceDestroyMult = .091;
        //damage amount for constant exp every combat round, in addition to standard exp for actual damage
        public const double ExperienceConstDmgAmt = .65;
        //randomness for exp gained and needed
        public const double ExperienceRndm = .21;
        //modifier to upkeep payoff when gaining levels
        public const double ExperienceUpkeepPayoffMult = 1 / RepairCostMult;

        //pct of original ship production cost
        public const double RepairCostMult = .3;
        //base for exponential gold repair cost
        public const double RepairGoldIncPowBase = 2.1;
        //pct of HP that can be repaired at mult*base^1 cost
        public const double RepairGoldHPPct = 1 / 16.9;

        public const double DisbandPct = RepairCostMult;
        public const double ColonizationCostRndm = .078;
        public const double AnomalyQualityCostMult = 1.69;
        public const double TerraformQuality = AverageQuality * .65;

        public const double InvadeStrength = 1;
        public const double InvadeNumbersPower = .13;
        public const double InvadeGoldStrength = 1.69;
        public const double InvadeGoldIncPower = .39;
        public const double InvadeDefenseStrength = 1.69;
        public const double InvadeDefenseNumbersPower = .039;
        //maximum for random pct bonus to troop combat strength
        public const double InvadeMultRandMax = Math.PI * .13;
        //PlanetDamage is average planet quality lost as a percentage of total troops killed in the battle
        public const double PlanetDamage = .3;
        public const double TroopExperienceMult = 1 / PopulationForGoldMid / 5.2;

        public const double DeathStarDamageRndm = .26;
        //multiplyer to planet quality lost when bombarded by a death star
        public const double DeathStarPlanetDamage = .5;

        public const double PlanetDefenseStatRndm = .091;
        public const double PlanetDefenseBuildRndm = Math.PI / 13.0;
        public const double PlanetDefensesCostMult = .78;
        //percent of research that automatically upgrades planet defenses
        public const double PlanetDefensesUpgradePct = .52;
        //PlanetDefensesUpkeepMult will be multiplied by ProdUpkeepMult
        public const double PlanetDefensesUpkeepMult = .65;
        public const double PlanetDefensesAttackCostMult = PlanetDefensesUpkeepMult * .39;
        public const double BombardFreeDmgMult = 1 / PopulationForGoldMid / 1.3;

        public const double FLOAT_ERROR_ZERO = 1.0 / ( 1 << ( MTRandom.FLOAT_BITS - 2 ) );
        public const double FLOAT_ERROR_ONE = 1.0 + FLOAT_ERROR_ZERO;

        public static double GetResearchVictoryChance(double mult)
        {
            //research victory can happen when the top player exceeds a certain multiple of the second place player
            if (mult > Consts.ResearchVictoryMin)
            {
                double chance = ( mult - Consts.ResearchVictoryMin ) / ( Consts.ResearchVictoryMult - Consts.ResearchVictoryMin );
                return Math.Pow(Consts.LimitPct(chance), Consts.ResearchVictoryPow);
            }
            return 0;
        }

        internal static int NewPlanetQuality()
        {
            return Game.Random.OEInt(PlanetQualityOE) + Game.Random.RangeInt(PlanetQualityMin, PlanetQualityMax);
        }
        internal static int TerraformPlanetQuality()
        {
            return Game.Random.GaussianOEInt(TerraformQuality, 2.1, PlanetQualityOE / AverageQuality, Game.Random.Round(PlanetConstValue));
        }

        public static double GetMoveOrderGold(IEnumerable<Player> players)
        {
            return ( MoveOrderGold + players.Average(player => player.GetTotalIncome()) ) / 2.0 / ( players.Count() - 1.0 );
        }

        internal static double GetExperience(double experience)
        {
            return Game.Random.GaussianOE(experience, ExperienceRndm, ExperienceRndm);
        }

        public static double GetPopulationGrowth(double population, int quality)
        {
            //approximately logistic growth, but modified such that quality is irrelevant until it is exceeded by population
            double growth;
            if (population > quality)
                growth = 2 * quality - population;
            else
                growth = population;

            //plus 1 constant as a bonus for acquiring new planets before population exceeds quality on existing planets
            //and to make even pitiful planets have a small carrying capacity
            return ( 1 + growth * PopulationGrowth );
        }

        internal static double GetColonizationMult()
        {
            return Game.Random.GaussianOE(1, ColonizationCostRndm, ColonizationCostRndm, .39);
        }

        internal static double GetColonizationCost(double value, double mult)
        {
            return 0.78 * mult * value * Math.Pow(value / ( AverageQuality + PlanetConstValue ), .65);
        }

        public static double GetMovePopCost(double mapSize, int population, double soldiers)
        {
            return population * Consts.MovePopulationCost + Consts.GetSoldierUpkeep(mapSize, soldiers) * Consts.MoveSoldiersCost;
        }

        public static double GetProductionUpkeepMult(double mapSize)
        {
            return ProdUpkeepMult / GetUpkeepPayoff(mapSize, 1, 1, 2.1);
        }

        public static double GetSoldierUpkeep(PopCarrier popCarrier)
        {
            return GetSoldierUpkeep(popCarrier.Player.Game.MapSize, popCarrier.Soldiers);
        }
        public static double GetSoldierUpkeep(double mapSize, double soldiers)
        {
            return Consts.SoldierUpkeepMult * Consts.ProductionForSoldiers * GetProductionUpkeepMult(mapSize) * soldiers;
        }

        public static double GetGoldRepairCost(int hp, int maxHP, double repairCost)
        {
            return hp * repairCost * Math.Pow(Consts.RepairGoldIncPowBase, hp / (double)maxHP / Consts.RepairGoldHPPct);
        }

        //upkeep payoff is the number of turns the ship is expected to live
        public static double GetUpkeepPayoff(double mapSize, double nonColonyPct, double nonTransPct, int speed)
        {
            return GetUpkeepPayoff(mapSize, nonColonyPct, nonTransPct, (double)speed);
        }

        //upkeep payoff is the number of turns the ship is expected to live
        private static double GetUpkeepPayoff(double mapSize, double nonColonyPct, double nonTransPct, double speed)
        {
            return ( .78 * Math.Sqrt(mapSize) * ScalePct(.39, 1, nonColonyPct) * ScalePct(1.69, 1, nonTransPct) * ( 4.2 / ( speed + 2.1 ) ) );
        }

        internal static double ScalePct(double zero, double one, double pct)
        {
            return ( zero + ( one - zero ) * pct );
        }

        public static double GetNonColonyPct(int att, int def, int hp, int speed, int trans, bool colony, double bombardDamage, Game game, bool sqr)
        {
            double retVal = 1;
            if (colony)
            {
                retVal = ShipDesign.GetTotCost(att, def, hp, speed, trans / ( 1.0 + 13 / (double)trans ), false, bombardDamage, game.AvgResearch)
                        / ShipDesign.GetTotCost(att, def, hp, speed, trans, colony, bombardDamage, game.AvgResearch);
                if (sqr)
                    retVal *= retVal;
            }
            return retVal;
        }

        public static double GetNonTransPct(int att, int def, int hp, int speed, int trans, bool colony, double bombardDamage, Game game, bool sqr)
        {
            double retVal = 1;
            if (trans > 0)
            {
                retVal = ShipDesign.GetTotCost(att, def, hp, speed, 0, colony, bombardDamage, game.AvgResearch)
                        / ShipDesign.GetTotCost(att, def, hp, speed, trans, colony, bombardDamage, game.AvgResearch);
                if (sqr)
                    retVal *= retVal;
            }
            return retVal;
        }

        //randomized
        internal static double GetInvadeStrength(int attackers, double soldiers, int gold, double totalDefense)
        {
            int initialWave;
            return RandomizeInvadeStrength(GetInvadeStrength(attackers, soldiers, gold, totalDefense, out initialWave));
        }
        //non-randomized
        public static double GetInvadeStrengthBase(int attackers, double soldiers, int gold, double defenseBase)
        {
            int min, max;
            double maxStr = GetInvadeStrength(attackers, soldiers, gold, defenseBase, out min);
            double minStr = GetInvadeStrength(attackers, soldiers, gold, defenseBase * ( 1 + InvadeMultRandMax ), out max);
            double avgStr = ( maxStr + minStr ) / 2.0;

            if (attackers > 1)
            {

                if (( max == attackers ) && ( min == 1 ) && max - min > 1)
                {
                }

                double lowPct = 0, highPct = 0;
                if (max == attackers)
                    lowPct = FindInvadeStrengthPct(attackers, soldiers, gold, defenseBase, false);
                if (min == 1)
                    highPct = FindInvadeStrengthPct(attackers, soldiers, gold, defenseBase, true);
                avgStr = minStr * lowPct + maxStr * highPct + avgStr * ( 1 - lowPct - highPct );
            }

            return avgStr;
        }
        private static double FindInvadeStrengthPct(int attackers, double soldiers, int gold, double defenseBase, bool high)
        {
            checked
            {
                const int max = int.MaxValue - 1;
                double interval = InvadeMultRandMax / (double)max;
                int retVal = TBSUtil.FindValue(delegate (int value)
                {
                    int ret;
                    GetInvadeStrength(attackers, soldiers, gold, defenseBase * ( 1 + interval * value ), out ret);
                    return ( high ? ret > 1 : ret < attackers );
                }, 0, max, high);
                if (!high)
                    retVal = ( max - retVal );
                return retVal / ( 1.0 + max );
            }
        }

        private static double GetInvadeStrength(int attackers, double soldiers, int gold, double totalDefense, out int initialWave)
        {
            double attack = GetInvadeStrengthBase(attackers, soldiers, InvadeStrength, InvadeNumbersPower);
            initialWave = TBSUtil.FindValue(value => ( value * GetInvadeStrength(value, attack, gold) > totalDefense ), 1, attackers, true);
            return GetInvadeStrength(initialWave, attack, gold);
        }
        private static double GetInvadeStrength(int initialWave, double attack, int gold)
        {
            if (gold == 0)
                return attack;
            return attack + InvadeGoldStrength * Math.Pow(initialWave / (double)gold, InvadeGoldIncPower) * gold / (double)initialWave;
        }

        //randomized
        internal static double GetInvadeDefenseStrength(int population, double soldiers)
        {
            return RandomizeInvadeStrength(GetInvadeDefenseStrengthBase(population, soldiers));
        }
        //non-randomized
        public static double GetInvadeDefenseStrengthBase(int population, double soldiers)
        {
            return GetInvadeStrengthBase(population, soldiers, InvadeDefenseStrength, InvadeDefenseNumbersPower);
        }

        private static double GetInvadeStrengthBase(int troops, double soldiers, double strength, double power)
        {
            if (troops > 0)
                return strength * Math.Pow(troops + soldiers, power) * ( 1 + soldiers / (double)troops );
            throw new Exception();
        }

        private static double RandomizeInvadeStrength(double str)
        {
            return str * ( 1 + Game.Random.DoubleHalf(InvadeMultRandMax) );
        }

        public static int GetPlanetDamage(int population)
        {
            return Game.Random.WeightedInt(population, PlanetDamage);
        }

        public static double GetTransLoss(Ship ship, double damage)
        {
            if (ship == null)
                return 0;
            if (damage >= ship.MaxHP)
                return ship.Population;
            return ( damage / (double)ship.MaxHP * ship.MaxPop * Consts.TransLossMult
                    * Math.Pow(ship.Population / (double)ship.MaxPop, Consts.TransLossPctPower) );
        }

        public static double GetBombardDamage(double att)
        {
            return Math.Pow(att, 1.69) * 0.0091;
        }

        public static IDictionary<int, double> GetDamageTable(int att, int def, out double avgAtt, out double avgDef)
        {
            avgAtt = 0;
            avgDef = 0;
            IDictionary<int, double> damageTable = new SortedDictionary<int, double>();
            for (int a = 0 ; a <= att ; ++a)
                for (int d = 0 ; d <= def ; ++d)
                {
                    int damage = a - d;
                    if (damage > 0)
                        avgAtt += damage;
                    else
                        avgDef -= damage;
                    double val;
                    damageTable.TryGetValue(damage, out val);
                    damageTable[damage] = val + 1;
                }
            double mult = att / ( att + 1.0 ) / ( def + 1.0 );
            avgAtt *= mult;
            avgDef *= mult;
            return damageTable;
        }

        public static double LimitPct(double chance)
        {
            if (chance > .5)
                chance /= ( chance + .5 );
            return chance;
        }

        public static double LimitMin(double value, double min)
        {
            if (value < min)
                value = min * min / ( 2.0 * min - value );
            return value;
        }
    }
}
