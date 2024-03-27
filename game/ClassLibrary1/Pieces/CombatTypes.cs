using System;
using System.Linq;

namespace ClassLibrary1.Pieces
{
    public static class CombatTypes
    {
        public enum AttackType
        {
            Kinetic,
            Energy,
            Explosive,
        }
        public enum DefenseType
        {
            Hits, //reduce attack values?
            Shield,
            Armor, //needs special..
        }

        internal static int GetStartCur(AttackType type, int attack) =>
            type == AttackType.Energy ? 0 : attack;
        internal static int GetStartCur(DefenseType type, int defense) =>
            type == DefenseType.Shield ? 0 : defense;

        internal static object OrderBy(Defense defense)
        {
            int value = defense.DefenseCur;
            int higher = defense.Piece.GetBehavior<IKillable>().TotalDefenses.Max(d => d.DefenseCur) + 1;
            if (defense.Type == DefenseType.Hits)
                value -= higher;
            else if (defense.Type == DefenseType.Shield && defense.DefenseCur == defense.DefenseMax)
                value += higher;
            return -value;
        }
        //internal static object OrderBy(Defense defense) => defense.Type switch
        //{
        //    DefenseType.Hits => 3,
        //    DefenseType.Shield => 1,
        //    DefenseType.Armor => 2,
        //    _ => throw new Exception(),
        //};
        //internal static DefenseType? SkipsDefense(AttackType attackType) => attackType switch
        //{
        //    AttackType.Kinetic => null,
        //    AttackType.Energy => null,// DefenseType.Shield,
        //    AttackType.Explosive => null,// DefenseType.Armor, //???
        //    _ => throw new Exception(),
        //};
        internal static bool SplashDamage(AttackType attackType, DefenseType defenseType) =>
            attackType == AttackType.Explosive && defenseType != DefenseType.Shield;

        internal static int GetRegen(AttackType attackType, bool attacked, bool inBuild) => attackType switch //bool moved,
        {
            //reduce with dmg pct?
            AttackType.Kinetic => attacked ? 0 : (inBuild ? 3 : 2),
            AttackType.Energy => attacked ? 2 : 3,
            AttackType.Explosive => attacked ? 0 : (inBuild ? 3 : 1),
            _ => throw new Exception(),
        };
        internal static int GetRegen(DefenseType defenseType, bool inRepair) => defenseType switch //, bool moved, bool attacked)
        {
            //reduce with dmg pct?
            DefenseType.Hits => 0, //need to fix Hits repair system
            DefenseType.Shield => 1,
            DefenseType.Armor => inRepair ? 2 : 0,
            _ => throw new Exception(),
        };
        internal static double GetRegenCostMult(DefenseType defenseType, out bool mass)
        {
            switch (defenseType)
            {
                case DefenseType.Shield:
                    mass = false;
                    return Consts.EnergyPerShield;
                case DefenseType.Armor:
                    mass = true;
                    return Consts.MassPerArmor;
                default: throw new Exception();
            };
        }

        //cheaper armor repair cost?
        internal static bool Repair(DefenseType defenseType) =>
            defenseType == DefenseType.Hits;

        internal static double Cost(AttackType attackType) => attackType switch
        {
            AttackType.Kinetic => .91,
            AttackType.Energy => 1,
            AttackType.Explosive => 1.3,
            _ => throw new Exception(),
        };
        internal static double Cost(DefenseType defenseType) => defenseType switch
        {
            DefenseType.Hits => 1,
            DefenseType.Shield => .91,
            DefenseType.Armor => .65,
            _ => throw new Exception(),
        };
        internal static double EnergyCostRatio(AttackType attackType) => attackType switch
        {
            AttackType.Kinetic => .26,
            AttackType.Energy => .78,
            AttackType.Explosive => .52,
            _ => throw new Exception(),
        };
        internal static double EnergyCostRatio(DefenseType defenseType) => defenseType switch
        {
            DefenseType.Hits => .39,
            DefenseType.Shield => .91,
            DefenseType.Armor => .13,
            _ => throw new Exception(),
        };
    }
}
