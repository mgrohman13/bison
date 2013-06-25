using System;
using System.Collections.Generic;
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
        public const double ResearchVictoryMin = 1.5;
        public const double ResearchVictoryPow = 5.2;

        //StartAnomalies is the number of turn-rounds for which we immediately create anomalies
        public const double StartAnomalies = 13;
        public const double StartPopulation = 130;
        //StartGold will be divided by the number of planets per player and by each indivual player's homeworld quality
        public const double StartGold = AverageQuality * 650;
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
        //emphasising a single value gives on average precisely double the income of when emphasising the other two
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
        public const double ProductionForGold = 10.0 / 3.0;
        public const double GoldProductionForGold = ProductionForGold / 1.3;
        public const double GoldForProduction = 2;
        public const double PopulationForGoldLow = 1 / Income / 2.1;
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
        public const double ColonizationCostRndm = .104;

        public const double AttackStrength = 1;
        public const double GoldStrength = 1.69;
        public const double DefenseStrength = 1.69;
        public const double AttackNumbersPower = .13;
        public const double DefenseNumbersPower = .039;
        //payoff power for gold used to boost a planetary invasion
        public const double InvadeGoldIncPower = .39;
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
        public const double PlanetDefensesCostMult = .65;
        //PlanetDefensesUpkeepMult will be multiplied by ProductionUpkeepMult
        public const double PlanetDefensesUpkeepMult = .65;
        public const double PlanetDefensesAttackCostMult = PlanetDefensesUpkeepMult * .39;
        public const double BombardFreeDmgMult = 1 / PopulationForGoldMid / 1.3;

        public const double FLOAT_ERROR = 1.0 / ( 1 << 20 );

        public static double GetResearchVictoryChance(double mult, double numPlayers)
        {
            //research victory can happen when the top player exceeds a certain multiple of the second place player
            if (mult > Consts.ResearchVictoryMin)
            {
                double chance = ( mult - Consts.ResearchVictoryMin ) / ( Consts.ResearchVictoryMult - Consts.ResearchVictoryMin );
                chance = Math.Pow(Consts.LimitPct(chance), Consts.ResearchVictoryPow);
                return ( 1 - Math.Pow(1 - chance, 1 / numPlayers) );
            }
            return 0;
        }

        internal static int NewPlanetQuality()
        {
            return Game.Random.OEInt(PlanetQualityOE) + Game.Random.RangeInt(PlanetQualityMin, PlanetQualityMax);
        }

        public static double GetMoveOrderGold(int numPlayers)
        {
            return MoveOrderGold / ( numPlayers - 1.0 );
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
            return GetSoldierUpkeep(popCarrier.Tile.Game.MapSize, popCarrier.Soldiers);
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

        public static double GetNonColonyPct(int att, int def, int hp, int speed, int trans, bool colony, double bombardDamage, int lastResearched, bool sqr)
        {
            double retVal = 1;
            if (colony)
            {
                retVal = ShipDesign.GetTotCost(att, def, hp, speed, trans / ( 13 / (double)trans + 1 ), false, bombardDamage, lastResearched)
                        / ShipDesign.GetTotCost(att, def, hp, speed, trans, colony, bombardDamage, lastResearched);
                if (sqr)
                    retVal *= retVal;
            }
            return retVal;
        }

        public static double GetNonTransPct(int att, int def, int hp, int speed, int trans, bool colony, double bombardDamage, int lastResearched, bool sqr)
        {
            double retVal = 1;
            if (trans > 0)
            {
                retVal = ShipDesign.GetTotCost(att, def, hp, speed, 0, colony, bombardDamage, lastResearched)
                        / ShipDesign.GetTotCost(att, def, hp, speed, trans, colony, bombardDamage, lastResearched);
                if (sqr)
                    retVal *= retVal;
            }
            return retVal;
        }

        //randomized
        internal static double GetInvasionStrength(int attackers, double soldiers, int gold, double totalDefense)
        {
            int initialWave;
            return RandomizeInvasionStr(GetInvasionStrength(attackers, soldiers, gold, totalDefense, out initialWave));
        }

        //non-randomized
        public static double GetInvasionStrengthBase(int attackers, double soldiers, int gold, double defenseBase)
        {
            int min, max;
            double maxStr = GetInvasionStrength(attackers, soldiers, gold, defenseBase, out min);
            double minStr = GetInvasionStrength(attackers, soldiers, gold, defenseBase * ( 1 + InvadeMultRandMax ), out max);
            double avgStr = ( maxStr + minStr ) / 2.0;

            if (attackers > 1)
            {

                if (( max == attackers ) && ( min == 1 ) && max - min > 1)
                {
                }

                double lowPct = 0, highPct = 0;
                if (max == attackers)
                    lowPct = GetStrPct(attackers, soldiers, gold, defenseBase, false);
                if (min == 1)
                    highPct = GetStrPct(attackers, soldiers, gold, defenseBase, true);
                avgStr = minStr * lowPct + maxStr * highPct + avgStr * ( 1 - lowPct - highPct );
            }

            return avgStr;
        }
        private static double GetStrPct(int attackers, double soldiers, int gold, double defenseBase, bool high)
        {
            checked
            {
                const int max = int.MaxValue - 1;
                double interval = InvadeMultRandMax / (double)max;
                int retVal = TBSUtil.FindValue(delegate(int value)
                {
                    int ret;
                    GetInvasionStrength(attackers, soldiers, gold, defenseBase * ( 1 + interval * value ), out ret);
                    return ( high ? ret > 1 : ret < attackers );
                }, 0, max, high);
                if (!high)
                    retVal = ( max - retVal );
                return retVal / ( 1.0 + max );
            }
        }

        private static double GetInvasionStrength(int attackers, double soldiers, int gold, double totalDefense, out int initialWave)
        {
            double attack = GetStrengthBase(attackers, soldiers, AttackStrength, AttackNumbersPower);
            initialWave = TBSUtil.FindValue(delegate(int value)
            {
                return ( value * GetInvasionStrength(value, attack, gold) > totalDefense );
            }, 1, attackers, true);
            return GetInvasionStrength(initialWave, attack, gold);
        }
        private static double GetInvasionStrength(int initialWave, double attack, int gold)
        {
            if (gold == 0)
                return attack;
            return attack + GoldStrength * Math.Pow(initialWave / (double)gold, InvadeGoldIncPower) * gold / (double)initialWave;
        }

        //randomized
        internal static double GetPlanetDefenseStrength(int population, double soldiers)
        {
            return RandomizeInvasionStr(GetPlanetDefenseStrengthBase(population, soldiers));
        }

        //non-randomized
        public static double GetPlanetDefenseStrengthBase(int population, double soldiers)
        {
            return GetStrengthBase(population, soldiers, DefenseStrength, DefenseNumbersPower);
        }

        private static double GetStrengthBase(int troops, double soldiers, double strength, double power)
        {
            if (troops > 0)
                return strength * Math.Pow(troops + soldiers, power) * ( 1 + soldiers / (double)troops );
            throw new Exception();
        }

        public static int GetPlanetDamage(int population)
        {
            return Game.Random.WeightedInt(population, PlanetDamage);
        }

        private static double RandomizeInvasionStr(double str)
        {
            return str * ( 1 + Game.Random.DoubleHalf(InvadeMultRandMax) );
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

        public static Dictionary<int, double> GetDamageTable(int att, int def, out double avgAtt, out double avgDef)
        {
            avgAtt = 0;
            avgDef = 0;
            Dictionary<int, double> damageTable = new Dictionary<int, double>();
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
    }
}
