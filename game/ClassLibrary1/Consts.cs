﻿using ClassLibrary1.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;

namespace ClassLibrary1
{
    public static class Consts
    {
        //public const int MinPaths = 3;
        //public static readonly int MaxPaths = Game.Rand.RangeInt(3, 4);
        public const double PathMinSeparation = Math.PI * 1.3;
        public const double PathWidth = 16.9;
        public const double PathWidthDev = .13;
        public const double PathWidthMin = 9.1;

        public const double ResourceAvgDist = 21;

        public const double ResearchFactor = 2600;

        public const double EnemyStartEnergy = 5200;
        public const double EnemyEnergy = 260;
        public const double EnemyEnergyRampTurns = 52;
        public const double DifficultyIncTurns = 91;
        public const double DifficultyEnergyPow = 1.69;
        public const double DifficultyResearchPow = 1.3;
        public const double DifficultyMoveDirPow = .65;
        public const double DistanceMoveDirPow = .39;

        public const double HitsIncDev = .065;
        public const double MoveDev = .013;
        public const double MoveLimitPow = 1.3;

        public const double ResourceDistAdd = 21;
        public const double ResourceDistDiv = 52;
        public const double ResourceDistPow = .39;
        public const double ResourceSustainValuePow = .169;
        public const double ExtractTurns = 52;
        public const double ExtractPower = .65 / (1 - .65); // x/(1-x) where x is desired power when sustain=1
        public const double ExtractSustainPow = .26;
        public const double ResourceDev = .21;
        public const double ResourceOE = .26;

        public const double BiomassEnergyInc = 104;
        public const double BiomassSustain = .78;
        public const double BiomassResearchIncDiv = 50;
        public const double MetalMassInc = 52;
        public const double MetalSustain = 1.17;
        public const double MetalEnergyUpkDiv = 10;
        public const double ArtifactResearchInc = 9.1;
        public const double ArtifactSustain = 1.69;
        public const double ArtifactMassIncDiv = 3;
        public const double ArtifactEnergyUpkDiv = 1;

        public const double BiomassExtractorEnergyCost = 780;
        public const double BiomassExtractorMassCost = 650;
        public const double MetalExtractorEnergyCost = 1300;
        public const double MetalExtractorMassCost = 390;
        public const double ArtifactExtractorEnergyCost = 2100;
        public const double ArtifactExtractorMassCost = 169;

        public const int EnergyForFabricateMass = 10;
        public const int BurnMassForEnergy = 2;
        public const int MassForScrapResearch = 5; // inverted value from the other two

        public const double BaseConstructorUpkeep = 5;
        public const double BaseMechUpkeep = 1;
        //public const double WeaponRechargeUpkeep = 10;

        public const double EnergyPerMove = 1;
        public const double EnergyPerAttack = .25;
        public const double EnergyPerShield = .5;
        public const double MassPerArmor = .25;

        public const double MechCostMult = .13;
        //public const double MechStatMult = .13;
        public const double MechMassDiv = 1.69;

        public const double RepairCost = .21;
        //public const double RechargeCost = .169;
        public const double EnergyRepairDiv = 2.1;
        public const double AutoRepair = .65;
        public const double AutoRepairPct = .0169;
        public const double ReplaceRefundPct = .8;

        public static double StatValue(double stat)
        {
            return stat * stat + stat;
        }
        public static double StatValueInverse(double value)
        {
            return (Math.Sqrt(4 * value + 1) - 1) / 2.0;
        }
        public static double SumStats(IEnumerable<int> stats)
        {
            return SumStats(stats.Select(s => (double)s));
        }
        public static double SumStats(IEnumerable<double> stats)
        {
            return StatValueInverse(stats.Sum(StatValue));
        }

        internal static int Income(double income)
        {
            return Game.Rand.Round(income + Game.Rand.Gaussian(.65 + Math.Abs(income) / 130.0));
        }

        internal static double GetPct(double pct, double mult)
        {
            return 1 - Math.Pow(1 - pct, mult);
        }

        public static double GetRepairCost(double energy, double mass)
        {
            return (mass + energy / Consts.EnergyRepairDiv) * Consts.RepairCost;
        }
        //public static double GetRechargeCost(double energy, double mass)
        //{
        //    return energy * Consts.RechargeCost;
        //}

        public static double GetDamagedValue(Piece piece, double value, double min)
        {
            return GetDamagedValue(piece, value, min, false);
        }
        private static double GetDamagedValue(Piece piece, double value, double min, bool sqrt)
        {
            if (piece.HasBehavior(out IKillable killable))
            {
                double resilience = killable.Resilience;
                if (sqrt)
                    resilience = Math.Sqrt(resilience);
                return min + (value - min) * Math.Pow(killable.Hits.DefenseCur / (double)killable.Hits.DefenseMax, 1 - resilience);

                //return min + (value - min) * Math.Sqrt(killable.DefenseCur / (double)killable.DefenseMax);
            }
            return value;
        }

        public static int IncDefense(DefenseType type, int cur, int max, bool inRepair, ref double energyUpk, ref double massUpk)
        {
            int newValue = cur;
            int regen = CombatTypes.GetRegen(type, inRepair);
            if (regen > 0)
            {
                double costMult = CombatTypes.GetRegenCostMult(type, out bool mass);
                double upkeep = 0;
                newValue = IncStatValue(cur, max, regen, costMult, ref upkeep);
                if (mass)
                    massUpk += upkeep;
                else
                    energyUpk += upkeep;
            }
            return newValue;
        }
        public static int IncStatValue(int cur, int max, int regen, double upkeepRate, ref double upkeep)
        {
            int newValue = cur;
            if (cur < max)
            {
                newValue = Math.Min(max, cur + regen);
                double cost = StatValue(newValue) - StatValue(cur);
                cost *= upkeepRate;
                upkeep += cost;
            }
            return newValue;
        }
        public static double IncValueWithMaxLimit(double cur, double inc, double dev, double max, double limit, double pow, bool rand)
        {
            double start = cur;
            if (inc > 0)
            {
                double startMax = Math.Max(cur, max);

                if (rand)
                    inc = Game.Rand.GaussianCapped(inc, dev);
                cur += inc;

                double extra = cur - startMax;
                if (extra > 0)
                {
                    limit -= startMax;
                    double mult = limit / (limit + max);
                    extra *= Math.Pow(mult, pow);
                    extra += startMax;

                    cur = extra;
                }

                //Debug.WriteLine(cur);
            }
            return cur - start;
        }
    }
}
