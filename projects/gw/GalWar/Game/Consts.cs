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
        public const double ResearchVictoryMinMult = 1.3;
        public const double ResearchVictoryRndm = 0.065;

        //StartAnomalies is the number of turn-rounds for which we immediately create anomalies
        public const double StartAnomalies = 21;
        public const double StartPopulation = 130;
        //StartGold will be divided by the number of planets per player and by each players homeworld quality
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
        public const double PlanetConstValue = .5 / Consts.PopulationGrowth;
        //as minimum number of hexes in between planets
        public const int PlanetDistance = 3;
        public const int HomeworldDistance = 6;

        //as pcts of population
        public const double PopulationGrowth = Math.E / 130.0;
        public const double Income = .13;
        //emphasising a single value gives on average precisely double the income of when emphasising the other two
        public static readonly double EmphasisValue = 8.0 / ( Math.Sqrt(33.0) - 1.0 );

        public const double SwitchBuildTypeLossPct = .39;       //  2.6
        public const double SwitchBuildLossPct = .3;            //  3.3
        public const double ManualObsoleteLossPct = .21;        //  4.8
        public const double StoreProdLossPct = .169;            //  5.9
        public const double CarryProductionLossPct = .091;      // 11.0
        public const double AutomaticObsoleteLossPct = .065;    // 15.4

        public const double CostMult = .104;
        public const double CostUpkeepPct = .21;
        public const double ProdUpkeepMult = 1 / ( 1 / Consts.CostUpkeepPct - 1 ) / 1.3;
        public const double BaseDesignHPMult = .3;
        //percent of upkeep returned when a ship doesnt move
        public const double UpkeepUnmovedReturn = .169;
        //as a multiple of upkeep payoff
        public const double MinCostMult = 1.13;
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
        public const double ProductionForSoldiers = .52;
        public const double ExpForSoldiers = ProductionForSoldiers / 1.3;
        public const double SoldiersForGold = ProductionForGold / ProductionForSoldiers;
        //ExpForGold will be increased by the players most recent research
        public const double ExpForGold = 1 / DisbandPct;

        public const double MovePopulationGoldCost = Income / 2.0;
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
        public const double AttackNumbersPower = 0.091;
        public const double DefenseStrength = 1.13;
        public const double DefenseNumbersPower = 0.03;
        public const double TroopExperienceMult = 1 / PopulationForGoldMid / Math.E;
        //maximum for random pct bonus to troop combat strength
        public const double InvadeMultRandMax = Math.PI * .13;
        //payoff power for gold used to boost a planetary invasion
        public const double InvadeGoldIncPower = .3;
        //average planet quality lost as a percentage of total troops killed in the battle
        public const double PlanetDamage = .3;

        public const double DeathStarDamageRndm = .26;
        //multiplyer to planet quality lost when bombarded by a death star
        public const double DeathStarPlanetDamage = .5;

        public const double PlanetDefenseStatRndm = .091;
        public const double PlanetDefenseBuildRndm = Math.PI / 13.0;
        public const double PlanetDefensesCostMult = .78;
        //PlanetDefensesUpkeepMult will be multiplied by ProductionUpkeepMult
        public const double PlanetDefensesUpkeepMult = .65;
        public const double PlanetDefensesAttackCostMult = PlanetDefensesUpkeepMult * .39;
        public const double BombardFreeDmgMult = 1 / PopulationForGoldMid / 1.3;

        public const double FLOAT_ERROR = 1.0 / ( 1 << 20 );

        internal static int NewPlanetQuality()
        {
            return Game.Random.OEInt(Consts.PlanetQualityOE) + Game.Random.RangeInt(Consts.PlanetQualityMin, Consts.PlanetQualityMax);
        }

        public static double GetMoveOrderGold(int numPlayers)
        {
            return MoveOrderGold / ( numPlayers - 1.0 );
        }

        internal static double GetExperience(double experience)
        {
            return Game.Random.GaussianOE(experience, Consts.ExperienceRndm, Consts.ExperienceRndm);
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
            return ( 1 + growth * Consts.PopulationGrowth );
        }

        internal static double GetColonizationMult()
        {
            return Game.Random.GaussianOE(1, Consts.ColonizationCostRndm, Consts.ColonizationCostRndm, .39);
        }

        internal static double GetColonizationCost(double value, double mult)
        {
            return 0.78 * mult * value * Math.Pow(value / ( AverageQuality + Consts.PlanetConstValue ), .65);
        }

        public static double GetProductionUpkeepMult(int mapSize)
        {
            return Consts.ProdUpkeepMult / GetUpkeepPayoff(mapSize, 1, 1, 2.1);
        }

        //upkeep payoff is the number of turns the ship is expected to live
        public static double GetUpkeepPayoff(int mapSize, double nonColonyPct, double nonTransPct, int speed)
        {
            return GetUpkeepPayoff(mapSize, nonColonyPct, nonTransPct, (double)speed);
        }

        //upkeep payoff is the number of turns the ship is expected to live
        private static double GetUpkeepPayoff(int mapSize, double nonColonyPct, double nonTransPct, double speed)
        {
            return ( .78 * Math.Sqrt(mapSize) * ScalePct(.39, 1, nonColonyPct) * ScalePct(1.69, 1, nonTransPct) * ( 4.2 / ( speed + 2.1 ) ) );
        }

        internal static double ScalePct(double zero, double one, double pct)
        {
            return ( zero + ( one - zero ) * pct );
        }

        public static double GetNonColonyPct(int att, int def, int hp, int speed, int trans, bool colony, double bombardDamage, double research, bool sqr)
        {
            double retVal = 1;
            if (colony)
            {
                retVal = ShipDesign.GetTotCost(att, def, hp, speed, trans / ( 26.0 / (double)trans + 1 ), false, bombardDamage, research)
                        / ShipDesign.GetTotCost(att, def, hp, speed, trans, colony, bombardDamage, research);
                if (sqr)
                    retVal *= retVal;
            }
            return retVal;
        }

        public static double GetNonTransPct(int att, int def, int hp, int speed, int trans, bool colony, double bombardDamage, double research)
        {
            if (trans > 0)
            {
                double retVal = ShipDesign.GetTotCost(att, def, hp, speed, 0, colony, bombardDamage, research)
                        / ShipDesign.GetTotCost(att, def, hp, speed, trans, colony, bombardDamage, research);
                return ( retVal * retVal );
            }
            return 1;
        }

        //randomized
        public static double GetInvasionStrength(int attackers, double soldiers, int gold, double totalDefense)
        {
            return GetInvasionStrength(attackers, soldiers, gold, totalDefense, true);
        }

        //non-randomized
        public static double GetInvasionStrengthBase(int attackers, double soldiers, int gold, double totalDefense)
        {
            return GetInvasionStrength(attackers, soldiers, gold, totalDefense, false);
        }

        private static double GetInvasionStrength(int attackers, double soldiers, int gold, double totalDefense, bool randomize)
        {
            double attack = GetStrengthBase(attackers, soldiers, AttackStrength, AttackNumbersPower);
            if (randomize)
                attack = RandomizeInvasionStr(attack);
            return GetInvasionStrength(TBSUtil.FindValue(delegate(int initialWave)
            {
                return ( initialWave * GetInvasionStrength(initialWave, attack, gold) > totalDefense );
            }, 1, attackers, true), attack, gold);
        }

        private static double GetInvasionStrength(int initialWave, double attack, int gold)
        {
            if (gold == 0)
                return attack;
            return ( initialWave + gold * Math.Pow(initialWave / (double)gold, InvadeGoldIncPower) ) * attack / (double)initialWave;
        }

        //randomized
        public static double GetPlanetDefenseStrength(int population, double soldiers)
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
                return strength * Math.Pow(troops, power) * Math.Pow(1 + soldiers / (double)troops, 1 + power);
            throw new Exception();
        }

        public static int GetPlanetDamage(int population)
        {
            return Game.Random.WeightedInt(population, Consts.PlanetDamage);
        }

        private static double RandomizeInvasionStr(double str)
        {
            return str * ( 1 + Game.Random.DoubleHalf(Consts.InvadeMultRandMax) );
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
    }
}
