using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using Type = ClassLibrary1.Research.Type;

namespace ClassLibrary1
{
    internal static class ResearchUpgValues
    {
        internal const double Blueprint_Attack_Pow = 0.60;
        internal const double Blueprint_Attacks_Count_Pow = 0.20;
        internal const double Blueprint_Defense_Pow = 0.65;
        internal const double Blueprint_Move_Pow = 0.40;
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

        private static readonly IReadOnlyDictionary<UpgType, UpgParam> UpgParams = new Dictionary<UpgType, UpgParam>() {
            { UpgType.AmbientGenerator, new(Consts.GeneratorEnergyInc, 0.65) },
            { UpgType.AmbientGeneratorCost, new(0.20, true) },
            { UpgType.ConstructorCost, new(0.70, true) },
            { UpgType.ConstructorDefense, new(8, 0.45, add: 2) },
            { UpgType.ConstructorMove, new(Constructor.BASE_MOVE_INC * Constructor.MOVE_RAMP, 0.30, Constructor.MOVE_RAMP) },
            { UpgType.ConstructorRange, new(5.2, 0.35,  add: .3) },
            { UpgType.ConstructorVision, new(Constructor.BASE_VISION, 0.25) },
            //{ UpgType.ConstructorRepair, new(1, 0.45) },
            //{ UpgType.CoreDefense, new(11, 0.65, 11 / 10.0) },
            { UpgType.CoreShields, new(7.8, 0.85, 1.5) },
            { UpgType.DroneCost, new(0.10, true) },
            { UpgType.DroneDefense, new(21, 0.30, 1.1) },
            { UpgType.DroneMove, new(2.6, 0.35) },
            { UpgType.DroneRepair, new(1.3, 0.75, add: -0.3) },
            { UpgType.DroneTurns, new(7.8, 0.45) },
            /*UpgType.ExtractorResilience*/
            { UpgType.ExtractorCost, new(0.15, true) },
            { UpgType.ExtractorDefense, new(16.9, 0.50, 3.90, 1.3) },
            { UpgType.ExtractorSustain, new(1, 0.10) },
            { UpgType.ExtractorValue, new(1, 0.25) },
            { UpgType.ExtractorVision, new(5, 0.80) },
            { UpgType.FactoryCost, new(0.60, true) },
            { UpgType.FactoryDefense, new(9, 0.40, 9 / 5.0) },
            { UpgType.FactoryRepair, new(1, 0.50, Math.E, .65) },
            { UpgType.FactoryVision, new(6.5, 0.90, 6.5 / 4.5 ) },
            { UpgType.MissileAttack, new(16.9, 0.80) },
            { UpgType.MissileCost, new(0.50, true) },
            { UpgType.MissileRange, new(MissileSilo.START_RANGE, 0.55) },
            { UpgType.TurretArmorDefense, new(11, 0.55, 5.2, 1.69) },
            { UpgType.TurretAttack, new(8, 0.70, 1.69, .39) },
            { UpgType.TurretCost, new(0.30, true) },
            { UpgType.TurretDefense, new(5, 0.25, add: 10) },
            { UpgType.TurretExplosivesAttack, new(6, 0.65) },
            { UpgType.TurretExplosivesRange, new(9, 0.40, 1.35, Attack.MIN_RANGED - 1) },
            { UpgType.TurretLaserAttack, new(4, 0.75) },
            { UpgType.TurretLaserRange, new(15, 0.60, 1.45, Attack.MIN_RANGED + 1) },
            { UpgType.TurretRange, new(13, 0.50, 1.4, Attack.MIN_RANGED) },
            { UpgType.TurretShieldDefense, new(7, 0.35, 1.85) },
            { UpgType.TurretVision, new(10, 0.45, 1.7) },
        }.AsReadOnly();

        internal static double Calc(UpgType upgType, double researchMult) =>
            UpgParams[upgType].CalcAvg(null, researchMult);

