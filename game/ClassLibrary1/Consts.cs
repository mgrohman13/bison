using ClassLibrary1.Pieces;
using System;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;

namespace ClassLibrary1
{
    public static class Consts
    {
        public const double Scale = Math.E * .26;

        public const double PathMinSeparation = Scale * Math.PI * 1.3;
        public const double PathWidth = Scale * 16.9;
        public const double PathWidthDev = .13;
        public const double PathWidthMin = Scale * 9.1;
        public const double FeatureDist = Scale * 169;
        public const double FeatureMin = FeatureDist / Math.PI;
        public static readonly double NoiseDistance = CaveDistance / Math.Sqrt(Scale);

        public const double CaveDistance = Scale * 210;
        public const double CaveDistanceDev = .13;
        public const double CaveDistanceOE = .169;
        public const double CaveMinDist = Scale * 91;
        public const double CaveSize = Scale * 13;
        public static readonly double CavePathSize = Math.Sqrt(Scale) * 2.6;
        public const double CaveDistPow = 1.13;

        public static readonly double ResourceAvgDist = Math.Sqrt(Scale) * 21;
        public static readonly double TreasureDiv = Scale * 21;
        public const double ResearchFactor = 2600;
        public const int ExploreForResearch = 39;

        public const double EnemyStartEnergy = 10400;
        public const double EnemyEnergy = 520;
        public const double ExploreEnergy = 6500;
        public const double EnemyEnergyRampTurns = EnemyUnlockTurns;
        public const double EnemyUnlockTurns = 169;
        public const double DifficultyIncTurns = 91;
        public const double DifficultyEnergyPow = 1.3;
        public const double DifficultyResearchPow = 3.9 / Math.E;
        public const double DifficultyAIPow = .52;
        public const double PortalSpawnTime = 52;
        public const double PortalSpawnStrMult = 1.69;
        public const double PortalCost = 13;
        public const double PortalExitDef = 520;
        public const double PortalEntranceDef = 210;
        public const double PortalDecayRate = 7.8;
        public const double PortalRewardPct = .39;

        public const double MoveDev = .013;
        public const double MoveLimitPow = 1.3;

        public const double CoreEnergyLow = 260;
        public const double CoreEnergyMid = 91;
        public const double CoreEnergyHigh = CoreEnergy - CoreEnergyMid - CoreEnergyLow;
        public const double CoreEnergy = 550;
        public const double CoreMass = 250;
        public const double CoreResearch = 20;
        public const double CoreExtractTurns = 91;//0.98901098901098901098901098901099

        public const double DroneCost = 16.9;
        public const double DroneMassCostMult = 1.69;
        public const double DroneRefund = .65;

        public const double ResourceDistAdd = 21;
        public const double ResourceDistDiv = 65;
        public const double ResourceDistPow = .52;
        public const double ResourceSustainValuePow = .26;
        public const double ExtractTurns = 65;//0.98461538461538461538461538461538
        //ExtractPow=x/(1-x) where x is desired exponent when sustain=1 
        //inverse: x=ExtractPow/(ExtractPow+1)
        public const double ExtractPow = 3.5; //x=0.77777777777777777777777777777778
        public const double ExtractSustainPow = .26;//.39?
        public const double ExtractorSustainCostPow = .65;
        public const double ExtractorHitsPow = .39;//.26??
        public const double ResourceDev = .21;
        public const double ResourceOE = .26;

        public const double BiomassEnergyInc = 117;
        public const double BiomassSustain = .78;
        public const double BiomassResearchDiv = 8;
        public const double BiomassResearchPow = .39;
        public const double MetalMassInc = 52;
        public const double MetalSustain = 1.17;
        public const double MetalEnergyUpkDiv = 4;
        public const double ArtifactResearchInc = 10.4;
        public const double ArtifactSustain = 1.69;
        public const double ArtifactMassIncDiv = 3;
        public const double ArtifactEnergyUpkMult = 2.5;

        public const double ExtractorCostPow = .91;
        public const double BiomassExtractorEnergyCost = 750;
        public const double BiomassExtractorMassCost = 910;
        public const double MetalExtractorEnergyCost = 780;
        public const double MetalExtractorMassCost = 400;
        public const double ArtifactExtractorEnergyCost = 1300;
        public const double ArtifactExtractorMassCost = 300;

        public const double GeneratorEnergyInc = 35;
        public const double GeneratorResearchUpk = .5;
        public const double GeneratorEnergyCost = 225;
        public const double GeneratorMassCost = 550;

        public const int EnergyForFabricateMass = 10;
        public const int BurnMassForEnergy = 2;
        public const int MassForScrapResearch = 5; //inverted value from the other two
        public static readonly double ResearchMassConversion = MassForScrapResearch
            * Math.Sqrt(EnergyForFabricateMass * BurnMassForEnergy);
        public static readonly double ResearchEnergyConversion = ResearchMassConversion * EnergyMassRatio;

