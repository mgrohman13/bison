using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Players;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Type = ClassLibrary1.Research.Type;

namespace ClassLibrary1
{
    [Serializable]
    [DataContract(IsReference = true)]
    internal class ResearchUpgValues
    {
        internal const double Blueprint_Attack_Pow = 0.60;
        internal const double Blueprint_Attacks_Count_Pow = 0.20;
        internal const double Blueprint_Defense_Pow = 0.65;
        internal const double Blueprint_Move_Pow = 0.35;
        internal const double Blueprint_Range_Pow = 0.45;
        internal const double Blueprint_Vision_Pow = 0.70;

        //internal const double Constructor_Cost_Pow = ;
        //internal const double Constructor_Defense_Pow = ;
        //internal const double Constructor_Move_Pow = ;
        //internal const double Constructor_Repair_Pow = ;
        //internal const double Constructor_Vision_Pow = ;

        //internal const double Core_Defense_Pow = ;
        //internal const double Core_Shields_Pow = ;

        //internal const double Extractor_Cost_Pow = ;
        //internal const double Extractor_Defense_Pow = ;
        //internal const double Extractor_Resilience = ;
        //internal const double Extractor_Sustain_Pow = ;
        //internal const double Extractor_Value_Pow = ;
        //internal const double Extractor_Vision_Pow = ;

        //internal const double Factory_Cost_Pow = ;
        //internal const double Factory_Defense_Pow = ;
        //internal const double Factory_Repair_Pow = ;
        //internal const double Factory_Vision_Pow = ;

        //internal const double Turret_Attack_Pow = ;
        //internal const double Turret_Cost_Pow = ;
        //internal const double Turret_Defense_Pow = ;
        //internal const double Turret_Range_Pow = ;
        //internal const double Turret_Vision_Pow = ;

        private const double ConstructorDef = 6.50, ConstructorDefAdd = 4.00;
        private const double ConstructorRange = 2.75, ConstructorRangeAdd = 3.25;
        public static readonly int ConstructorStartDef = (int)Math.Floor(ConstructorDef + ConstructorDefAdd);
        public const double ConstructorStartRange = ConstructorRange + ConstructorRangeAdd;
        internal const double TurretResilience = 0.26;

        private readonly ReadOnlyDictionary<UpgType, UpgParam> UpgParams;

        public ResearchUpgValues(Consts consts)
        {
            UpgParams = new Dictionary<UpgType, UpgParam>() {
                { UpgType.AmbientGenerator, new(Type.AmbientGenerator, consts.GeneratorEnergyInc, 0.35) },
                { UpgType.AmbientGeneratorCost, new(Type.AmbientGenerator,0.25, true) },
                //{ UpgType.CombineMechs, new(Type.CombineMechs, 1.20, 0.60) },
                { UpgType.ConstructorCost, new(Type.Constructor, 0.55, true) },
                { UpgType.ConstructorDefense, new(Type.Constructor, ConstructorDef, 0.50, add: ConstructorDefAdd) },
                { UpgType.ConstructorMove, new(Type.Constructor, Constructor.START_MOVE_INC * Constructor.MOVE_RAMP, 0.25, Constructor.MOVE_RAMP) },
                { UpgType.ConstructorRange, new(Type.Constructor, ConstructorRange, 0.20,  add: ConstructorRangeAdd) },
                { UpgType.ConstructorVision, new(Type.Constructor, Constructor.START_VISION - Constructor.VISION_ADD, 0.30, add: Constructor.VISION_ADD) },
                //{ UpgType.ConstructorRepair, new(1, 0.45) },
                //{ UpgType.CoreDefense, new(11, 0.65, 11 / 10.0) },
                { UpgType.CoreArmor, new(Type.CoreArmor, 4.50, 0.70, 2.50) },
                { UpgType.CoreShields, new(Type.CoreDefense, 5.50, 0.45) },
                { UpgType.DroneCost, new(Type.RepairDrone, 0.20, true) },
                { UpgType.DroneDefense, new(Type.RepairDrone, 21.00, 0.40, 1.10) },
                { UpgType.DroneMove, new(Type.RepairDrone, 2.6, 0.40) },
                { UpgType.DroneRepair, new(Type.RepairDrone, 1.3, 0.75, add: -0.3) },
                { UpgType.DroneTurns, new(Type.RepairDrone, 7.8, 0.45) },
                /*UpgType.ExtractorResilience*/
                { UpgType.ExtractorCost, new(Type.Mech, 0.10, true) },
                { UpgType.ExtractorDefense, new(Type.Mech, 16.90, 0.20, 3.90, 1.30) },
                { UpgType.ExtractorSustain, new(Type.Mech, 1, 0.10, min: 1) },
                { UpgType.ExtractorValue, new(Type.Mech, 1, 0.25, min: 1) },
                { UpgType.ExtractorVision, new(Type.Mech, 5, 0.80) },
                { UpgType.FactoryCost, new(Type.Factory, 0.60, true) },
                { UpgType.FactoryDefense, new(Type.Factory, 10.00, 0.60, 10.00 / 5.0) },
                { UpgType.FactoryRepair, new(Type.Factory, 1, 0.50, Math.E, .65) },
                { UpgType.FactoryVision, new(Type.Factory, 6.75, 0.90, 1.70 ) },
                { UpgType.MissileAttack, new(Type.Missile, 9.50, 0.80, add: 5.2) },
                { UpgType.MissileCost, new(Type.Missile, 0.50, true) },
                { UpgType.MissileRange, new(Type.Missile, MissileSilo.START_RANGE, 0.55) },
                { UpgType.OutpostAttack, new(Type.Outpost, 3.00, 0.55, 1.95, 2.00) },
                { UpgType.OutpostCost, new(Type.Outpost, 0.35, true) },
                { UpgType.OutpostDefense, new(Type.Outpost, 9.10, 0.30, add: 2.40) },
                { UpgType.OutpostRepair, new(Type.Outpost, 3.50, 0.25, 1.80) },
                { UpgType.OutpostVision, new(Type.Outpost, 9.10, 0.40 ) },          
                { UpgType.TurretAttack, new(Type.Turret, 8.00, 0.70, 1.69, 2.10) },
                { UpgType.TurretCost, new(Type.Turret, 0.30, true) },
                { UpgType.TurretProtection, new(Type.Turret, 11.00, 0.55, 5.20, 1.69) },
                { UpgType.TurretDefense, new(Type.Turret, 5.00, 0.25, add: 9.10) },
                { UpgType.TurretRange, new(Type.Turret, 13.00, 0.50, 1.75, Attack.MIN_RANGED) },
                { UpgType.TurretResilience, new(Type.Turret, TurretResilience, true) },
                { UpgType.TurretVision, new(Type.Turret, 10.00, 0.45, 1.90) },

                //{ UpgType.TurretExplosivesAttack, new(Type.TurretExplosives, 6, 0.65) },
                //{ UpgType.TurretExplosivesRange, new(Type.TurretExplosives, 9, 0.40, 1.55, Attack.MIN_RANGED - 1) },
                //{ UpgType.TurretLaserAttack, new(Type.TurretLasers, 4, 0.75) },
                //{ UpgType.TurretLaserRange, new(Type.TurretLasers, 15, 0.60, 1.65, Attack.MIN_RANGED + 1) },
                //{ UpgType.TurretShieldDefense, new(Type.TurretShields, 7, 0.35, 1.85) },
            }.AsReadOnly();
        }