        internal static string GetUpgInfo(Game game, Type type, double prevMult, double nextMult)
        {
            return UpgTypes[type].Where(u => !u.ToString().Contains("Vision")).Select(upgType =>
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

        private class UpgParam
        {
            //private Func<Game, double> GetRounding;
            private readonly double avg, add, ramp, pow;
            private readonly bool cost;
            public bool Pct => cost || avg == 1;
            public UpgParam(double pow, bool cost)
            {
                this.avg = 0;
                this.add = 0;
                this.pow = pow;
                this.ramp = 0;
                this.cost = cost;
            }
            public UpgParam(double avg, double pow, double ramp = 1, double add = 0, bool cost = false)
            //, Func<Game, double> GetRounding = null)
            {
                //this.GetRounding = GetRounding;
                this.avg = avg;
                this.add = add;
                this.pow = pow;
                this.ramp = ramp;
                this.cost = cost;
            }
            public double CalcAvg(Game game, double mult)
            {
                double avg = cost ? CalcCost(mult) : Calc(mult);
                //if (GetRounding != null)
                //    avg = MTRandom.Round(avg, GetRounding(game));
                return avg;
            }
            private double Calc(double mult) => add + avg * (mult < ramp ? mult / ramp : 1) * Math.Pow(mult, pow);
            private double CalcCost(double mult) => 1 / Math.Pow(mult, pow);
        }

        private static readonly UpgType[] BaseZero = new[] { UpgType.CoreShields, UpgType.FactoryRepair, // UpgType.RepairDrone,
            UpgType.TurretLaserAttack, UpgType.TurretExplosivesAttack, UpgType.TurretShieldDefense,
            UpgType.TurretArmorDefense, UpgType.TurretLaserRange, UpgType.TurretExplosivesRange, };

        private static readonly IReadOnlyDictionary<Type, UpgType[]> UpgTypes = new Dictionary<Type, UpgType[]>() {
            { Type.AmbientGenerator, new[] { UpgType.AmbientGenerator, } },
            { Type.BuildingCost, new[] { UpgType.ExtractorCost, UpgType.FactoryCost, UpgType.TurretCost, UpgType.AmbientGeneratorCost, } },
            { Type.BuildingDefense, new[] { UpgType.ExtractorDefense, UpgType.ExtractorVision, UpgType.FactoryDefense, UpgType.FactoryVision, } },
            { Type.ConstructorCost, new[] { UpgType.ConstructorCost, UpgType.DroneCost, } },
            { Type.ConstructorDefense, new[] { UpgType.ConstructorDefense, UpgType.DroneDefense, } },
            { Type.ConstructorMove, new[] { UpgType.ConstructorMove, UpgType.ConstructorVision, UpgType.ConstructorRange, UpgType.DroneMove, } },
            { Type.CoreDefense, new[] { UpgType.CoreShields, } },
            { Type.ExtractorValue, new[] { UpgType.ExtractorValue, UpgType.ExtractorSustain, } },
            { Type.FactoryRepair, new[] { UpgType.FactoryRepair } },
            { Type.Missile, new[] { UpgType.MissileAttack, } },
            { Type.MissileCost, new[] { UpgType.MissileCost, } },
            { Type.MissileRange, new[] { UpgType.MissileRange, } },
            { Type.RepairDrone, new[] { UpgType.DroneRepair, UpgType.DroneTurns, } },
            { Type.TurretAttack, new[] { UpgType.TurretAttack, UpgType.TurretLaserAttack, UpgType.TurretExplosivesAttack, } },
            { Type.TurretDefense, new[] { UpgType.TurretDefense, UpgType.TurretShieldDefense, UpgType.TurretArmorDefense, UpgType.TurretVision, } },
            { Type.TurretRange, new[] { UpgType.TurretRange, UpgType.TurretLaserRange, UpgType.TurretExplosivesRange, } },
        }.AsReadOnly();

        internal enum UpgType
        {
            AmbientGenerator,
            AmbientGeneratorCost,
            //AmbientGeneratorDefense,
            //AmbientGeneratorVision,
            ConstructorCost,
            ConstructorDefense,
            ConstructorMove,
            ConstructorRange,
            ConstructorVision,
            //CoreDefense,
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
            //ResearchChoices,
            TurretArmorDefense,
            TurretAttack,
            TurretCost,
            TurretDefense,
            TurretExplosivesAttack,
            TurretExplosivesRange,
            TurretLaserAttack,
            TurretLaserRange,
            TurretRange,
            TurretShieldDefense,
            TurretVision,
        }
    }
}
