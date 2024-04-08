using ClassLibrary1.Pieces;
using System;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;

namespace ClassLibrary1
{
    public static class Consts
    {
        public const double PathMinSeparation = Math.PI * 1.3;
        public const double PathWidth = 16.9;
        public const double PathWidthDev = .13;
        public const double PathWidthMin = 9.1;

        public const double CaveDistance = 130;
        public const double CaveDistanceDev = .13;
        public const double CaveDistanceOE = .169;
        public const double CaveMinDist = 65;
        public const double CaveSize = 13;
        public const double CaveDistPow = 1.13;

        public const double ResourceAvgDist = 16.9;

        public const double ResearchFactor = 2100;

        public const double EnemyStartEnergy = 6500;
        public const double EnemyEnergy = 390;
        public const double EnemyEnergyRampTurns = 91;
        public const double DifficultyIncTurns = 78;
        public const double DifficultyEnergyPow = 1.69;
        public const double DifficultyResearchPow = 1.3;
        public const double DifficultyMoveDirPow = .52;
        public const double DistanceMoveDirPow = .26;

        public const double MoveDev = .013;
        public const double MoveLimitPow = 1.3;

        public const double ExtractorCostPow = .91;
        public const double ResourceDistAdd = 21;
        public const double ResourceDistDiv = 65;
        public const double ResourceDistPow = .52;
        public const double ResourceSustainValuePow = .26;
        public const double ExtractTurns = 65;
        public const double ExtractPow = .78 / (1 - .78); // x/(1-x) where x is desired exponent when sustain=1
        public const double ExtractSustainPow = .39;
        public const double ExtractorSustainCostPow = .65;
        public const double ResourceDev = .21;
        public const double ResourceOE = .26;

        public const double BiomassEnergyInc = 117;
        public const double BiomassSustain = .78;
        public const double BiomassResearchIncDiv = 50;
        public const double MetalMassInc = 52;
        public const double MetalSustain = 1.17;
        public const double MetalEnergyUpkDiv = 4;
        public const double ArtifactResearchInc = 14.3;
        public const double ArtifactSustain = 1.69;
        public const double ArtifactMassIncDiv = 3;
        public const double ArtifactEnergyUpkMult = 2.5;

        public const double BiomassExtractorEnergyCost = 780;
        public const double BiomassExtractorMassCost = 650;
        public const double MetalExtractorEnergyCost = 1170;
        public const double MetalExtractorMassCost = 390;
        public const double ArtifactExtractorEnergyCost = 2100;
        public const double ArtifactExtractorMassCost = 260;

        public const int EnergyForFabricateMass = 10;
        public const int BurnMassForEnergy = 2;
        public const int MassForScrapResearch = 5; // inverted value from the other two

        public const double BaseConstructorUpkeep = 5;
        public const double BaseMechUpkeep = 1;

        public const double EnergyPerMove = 1;
        public const double EnergyPerAttack = EnergyPerShield / 2.0;
        public const double EnergyPerShield = .5;
        public const double MassPerArmor = EnergyPerShield / 2.0;
        public const double RegenCostPassiveDiv = 2;

        public const double MechCostMult = .13;
        public const double MechMassDiv = 1.69;

        public const double RepairCost = .169;
        public const double PassiveRepairCost = .13;
        public const double EnergyRepairDiv = 2.1;
        public const int AutoRepair = 1;
        public const double ReplaceRefundPct = .8;

        //public static double? StatValue(double? stat)
        //{
        //    if (stat.HasValue)
        //        return StatValue(stat.Value);
        //    return null;
        //}
        public static double StatValue(double stat)
        {
            if (stat < 0) throw new Exception();
            return stat * stat + stat;
        }
        public static double StatValueInverse(double value)
        {
            if (value < 0) throw new Exception();
            return (Math.Sqrt(4 * value + 1) - 1) / 2.0;
        }
        //public static double SumStats(IEnumerable<int> stats)
        //{
        //    return SumStats(stats.Select(s => (double)s));
        //}
        //public static double SumStats(IEnumerable<double> stats)
        //{
        //    return StatValueInverse(stats.Sum(StatValue));
        //}
        public static double MoveValue(IMovable.Values? movable)
        {
            double move = 0;
            if (movable.HasValue)
            {
                var m = movable.Value;
                move = 8 * m.MoveInc / 1.0 + 2 * m.MoveMax / 2.1 + 1 * m.MoveLimit / 3.9;
            }
            move /= 8 + 2 + 1;
            return move;
        }

        internal static int Income(int cur, double income)
        {
            double avg = cur + Income(income);

            int div = (Game.Rand.OEInt(.39) + 1) * 5;
            div = Game.Rand.WeightedInt(div, 1 - .21 / Math.Sqrt(div));
            if (div < 1)
                div = 5;
            return Game.Rand.Round(avg / div) * div;
        }
        internal static double Income(double income)
        {
            return income + Game.Rand.Gaussian(.65 + Math.Abs(income) / 65.0);
        }

        internal static double GetPct(double pct, double mult)
        {
            return 1 - Math.Pow(1 - pct, mult);
        }

        internal static bool CanRepair(Piece piece) => !(piece.GetBehavior<IMovable>()?.Moved ?? false)
            && !(piece.GetBehavior<IKillable>()?.Defended ?? false)
            && !(piece.GetBehavior<IAttacker>()?.Attacked ?? false);
        public static double GetRepairCost(Piece piece, double energy, double mass)
        {
            double costMult = piece.HasBehavior<IAttacker>() ? Consts.RepairCost : Consts.PassiveRepairCost;
            return (mass + energy / Consts.EnergyRepairDiv) * costMult;
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
            IKillable killable = piece.GetBehavior<IKillable>();
            //if (piece != null && piece.HasBehavior(out IKillable killable))
            //{
            double resilience = killable.Resilience;
            if (sqrt)
                resilience = Math.Sqrt(resilience);
            return min + (value - min) * Math.Pow(killable.Hits.DefenseCur / (double)killable.Hits.DefenseMax, 1 - resilience);
            //}
            //return value;
        }

        public static int IncDefense(DefenseType type, bool isAttacker, int cur, int max, bool moved, bool attacked, bool defended, bool inRepair, ref double energyUpk, ref double massUpk)
        {
            int newValue = cur;
            int regen = CombatTypes.GetRegen(type, moved, attacked, defended, inRepair);
            if (regen > 0)
            {
                double costMult = CombatTypes.GetRegenCostMult(type, isAttacker, out bool mass);
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