        internal double Calc(UpgType upgType, double researchMult) =>
            UpgParams[upgType].CalcAvg(null, researchMult);

        internal string GetUpgInfo(Game game, Type type, double prevMult, double nextMult)
        {
            return UpgTypes[type].Where(upgType => !upgType.ToString().Contains("Vision"))
                .Where(upgType => game.Player.Research.HasType(UpgParams[upgType].Preq))
                .Select(upgType =>
                {
                    var param = UpgParams[upgType];
                    double prev = CheckZero(upgType, prevMult, param.CalcAvg(game, prevMult));
                    return GetUpgInfo(upgType, prev, param.CalcAvg(game, nextMult), param.Pct);
                }).Aggregate("", (a, b) => a + (a.Length > 0 ? Environment.NewLine : string.Empty) + b);
        }
        private static double CheckZero(UpgType upgType, double prevMult, double prev)
        {
            //if (prevMult == 1 && BaseZero.Contains(upgType))
            //    prev = 0;
            return prev;
        }

        private static string GetUpgInfo<T>(T type, double prev, double next, bool pct) where T : Enum =>
            GetUpgInfo(type, prev, next, v => v.ToString(pct ? "P0" : "0.0"));// $"+{(v - 1) * 100:0)}%");
        internal static string GetUpgInfo<T>(T type, double prev, double next, Func<double, string> Format) where T : Enum =>
            $"{type}: {Format(prev)} -> {Format(next)}";

        [Serializable]
        [DataContract(IsReference = true)]
        private class UpgParam
        {
            //private Func<Game, double> GetRounding;
            public readonly Type Preq;

            private readonly double avg, pow, rmp, add, min;
            private readonly bool cost, pct;

            public bool Pct => pct;

            public UpgParam(Type preq, double pow, bool cost)
                : this(preq, 0, pow, 0, 0, cost)
            { }
            public UpgParam(Type preq, double avg, double pow, double rmp = 1, double add = 0, bool cost = false, double min = 0)
            //, Func<Game, double> GetRounding = null)
            {
                //this.GetRounding = GetRounding;
                this.Preq = preq;
                this.min = min;
                this.cost = cost;
                this.pct = cost || avg == 1;

                bool neg = add < 0;
                if (neg)
                    add *= -1;
                double Dev() => Game.Rand.GaussianOE(.13, .13, .13);
                double Min(double value, double mult) => Game.Rand.GaussianCapped(value * mult, Dev(), value * Math.Max(mult * 2 - 1, 0));

                this.avg = Game.Rand.GaussianCapped(avg, Dev() / 2.5, Min(avg, .78));
                this.pow = Game.Rand.GaussianCapped(pow, Dev() / 3.0, Min(pow, .91));
                this.rmp = Game.Rand.GaussianCapped(rmp, Dev() / 2.0, Min(rmp, .65));
                this.add = Game.Rand.GaussianCapped(add, Dev() / 1.5, Min(add, .52));

                if (neg)
                    this.add *= -1;
                //if ( preq == Type.ExtractorValue || preq == Type.sus)
            }

