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
        internal const double Blueprint_Attack_Pow = 0.80;
        internal const double Blueprint_Attacks_Count_Pow = 0.20;
        internal const double Blueprint_Defense_Pow = 0.70;
        internal const double Blueprint_Move_Pow = 0.40;
        internal const double Blueprint_Range_Pow = 0.60;
        internal const double Blueprint_Resilience_Pow = 1.00;
        internal const double Blueprint_Vision_Pow = 0.50;

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
            { UpgType.ConstructorCost, new(0.70, true) },
            { UpgType.ConstructorDefense, new(8, 0.50, 8 / 5.0) },
            { UpgType.ConstructorMove, new(Constructor.BASE_MOVE_INC * Constructor.MOVE_RAMP, 0.35, Constructor.MOVE_RAMP) },
            { UpgType.ConstructorVision, new(Constructor.BASE_VISION, 0.30) },
            { UpgType.ConstructorRepair, new(1, 0.45) },
            { UpgType.CoreDefense, new(11, 0.65, 11 / 10.0) },
            { UpgType.CoreShields, new(16.9, 0.40, 2.5) },
            /*UpgType.ExtractorResilience*/
            { UpgType.ExtractorCost, new(0.15, true) },
            { UpgType.ExtractorDefense, new(12, 0.60, 12 / 5.0) },
            { UpgType.ExtractorVision, new(5, 0.75) },
            { UpgType.ExtractorValue, new(1, 0.25) },
            { UpgType.ExtractorSustain, new(1, 0.10) },
            { UpgType.FactoryCost, new(0.60, true) },
            { UpgType.FactoryDefense, new(9, 0.55, 9 / 5.0) },
            { UpgType.FactoryVision, new(5, 0.85) },
            { UpgType.FactoryRepair, new(1, 0.50, Math.E, .65) },
            { UpgType.TurretCost, new(0.50, true) },
            { UpgType.TurretAttack, new(9, 0.65, 1.69) },
            { UpgType.TurretLaserAttack, new(4, 0.68, 1.75) },
            { UpgType.TurretExplosivesAttack, new(11, 0.62, 1.65) },
            { UpgType.TurretDefense, new(13, 0.55, 1.20) },
            { UpgType.TurretShieldDefense, new(7, 0.52, 1.15) },
            { UpgType.TurretArmorDefense, new(10, 0.58, 1.25) },
            { UpgType.TurretVision, new(12.5, 0.35) },
            { UpgType.TurretRange, new(12, 0.45, 1.40, Attack.MIN_RANGED + 1) },
            { UpgType.TurretLaserRange, new(16, 0.42, 1.45, Attack.MIN_RANGED) },
            { UpgType.TurretExplosivesRange, new(8, 0.48, 1.35, Attack.MIN_RANGED - 1) },
        }.AsReadOnly();

        internal static double Calc(UpgType upgType, double researchMult) => UpgParams[upgType].CalcAvg(researchMult);

        internal static string GetUpgInfo(Type type, double prevMult, double nextMult)
        {
            return UpgTypes[type].Where(u => !u.ToString().Contains("Vision")).Select(upgType =>
            {
                var param = UpgParams[upgType];
                double prev = CheckZero(upgType, prevMult, param.CalcAvg(prevMult));
                return GetUpgInfo(upgType, prev, param.CalcAvg(nextMult), param.Pct);
            }).Aggregate("", (a, b) => a + (a.Length > 0 ? Environment.NewLine : string.Empty) + b);
        }
        private static double CheckZero(UpgType upgType, double prevMult, double prev)
        {
            if (prevMult == 1 && BaseZero.Contains(upgType))
                prev = 0;
            return prev;
        }

        private static string GetUpgInfo<T>(T type, double prev, double next, bool pct) where T : Enum =>
            GetUpgInfo(type, prev, next, v => v.ToString(pct ? "P0" : "0.0"));// $"+{(v - 1) * 100:0)}%");
        internal static string GetUpgInfo<T>(T type, double prev, double next, Func<double, string> Format) where T : Enum =>
            $"{type}: {Format(prev)} -> {Format(next)}";

        private class UpgParam
        {
            private readonly double avg, start, ramp, pow;
            private readonly bool cost;
            public bool Pct => cost || avg == 1;
            public UpgParam(double pow, bool cost)
            {
                this.avg = 0;
                this.start = 0;
                this.pow = pow;
                this.ramp = 0;
                this.cost = cost;
            }
            public UpgParam(double avg, double pow, double ramp = 1, double start = 0, bool cost = false)
            {
                this.avg = avg;
                this.start = start;
                this.pow = pow;
                this.ramp = ramp;
                this.cost = cost;
            }
            public double CalcAvg(double mult) => cost ? CalcCost(mult) : Calc(mult);
            private double Calc(double mult) => start + avg * (mult < ramp ? mult / ramp : 1) * Math.Pow(mult, pow);
            private double CalcCost(double mult) => 1 / Math.Pow(mult, pow);
        }

        private static readonly UpgType[] BaseZero = new[] { UpgType.ConstructorRepair, UpgType.CoreShields, UpgType.FactoryRepair,
            UpgType.TurretLaserAttack, UpgType.TurretExplosivesAttack, UpgType.TurretShieldDefense,
            UpgType.TurretArmorDefense, UpgType.TurretLaserRange, UpgType.TurretExplosivesRange, };

        private static readonly IReadOnlyDictionary<Type, UpgType[]> UpgTypes = new Dictionary<Type, UpgType[]>() {
            { Type.BuildingCost, new[] { UpgType.ExtractorCost, UpgType.FactoryCost, UpgType.TurretCost, } },
            { Type.BuildingDefense, new[] { UpgType.CoreDefense, UpgType.ExtractorDefense, UpgType.ExtractorVision, UpgType.FactoryDefense, UpgType.FactoryVision, } },
            { Type.ConstructorCost, new[] { UpgType.ConstructorCost } },
            { Type.ConstructorDefense, new[] { UpgType.ConstructorDefense } },
            { Type.ConstructorMove, new[] { UpgType.ConstructorMove, UpgType.ConstructorVision, } },
            { Type.ConstructorRepair, new[] { UpgType.ConstructorRepair } },
            { Type.CoreShields, new[] { UpgType.CoreShields } },
            { Type.ExtractorValue, new[] { UpgType.ExtractorValue, UpgType.ExtractorSustain, } },
            { Type.FactoryRepair, new[] { UpgType.FactoryRepair } },
            { Type.TurretAttack, new[] { UpgType.TurretAttack, UpgType.TurretLaserAttack, UpgType.TurretExplosivesAttack, } },
            { Type.TurretDefense, new[] { UpgType.TurretDefense, UpgType.TurretShieldDefense, UpgType.TurretArmorDefense, UpgType.TurretVision, } },
            { Type.TurretRange, new[] { UpgType.TurretRange, UpgType.TurretLaserRange, UpgType.TurretExplosivesRange, } },
        }.AsReadOnly();

        internal enum UpgType
        {
            ConstructorCost,
            ConstructorDefense,
            ConstructorMove,
            ConstructorVision,
            ConstructorRepair,
            CoreDefense,
            CoreShields,
            //ExtractorResilience,
            ExtractorCost,
            ExtractorDefense,
            ExtractorVision,
            ExtractorValue,
            ExtractorSustain,
            FactoryCost,
            FactoryDefense,
            FactoryVision,
            FactoryRepair,
            //ResearchChoices,
            TurretCost,
            TurretAttack,
            TurretLaserAttack,
            TurretExplosivesAttack,
            TurretDefense,
            TurretShieldDefense,
            TurretArmorDefense,
            TurretVision,
            TurretRange,
            TurretLaserRange,
            TurretExplosivesRange,
        }
    }
}
