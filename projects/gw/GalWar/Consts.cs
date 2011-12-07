using System;
using System.Collections.Generic;

namespace GalWar
{
    public static class Consts
    {
        public const double WinPointsMult = 130;
        public const double WinPointsTilesPower = .39;
        //as a multiple of the second place players research
        public const double ResearchVictoryMult = 1.5;

        public const double StartPopulation = 130;
        //StartGold will be divided by the number of planets per player and by each players homeworld quality
        public const double StartGold = 2 * AverageQuality * 260;
        public const double StartResearch = 390;
        public const double StartRndm = .09;
        public const double StartMinMult = .6;
        public const double MoveOrderGold = AverageQuality * Income;
        //a higher MoveOrderShuffle makes the move order change faster
        public const double MoveOrderShuffle = .13;
        //multiplied by the number of starting planets
        public const double PlanetCreationRate = 1 / 100.0;

        public const int PlanetQualityMin = 0;
        public const int PlanetQualityMax = 390;
        public const double PlanetQualityOE = 66.6;
        public const double AverageQuality = ( PlanetQualityMin + PlanetQualityMax ) / 2.0 + PlanetQualityOE;
        //as minimum number of hexes in between planets
        public const int PlanetDistance = 3;
        public const int HomeworldDistance = 6;

        //as pcts of population
        public const double PopulationGrowth = .021;
        public const double Income = .13;
        //set up so that emphasising a single value allows double the income of when emphasising the other two
        public const double EmphasisValue = 1.6861406616345072; // = 8 / ( Math.Sqrt(33) - 1 )

        //pcts of production lost
        public const double SwitchBuildTypeLossPct = 1.0 / 2;    // .50
        public const double SwitchBuildLossPct = 1.0 / 3;        // .33
        public const double ManualObsoleteLossPct = 1.0 / 6;     // .17
        public const double StoreProdLossPct = 1.0 / 7;          // .14
        public const double CarryProductionLossPct = 1.0 / 13;   // .08
        public const double AutomaticObsoleteLossPct = 1.0 / 21; // .05

        public const double CostMult = .13;
        public const double CostUpkeepPct = .21;
        public const double ProdUpkeepMult = 1 / ( 1 / Consts.CostUpkeepPct - 1 ) / 2.1;
        public const double BaseDesignHPMult = .3;
        //percent of upkeep returned when a ship doesnt move
        public const double UpkeepUnmovedReturn = .169;
        //as a multiple of upkeep payoff
        public const double MinCostMult = 1;
        //as a multiple and a power of research
        public const double MaxCostMult = 66.6;
        public const double MaxCostPower = 0.21;

        //higher value makes research less useful
        public const double ResearchFactor = 1300;
        //how often new designs are researched
        public const float NewResearchFactor = 390;
        //mult and power of turn research income
        public const double ResearchIncMult = 2.6;
        public const double ResearchIncPower = .39;
        //mult and power of number of existing designs
        public const double NumDesignsFactor = 3.9;
        public const double NumDesignsPower = .21;
        //turn research income randomness
        public const float ResearchRndm = .3f;

        //trade rates
        public const double ProductionForGold = 3;
        public const double GoldProductionForGold = ProductionForGold / 1.3;
        public const double GoldForProduction = 2;
        public const double PopulationForGold = 1 / Income / 2.1;
        public const double PopulationIncomeForGold = 1 / Income / 13.0;
        public const float ProductionForSoldiers = 1.39f;
        public const double SoldiersForGold = ProductionForGold / ProductionForSoldiers * 1.3;
        public const float DefendingSoldiersForGold = (float)( SoldiersForGold * 1.3 );
        //ExpForGold will be increased by the players most recent research
        public const double ExpForGold = 1 / DisbandPct;

        public const double MovePopulationGoldCost = Income / 2;
        public const float MoveSoldiersMult = 3.9f;
        public const float SoldiersRndm = .26f;
        //rate for losing troops when a transport is damaged
        public const double TransLossPctPower = 1.3;
        public const double TransLossMult = .666;
        public const double TransLossRndm = .39;

        //value of exp gained as a pct of ship value
        public const double ExperienceMult = .13;
        //extra bonus for destroying a ship, as a pct of HP
        public const double ExperienceDestroyMult = .09;
        //damage amount for constant exp every combat round, in addition to standard exp for actual damage
        public const double ExperienceConstDmgAmt = 1;
        //randomness for exp gained and needed
        public const float ExperienceRndm = .21f;
        //modifier to upkeep payoff when gaining levels
        public const double ExperienceUpkeepPayoffMult = 1 / RepairCostMult;