            public double CalcAvg(Game game, double mult)
            {
                double avg = cost ? CalcCost(mult) : Calc(mult);
                //if (GetRounding != null)
                //    avg = MTRandom.Round(avg, GetRounding(game));
                return avg;
            }
            private double Calc(double mult)
            {
                double result = add + avg * (mult < rmp ? mult / rmp : 1) * Math.Pow(mult, pow);
                result = Math.Max(result, min);
                return result;
            }
            private double CalcCost(double mult) =>
                1 / Math.Pow(mult, pow);
        }

        //private static readonly UpgType[] BaseZero = [ UpgType.CoreShields, UpgType.FactoryRepair, // UpgType.RepairDrone,
        //    UpgType.TurretLaserAttack, UpgType.TurretExplosivesAttack, UpgType.TurretShieldDefense,
        //    UpgType.TurretArmorDefense, UpgType.TurretLaserRange, UpgType.TurretExplosivesRange, ];

        private static readonly ReadOnlyDictionary<Type, UpgType[]> UpgTypes = new Dictionary<Type, UpgType[]>() {
            { Type.AmbientGenerator, new[] { UpgType.AmbientGenerator, UpgType.AmbientGeneratorCost, } },
            { Type.BuildingCost, new[] {  UpgType.OutpostCost, UpgType.TurretCost, UpgType.FactoryCost,  } },
            { Type.BuildingDefense, new[] { UpgType.ExtractorDefense, UpgType.ExtractorVision, UpgType.OutpostDefense, UpgType.FactoryDefense, UpgType.FactoryVision, } },
            //{ Type.CombineMechs, new[] { UpgType.CombineMechs, } },
            { Type.ConstructorCost, new[] { UpgType.ConstructorCost, UpgType.DroneCost, } },
            { Type.ConstructorDefense, new[] { UpgType.ConstructorDefense, UpgType.DroneDefense, } },
            { Type.ConstructorMove, new[] { UpgType.ConstructorMove, UpgType.ConstructorVision, UpgType.ConstructorRange, UpgType.DroneMove, } },
            { Type.CoreDefense, new[] { UpgType.CoreShields, UpgType.CoreArmor, } },
            { Type.ExtractorValue, new[] { UpgType.ExtractorValue, UpgType.ExtractorSustain, UpgType.ExtractorCost, } },
            { Type.FactoryRepair, new[] { UpgType.FactoryRepair, UpgType.OutpostRepair, } },
            { Type.Missile, new[] { UpgType.MissileAttack, } },
            { Type.MissileCost, new[] { UpgType.MissileCost, } },
            { Type.MissileRange, new[] { UpgType.MissileRange, } },
            { Type.RepairDrone, new[] { UpgType.DroneRepair, UpgType.DroneTurns, } },
            { Type.TurretAttack, new[] { UpgType.TurretAttack, UpgType.OutpostAttack, } },
            { Type.TurretDefense, new[] { UpgType.TurretDefense, UpgType.TurretProtection, UpgType.TurretResilience, } },
            { Type.TurretRange, new[] { UpgType.TurretRange, UpgType.TurretVision, UpgType.OutpostVision, } }, 
        }.AsReadOnly();

        internal enum UpgType
        {
            AmbientGenerator,
            AmbientGeneratorCost,
            //AmbientGeneratorDefense,
            //AmbientGeneratorVision,
            //CombineMechs,
            ConstructorCost,
            ConstructorDefense,
            ConstructorMove,
            ConstructorRange,
            ConstructorVision,
            //CoreDefense,
            CoreArmor,
            CoreShields,
            DroneCost,
            DroneDefense,
            DroneMove,
            DroneRepair,
            DroneTurns,
            ExtractorCost,
            ExtractorDefense,
            //ExtractorResilience,
            ExtractorSustain,
            ExtractorValue,
            ExtractorVision,
            FactoryCost,
            FactoryDefense,
            FactoryRepair,
            FactoryVision,
            MissileAttack,
            MissileCost,
            MissileRange,
            OutpostAttack,
            OutpostCost,
            OutpostDefense,
            OutpostRepair,
            OutpostVision,
            //ResearchChoices,
            TurretAttack,
            TurretCost,
            TurretProtection,
            TurretDefense,
            TurretRange,
            TurretResilience,
            TurretVision,
            //TurretExplosivesAttack,
            //TurretExplosivesRange,
            //TurretLaserAttack,
            //TurretLaserRange,
            //TurretShieldDefense,
        }
    }
}