        public const double BaseConstructorUpkeep = 5;
        public const double BaseDroneUpkeep = 2;
        public const double BaseMechUpkeep = 1;

        public const double EnergyPerMove = 1 / 3.0;
        public const double EnergyPerAttack = EnergyPerShield / 2.0;
        public const double EnergyPerShield = 1;
        public const double MassPerArmor = EnergyPerShield / 2.0;
        public const double RegenCostPassiveDiv = 2;

        public const double MechCostMult = .13;
        public const double EnergyMassRatio = 1.69;

        public const double RepairCost = .169;
        public const double PassiveRepairCost = .13;
        public const double EnergyRepairDiv = 2.1;
        public const int AutoRepair = 1;
        public const double ReplaceRefundPct = .8;

        public static double StatValue(double stat)
        {
            if (stat < 0) throw new Exception();
            return (stat * stat + stat) / 2.0;
        }
        public static double StatValueInverse(double value)
        {
            if (value < 0) throw new Exception();
            return (Math.Sqrt(8 * value + 1) - 1) / 2.0;
        }
        //public static double SumStats(IEnumerable<int> stats)
        //{
        //    return SumStats(stats.Select(s => (double)s));
        //}
        //public static double SumStats(IEnumerable<double> stats)
        //{
        //    return StatValueInverse(stats.Sum(StatValue));
        //}
        public static double MoveValue(IMovable.Values? movable) =>
            MoveValue(movable?.MoveInc ?? 0, movable?.MoveMax ?? 0, movable?.MoveLimit ?? 0);
        public static double MoveValue(double moveInc, double moveMax, double moveLimit)
        {
            const double mi = 25, mm = 10, ml = 4;
            double move = mi * moveInc / 1.0 + mm * moveMax / 2.1 + ml * moveLimit / 3.9;
            move /= mi + mm + ml;
            return move;
        }

        internal static int Income(int cur, double income)
        {
            double avg = cur + Income(income);
            return IncomeRounding(avg);
        }
        internal static int IncomeRounding(double avg)
        {
            const int divMult = 5;
            int div = 1 + Game.Rand.OEInt(.52);
            if (Game.Rand.Bool())
                div = Game.Rand.WeightedInt(div * divMult, 1 - .21 / Math.Sqrt(div * divMult));
            else
                div = Game.Rand.RangeInt(0, div) * divMult;
            if (div < 1)
                div = Game.Rand.Bool() ? 1 : divMult;
            return Game.Rand.Round(avg / div) * div;
        }
        internal static double Income(double income) => income + Game.Rand.Gaussian(IncomeDev(income));
        public static double IncomeDev(double income) => .65 + Math.Abs(income) / 65.0;

        internal static double GetPct(double pct, double mult)
        {
            return 1 - Math.Pow(1 - pct, mult);
        }

        internal static bool CanRepair(Piece piece)
        {
            bool canRepair = !(piece.GetBehavior<IMovable>()?.Moved ?? false) && !(piece.GetBehavior<IKillable>()?.Defended ?? false) && !(piece.GetBehavior<IAttacker>()?.Attacked ?? false);
            if (canRepair && piece.Side.Mass < 0)
                canRepair = false;
            return canRepair;
        }
        public static double GetRepairCost(Piece piece, double energy, double mass)
        {
            double costMult = piece.HasBehavior<IAttacker>() ? RepairCost : PassiveRepairCost;
            return (mass + energy / EnergyRepairDiv) * costMult;
        }

        public static double GetDamagedValue(Piece piece, double value, double min) =>
            GetDamagedValue(piece, value, min, false);
        public static double GetDamagedValue(Piece piece, double value, double min, bool sqrt)
        {
            IKillable killable = piece.GetBehavior<IKillable>();
            double resilience = killable.Resilience;
            if (sqrt)
                resilience = Math.Sqrt(resilience);
            return min + (value - min)
                * Math.Pow(StatValue(killable.Hits.DefenseCur) / StatValue(killable.Hits.DefenseMax), (1 - resilience) / 2.0);
        }

        public static double IncDefense(bool doEndTurn, DefenseType type, bool isAttacker, int cur, int max, double repairAmt, ref double energyUpk, ref double massUpk)
        {
            double newValue = cur;
            double regen = CombatTypes.GetRegen(type, repairAmt);
            if (regen > 0)
            {
                double costMult = CombatTypes.GetRegenCostMult(type, isAttacker, out bool mass);
                double upkeep = 0;
                newValue = IncStatValue(doEndTurn, cur, max, regen, costMult, ref upkeep);
                if (mass)
                    massUpk += upkeep;
                else
                    energyUpk += upkeep;
            }
            return newValue;
        }
        public static double IncStatValue(bool doEndTurn, int cur, int max, double regen, double upkeepRate, ref double upkeep)
        {
            double newValue = cur;
            if (cur < max)
            {
                newValue = Math.Min(max, cur + regen);
                if (doEndTurn)
                    newValue = Game.Rand.Round(newValue);
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
            }
            return cur - start;
        }
    }
}