        //pct of original ship production cost
        public const double RepairCostMult = .3;
        //base for exponential gold repair cost
        public const double RepairGoldIncPowBase = 2.1;
        //pct of HP that can be repaired at mult*base^1 cost
        public const double RepairGoldHPPct = 1 / 16.9;

        public const double DisbandPct = RepairCostMult;
        public const double DisbandHitPctPower = 1;
        public const double ColonizationValueGoldCost = 1 / 1.69;
        //extra bonus to the production of a new colony, in addition to standard disband amount
        public const double ColonizationBonusPct = ( 1 + ColonizationBonusMoveFactor ) * ( .666 - DisbandPct );
        //a lower ColonizationBonusMoveFactor means a greater bonus reduction for faster colony ships
        public const double ColonizationBonusMoveFactor = 2.1;
        public const double ColonizationHitPctPower = .9;

        public const double AttackStrength = 1;
        public const double AttackNumbersPower = 0.09;
        public const double DefenseStrength = 1.13;
        public const double DefenseNumbersPower = 0.03;
        public const double TroopExperienceMult = 1 / PopulationForGold / 2.6;
        //maximum for random pct bonus to troop combat strength
        public const double InvadeMultRandMax = .39;
        //payoff power for gold used to boost a planetary invasion
        public const double InvadeGoldIncPower = .3;
        //average planet quality lost as a percentage of total troops killed in the battle
        public const double PlanetDamageAvg = .3;

        public const float BombardAttackMult = .039f;
        public const double DeathStarDamageRndm = .13;
        //multiplyer to planet quality lost when bombarded by a death star
        public const double DeathStarPlanetDamageMult = .5 / PlanetDamageAvg;
        //multiplyer to population lost when bombarding a friendly planet with a death star
        public const double DeathStarFriendlyPopDamageMult = .26;

        public const float PlanetDefensesRndm = .169f;
        public const double PlanetDefensesCostMult = 1.3;
        public const double PlanetDefensesUpkeepMult = .666;
        public const double PlanetDefensesAttackCostMult = .39;
        public const float PlanetDefensesSoldiersMult = (float)( DisbandPct / ProductionForSoldiers * 1.3 );
        public const double PlanetDefensesDeathStarMult = 1 / PopulationForGold / 1.3;

        internal const double FLOAT_ERROR = 0.000000071711838245391846;

        internal static double GetMoveOrderGold(int numPlayers)
        {
            return MoveOrderGold / ( numPlayers - 1.0 );
        }

        public static double GetProductionUpkeepMult(int mapSize)
        {
            return Consts.ProdUpkeepMult / GetUpkeepPayoff(mapSize, false, 0, 2.1);
        }

        //upkeep payoff is the number of turns the ship is expected to live
        public static double GetUpkeepPayoff(int mapSize, bool colony, int trans, int speed)
        {
            return GetUpkeepPayoff(mapSize, colony, trans, (double)speed);
        }

        //upkeep payoff is the number of turns the ship is expected to live
        private static double GetUpkeepPayoff(int mapSize, bool colony, int trans, double speed)
        {
            return Math.Sqrt(mapSize) * ( colony ? 0.39 : .666 ) * ( ( Math.Pow(trans, 0.169) + 39.0 ) / 39.0 ) * ( 4.2 / ( speed + 2.1 ) );
        }

        //randomized
        public static double GetInvasionStrength(int attackers, double soldiers, int initialWave, double gold)
        {
            return RandomizeInvasionStr(GetInvasionStrengthBase(attackers, soldiers, initialWave, gold));
        }

        //non-randomized
        public static double GetInvasionStrengthBase(int attackers, double soldiers, int initialWave, double gold)
        {
            return GetStrengthBase(attackers, soldiers, GetInvasionBonus(initialWave, gold) * AttackStrength, AttackNumbersPower);
        }

        private static double GetInvasionBonus(int initialWave, double gold)
        {
            if (gold == 0)
                return 1;
            return ( initialWave + gold * Math.Pow(initialWave / gold, InvadeGoldIncPower) ) / (double)initialWave;
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
            double retVal = strength * Math.Pow(troops, power);
            if (troops > 0)
                retVal *= Math.Pow(1 + soldiers / troops, 1 + power);
            else if (soldiers > FLOAT_ERROR)
                throw new Exception();
            return retVal;
        }

        public static double GetPlanetDamageMult()
        {
            return Game.Random.Weighted(Consts.PlanetDamageAvg);
        }

        public static int GetPlanetDamage(int population)
        {
            return Game.Random.WeightedInt(population, (float)Consts.PlanetDamageAvg);
        }

        private static double RandomizeInvasionStr(double str)
        {
            return str * ( 1 + Game.Random.DoubleHalf(Consts.InvadeMultRandMax) );
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
