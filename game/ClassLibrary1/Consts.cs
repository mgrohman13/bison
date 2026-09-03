using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Runtime.Serialization;
using static ClassLibrary1.Map.Map;
using DefenseType = ClassLibrary1.Pieces.Behavior.Combat.CombatTypes.DefenseType;

namespace ClassLibrary1
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Consts
    {
        internal const double MAX_ROUND = MTRandom.DOUBLE_DIV / MTRandom.DOUBLE_DIV_1;

        internal readonly double Scale;
        internal readonly double PathMinSeparation, PathWidth, PathDev, PathWidthMin, EnforcePathDist, FeatureDist, FeatureMin, NoiseDistance;
        internal readonly double CaveDistance, CaveDev, CaveOE, CaveMinDist, CaveSize, CavePathWidth;
        internal readonly double TreasureDiv;

        public readonly double ElevationDensity, ElevationMaxEffectDist, ElevationHeight;

        internal readonly double ResearchFactor;
        internal readonly double CombineResearchBuffer;
        internal readonly int ExploreForResearch;

        internal readonly double DifficultySetting, EnemyStartEnergy, ExploreEnergy, EnemyIncomeMatchFactor,
            EnemyEnergy, EnemyEnergyRampTurns, EnemyUnlockTurns, EnemyTreasureMatch, DifficultyIncTurns;
        internal readonly double DifficultyEnergyPow, DifficultyResearchPow, DifficultyAIPow, AgressionTurns;
        internal readonly double SpawnEnergyDiv, SpawnTurnAdd, SpawnTurnDiv, SpawnNeg;
        internal readonly double PortalSpawnTime, PortalSpawnStrMult, PortalCost, PortalExitDef, PortalEntranceDef,
            PortalDecayRate, PortalRewardPct, PortalLoan, PortalMinDist, PortalIncomeDiv;

        internal readonly double MoveDev, MoveLimitPow, MoveIncCost, MoveMaxCost, MoveLimitCost;

        public readonly double CoreEnergy, CoreMass, CoreResearch;
        internal readonly double CoreEnergyLow, CoreEnergyMid, CoreEnergyHigh, CoreExtractTurns, IncomeDevAdd, IncomeDevDiv;

        internal readonly double DroneCost, DroneMassCostMult, DroneRefund;

        internal readonly double ResourceAvgDist, ResourceDistAdd, ResourceDistDiv, ResourceDistPow, ResourceSustainValuePow,
            ExtractTurns, ExtractPow, ExtractSustainPow, ExtractorSustainCostPow, ExtractorResilienceCostPow, ExtractorHitsPow, ResourceDev, ResourceOE, FoundationAmt;

        internal readonly double BiomassEnergyInc, BiomassSustain, BiomassResearchDiv, BiomassResearchPow,
            MetalMassInc, MetalSustain, MetalEnergyUpkDiv, ArtifactResearchInc, ArtifactSustain, ArtifactMassIncDiv, ArtifactEnergyUpkMult;
        internal readonly double ExtractorCostPow, ExtractorResilience, BiomassExtractorEnergyCost, BiomassExtractorMassCost,
            MetalExtractorEnergyCost, MetalExtractorMassCost, ArtifactExtractorEnergyCost, ArtifactExtractorMassCost;

        internal readonly double GeneratorEnergyInc, GeneratorResearchUpk, GeneratorEnergyCost, GeneratorMassCost;

        public readonly int EnergyPerFabricateMass, BurnMassPerEnergy, MassForScrapResearch;
        public readonly double BaseConstructorUpkeep, BaseDroneUpkeep, BaseMechUpkeep;

        internal readonly double EnergyPerMove, EnergyPerAttack, EnergyPerShield, MassPerArmor, RegenCostPassiveDiv;

        internal readonly double MechCostMult, MechCostPow, EnergyMassRatio, TurretCost;

        internal readonly double CostAttMoveDiv, CostAttMovePow, CostVisionAdd, CostVisionMult, CostResilienceHitsPow, // CostResilienceHitsMult,
            CostReloadPow, CostResiliencePow, CostRangePow, CostMoveAdd, CostMoveMult, CostMovePow, CostStatsMult;

        internal readonly double MissileCostMult, MissileEnergyCostRatio, MissileHitRefundPct, MissileScrapRefund, MissileAttImmobileMult, MissileAttMaxMult;

        internal readonly double DisbandValue, RepairCost, PassiveRepairCost, EnergyRepairDiv, UpgRefundValue;
        internal readonly int AutoRepair;

        internal readonly double MassPerResearchConversion, GeneratorConstValue;

        //internal readonly double EnergyPerResearchConversion = MassPerResearchConversion * EnergyMassRatio;

        public Consts()
        {
            double Rand(double avg, double div = 1, double cap = .39) => RandCap(avg, Math.Pow(div, .91), avg
                * Game.Rand.GaussianCapped(cap, Dev(div), Math.Max(2 * cap - 1, 0)));
            double RandCap(double avg, double div = 1, double cap = 0) => Game.Rand.GaussianCapped(avg, Dev(div), cap);
            double Dev(double div) => Game.Rand.GaussianOE(.13 / div, .169, .065);

            EnergyPerFabricateMass = 10;
            BurnMassPerEnergy = 2;
            MassForScrapResearch = 5; //inverted value from the other two

            EnergyMassRatio = RandCap(1.69, div: 5, cap: 1);
            MassPerResearchConversion = Rand(MassForScrapResearch * Math.Sqrt(EnergyPerFabricateMass * BurnMassPerEnergy), div: 4, cap: .91);

            BaseConstructorUpkeep = 3;
            BaseDroneUpkeep = 2;
            BaseMechUpkeep = 1;

            EnergyPerMove = 1 / 3.0;
            EnergyPerShield = 1;
            EnergyPerAttack = EnergyPerShield / 2.0;
            MassPerArmor = EnergyPerShield / 2.0;
            RegenCostPassiveDiv = 2;

            MechCostMult = Rand(5.2, div: 2, cap: .52);
            MechCostPow = Rand(.65, div: 3, cap: .65);
            ResearchFactor = Rand(780 * Math.E, div: 4, cap: .91);
            CombineResearchBuffer = Rand(ResearchFactor, cap: .78);
            TurretCost = Rand(.78, div: 3, cap: .78);

            CostAttMoveDiv = Rand(3.9, div: 2, cap: .52);
            CostAttMovePow = Rand(.21, div: 3, cap: .65);
            CostVisionAdd = Rand(6.5, cap: .39);
            CostVisionMult = Rand(3.9, div: 2, cap: .52);
            //CostResilienceHitsMult = Rand(.78, div: 2, cap: .52);
            CostReloadPow = RandCap(1.3, div: 3, cap: 1);
            CostResiliencePow = Rand(.104, div: 3, cap: .78);
            CostResilienceHitsPow = RandCap(.21 / CostResiliencePow, div: 4, cap: 1);
            CostRangePow = RandCap(1.13, div: 4, cap: 1);
            CostMoveAdd = Rand(3.9, cap: .39);
            CostMoveMult = Rand(6.5, div: 2, cap: .52);
            CostMovePow = RandCap(1.5, div: 4, cap: 1.3);
            CostStatsMult = Rand(2.1, div: 2, cap: .52);

            DisbandValue = Rand(.26, div: 4, cap: .78);
            RepairCost = Rand(.169, div: 5, cap: .91);
            PassiveRepairCost = Rand(.91 * RepairCost, div: 4, cap: .91);
            EnergyRepairDiv = RandCap(1.3 * EnergyMassRatio, div: 4, cap: EnergyMassRatio);
            AutoRepair = 1;
            UpgRefundValue = Rand(.78, div: 3, cap: .78);

            Scale = Rand(Math.E * .21, div: 4, cap: .78);

            CaveMinDist = Rand(Scale * 91);
            CaveDistance = RandCap(Scale * 210, div: 3, cap: CaveMinDist);
            CaveDev = Rand(.13);
            CaveOE = Rand(.13);
            CaveSize = Rand(Scale * 13);
            CavePathWidth = Rand(Scale * 6.5);
            //CaveDistPow = Rand(1.13);

            PathWidthMin = Rand(Math.E, div: 2, cap: .65);
            PathWidth = RandCap(Scale * 16.9, div: 3, cap: PathWidthMin);
            PathDev = Rand(.21);
            PathMinSeparation = Rand(Scale * Math.PI * 1.3);
            EnforcePathDist = RandCap(Math.Sqrt(5), cap: 1);
            FeatureDist = Rand(Scale * 260, div: 2);
            FeatureMin = Rand(FeatureDist / Math.PI);
            NoiseDistance = RandCap(CaveDistance / Math.Sqrt(Scale), cap: FeatureMin);
            //ShapesDistance = Rand(CaveDistance * Math.PI);

            TreasureDiv = Rand(Scale * 13, div: 3, cap: .78);
            //IslandVisionMult = 6.5;
            ExploreForResearch = Game.Rand.Round(Rand(39, div: 3, cap: .65));
            //TreasureSpacingChance = .5;

            ElevationDensity = Rand(Scale * 26, div: 2);
            ElevationMaxEffectDist = Rand(Math.Sqrt(Scale) * 39);
            ElevationHeight = Rand(Island.HEIGHT, div: 2, cap: .52);

            DifficultySetting = Rand(Math.E / Math.PI, div: 5, cap: .91);
            ExploreEnergy = Rand(2600, div: 4, cap: .78);
            EnemyStartEnergy = Rand(5200, div: 2, cap: .52);
            EnemyIncomeMatchFactor = Rand(6500, div: 4, cap: .78);
            EnemyEnergy = Rand(169, div: 3, cap: .65);
            EnemyEnergyRampTurns = Rand(91, div: 2, cap: .52);
            EnemyUnlockTurns = Rand(210, div: 2, cap: .65);
            EnemyTreasureMatch = Rand(.65 * Math.Sqrt(DifficultySetting), div: 2, cap: .52);
            DifficultyIncTurns = Rand(91 / Math.Sqrt(DifficultySetting), div: 4, cap: .91);

            DifficultyEnergyPow = Rand(1 / .91, div: 4, cap: .78);
            DifficultyResearchPow = Rand(3.9 / Math.E, div: 5, cap: .91);
            DifficultyAIPow = Rand(.91);
            AgressionTurns = Rand(26, div: 2, cap: .65);

            SpawnEnergyDiv = Rand(169 * 169, div: 3, cap: .65);
            SpawnTurnAdd = Rand(13);
            SpawnTurnDiv = Rand(21, div: 2, cap: .52);
            SpawnNeg = Rand(.26);

            PortalSpawnTime = Rand(39, div: 3);
            PortalSpawnStrMult = Rand(1.3, div: 4, cap: .65);
            PortalCost = Rand(9.1, div: 3, cap: .65);
            PortalExitDef = Rand(390);
            PortalEntranceDef = Rand(169);
            PortalDecayRate = Rand(16.9, div: 3, cap: .52);
            PortalRewardPct = Rand(.39, div: 3, cap: .65);
            PortalLoan = Rand(3.9);
            PortalMinDist = Rand(Math.Sqrt(Scale) * 78, div: 2, cap: .78);
            PortalIncomeDiv = Rand(3.9, div: 2, cap: .13);

            MoveDev = Rand(.013);
            MoveLimitPow = RandCap(1.3, div: 4, cap: 1);
            MoveLimitCost = RandCap(4, cap: 1);
            MoveMaxCost = RandCap(10, cap: MoveLimitCost);
            MoveIncCost = RandCap(25, cap: MoveMaxCost);

            CoreEnergy = 550;
            CoreMass = 250;
            CoreResearch = 20;
            CoreEnergyLow = Rand(260, div: 3, cap: .65);
            CoreEnergyMid = Rand(91, div: 3, cap: .65);
            CoreEnergyHigh = CoreEnergy - CoreEnergyMid - CoreEnergyLow; //199
            CoreExtractTurns = Rand(91, div: 4, cap: .91); //0.98901098901098901098901098901099

            IncomeDevAdd = Rand(.65, div: 2, cap: .39);
            IncomeDevDiv = Rand(65, div: 3, cap: .52);

            DroneCost = Rand(16.9, div: 4, cap: .78);
            DroneMassCostMult = RandCap(1.69, div: 3, cap: 1);
            DroneRefund = Rand(.65, div: 2, cap: .52);

            ResourceAvgDist = Rand(Math.Sqrt(Scale) * 21, div: 4, cap: .91);
            ResourceDistAdd = Rand(21);
            ResourceDistDiv = Rand(65);
            ResourceDistPow = Rand(.52);
            ResourceSustainValuePow = Rand(.26, div: 2);
            ResourceDev = Rand(.21, cap: .78);
            ResourceOE = Rand(.26, cap: .65);
            FoundationAmt = RandCap(1.69, div: 3, cap: 1.3); // (Math.E + Math.PI) / 2.0; //~2.930
            ExtractTurns = Rand(65, div: 5, cap: .91); //0.98461538461538461538461538461538
            ExtractSustainPow = Rand(.26, div: 3, cap: .65);
            ExtractorSustainCostPow = Rand(.65, div: 2, cap: .52);
            ExtractorResilienceCostPow = Rand(.065, div: 3, cap: .65);
            ExtractorHitsPow = Rand(.39, div: 2, cap: .78);
            //ExtractPow=x/(1-x) where x is desired exponent when sustain=1 
            //inverse: x=ExtractPow/(ExtractPow+1)
            ExtractPow = RandCap(3.5, div: 4, cap: 2); //x=0.77777777777777777777777777777778

            BiomassEnergyInc = Rand(117, div: 2);
            BiomassSustain = Rand(.78, div: 3);
            BiomassResearchDiv = Rand(8, div: 3, cap: .65);
            BiomassResearchPow = Rand(.39, div: 4, cap: .78);
            MetalMassInc = Rand(52, div: 2);
            MetalSustain = Rand(1.17, div: 3);
            MetalEnergyUpkDiv = 4;
            ArtifactResearchInc = Rand(9.1, div: 2);
            ArtifactSustain = Rand(1.69, div: 3);
            ArtifactMassIncDiv = Rand(3, div: 5, cap: .91);
            ArtifactEnergyUpkMult = 2.5;

            ExtractorCostPow = Rand(.91, div: 4, cap: .78);
            ExtractorResilience = Rand(.3, div: 2, cap: .39);
            BiomassExtractorEnergyCost = Rand(750, div: 3, cap: .65);
            BiomassExtractorMassCost = Rand(910, div: 4, cap: .65);
            MetalExtractorEnergyCost = Rand(780, div: 4, cap: .65);
            MetalExtractorMassCost = Rand(400, div: 3, cap: .65);
            ArtifactExtractorEnergyCost = Rand(1300, div: 4, cap: .65);
            ArtifactExtractorMassCost = Rand(300, div: 3, cap: .65);

            GeneratorEnergyInc = Rand(30, div: 5, cap: .91);
            GeneratorResearchUpk = .5;
            GeneratorEnergyCost = 280;
            GeneratorMassCost = 450;

            MissileHitRefundPct = Rand(.78, div: 3, cap: .52);
            MissileCostMult = Rand((1 - MissileHitRefundPct) / 2.6, div: 4, cap: .65);
            MissileEnergyCostRatio = Rand(2.0 / (3 * EnergyMassRatio + 2), div: 2);
            MissileScrapRefund = Rand((1 - MissileHitRefundPct) * .91, div: 3, cap: .91);
            MissileAttImmobileMult = Rand((1 / Math.Sqrt(5)), div: 4, cap: .78);
            MissileAttMaxMult = RandCap(Math.Sqrt(2), div: 4, cap: 1);

            GeneratorConstValue = Rand(Math.Sqrt((MassPerResearchConversion * EnergyMassRatio) * (MassForScrapResearch / (double)BurnMassPerEnergy))
                * GeneratorResearchUpk, div: 4, cap: .91);
        }

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
        //internal static double SumStats(IEnumerable<int> stats)
        //{
        //    return SumStats(stats.Select(s => (double)s));
        //}
        //internal static double SumStats(IEnumerable<double> stats)
        //{
        //    return StatValueInverse(stats.Sum(StatValue));
        //}
        internal double MoveValue(IMovable.Values? movable) =>
            Math.Pow(MoveValue(movable?.MoveInc ?? 0, movable?.MoveMax ?? 0, movable?.MoveLimit ?? 0), CostMovePow);
        internal double MoveValue(double moveInc, double moveMax, double moveLimit)
        {
            double move = MoveIncCost * moveInc / 1.0 + MoveMaxCost * moveMax / 2.1 + MoveLimitCost * moveLimit / 5.2;
            move /= MoveIncCost + MoveMaxCost + MoveLimitCost;
            return move;
        }

        public static float LimitedMove(IMovable movable, out bool limitMove)
        {
            if (movable == null)
            {
                limitMove = false;
                return 0;
            }
            limitMove = movable.MoveCur + movable.MoveInc > movable.MoveMax;
            return (float)(limitMove ? movable.MoveCur + movable.MoveInc - movable.MoveMax : movable.MoveCur);
        }

        internal int Income(int cur, double income) => IncomeRounding(cur + Income(income));
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
        internal double Income(double income) => income + Game.Rand.Gaussian(IncomeDev(income));
        internal double IncomeDev(double income) => IncomeDevAdd + Math.Abs(income) / IncomeDevDiv;

        internal static double GetPct(double pct, double mult)
        {
            return 1 - Math.Pow(1 - pct, mult);
        }

        internal static bool CanRepair(Piece piece)
        {
            bool canRepair = (piece.GetBehavior<IKillable>()?.Defended != true)
                && (piece.GetBehavior<IMovable>()?.Moved != true)
                && (piece.GetBehavior<IAttacker>()?.Attacked != true)
                && (piece.GetBehavior<IMissileSilo>()?.Attacked != true)
                && (piece.GetBehavior<IBuilder>()?.Built != true);
            if (canRepair && piece.Side.Mass < 0)
                canRepair = false;
            return canRepair;
        }
        internal static double GetRepairCost(Piece piece, double energy, double mass) =>
            (mass + energy / piece.Game.Consts.EnergyRepairDiv) * GetRepairMult(piece);
        internal static double GetRepairMult(Piece piece) =>
            piece.HasBehavior<IAttacker>() ? piece.Game.Consts.RepairCost : piece.Game.Consts.PassiveRepairCost;
        public static double GetDamagedValue(Piece piece, double value, double min) =>
            GetDamagedValue(piece, value, min, false);
        internal static double GetDamagedValue(Piece piece, double value, double min, bool sqrt)
        {
            IKillable killable = piece.GetBehavior<IKillable>();
            double resilience = killable.Resilience;
            if (sqrt)
                resilience = Math.Sqrt(resilience);
            return min + (value - min)
                * Math.Pow(StatValue(killable.Hits.DefenseCur) / StatValue(killable.Hits.DefenseMax), (1 - resilience) / 2.0);
        }
        internal static int ModAtt(int att, int mod) => att + (att > 0 ? mod : 0);

        internal double IncDefense(bool doEndTurn, DefenseType type, bool isAttacker, int cur, int max, double repairAmt, ref double energyUpk, ref double massUpk)
        {
            double newValue = cur;
            double regen = CombatTypes.GetRegen(type, repairAmt);
            if (regen > 0)
            {
                double costMult = CombatTypes.GetRegenCostMult(this, type, isAttacker, out bool mass);
                double upkeep = 0;
                newValue = IncStatValue(doEndTurn, cur, max, regen, costMult, ref upkeep);
                if (mass)
                    massUpk += upkeep;
                else
                    energyUpk += upkeep;
            }
            return newValue;
        }
        internal static double IncStatValue(bool doEndTurn, int cur, int max, double regen, double upkeepRate, ref double upkeep)
        {
            double newValue = cur;
            if (cur < max)
            {
                newValue = Math.Min(max, cur + regen);
                if (doEndTurn)
                    newValue = Game.Rand.Round(newValue);
                upkeep += StatValueCost(cur, newValue, upkeepRate);
            }
            return newValue;
        }
        internal static double StatValueCost(double before, double after, double upkeepRate) =>
            (StatValue(after) - StatValue(before)) * upkeepRate;

        internal static double IncValueWithMaxLimit(double cur, double inc, double dev, double max, double limit, double pow, bool rand)
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

        internal double MapDistMult(Tile tile, double mapSize) =>
            MapDistMult(Tile.GetDistanceD(tile.LocationD, new(0, 0)), mapSize);
        internal double MapDistMult(double dist, double mapSize)
        {
            dist -= mapSize + CaveSize;
            return dist > 0 ? 1 / Math.Pow(13, dist / CaveSize) : 1;
        }
    }
}
