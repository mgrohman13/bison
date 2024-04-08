using System;
using System.Collections.Generic;
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

        internal static object CompareDef(Defense defense)
        {
            int value = defense.DefenseCur;
            int higher = defense.Piece.GetBehavior<IKillable>().TotalDefenses.Max(d => d.DefenseCur) + 1;
            if (defense.Type == DefenseType.Hits)
                value -= higher;
            else if (defense.Type == DefenseType.Shield && defense.DefenseCur == defense.DefenseMax)
                value += higher;
            return -value;
        }
        internal static IReadOnlyCollection<IAttacker.Values> OrderAtt(IEnumerable<IAttacker.Values> attacks) =>
            OrderAtt(attacks, a => a.Attack, a => a.Range, a => a.Type);
        internal static IReadOnlyCollection<Attack> OrderAtt(IEnumerable<Attack> attacks) =>
            OrderAtt(attacks, a => a.AttackMax, a => a.RangeBase, a => a.Type);
        internal static IReadOnlyCollection<T> OrderAtt<T>(IEnumerable<T> attacks, Func<T, int> AttMax, Func<T, double> Range, Func<T, AttackType> Type) =>
            Game.Rand.Iterate(attacks).OrderByDescending(AttMax).ThenByDescending(Range).ThenByDescending(Type).ToList().AsReadOnly();
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
        internal static bool DoSplash(AttackType type, DefenseType defenseType) => type == AttackType.Explosive;
        internal static bool SplashAgainst(Defense defense)
        {
            if (defense.Type == DefenseType.Shield)
                return true;
            if (defense.Piece.GetBehavior<IKillable>().TotalDefenses.Any(d => d.Type == DefenseType.Shield && !d.Dead))
                return false;
            return true;
        }

        internal static int GetRegen(AttackType attackType, bool moved, bool attacked, bool defended, bool inBuild) => attackType switch
        {
            //reduce with dmg pct?
            AttackType.Kinetic => attacked ? 0 : (inBuild ? 3 : 2),
            AttackType.Energy => attacked ? 2 : 3,
            AttackType.Explosive => attacked ? 0 : (inBuild ? 3 : 1),
            _ => throw new Exception(),
        };
        internal static int GetRegen(DefenseType defenseType, bool moved, bool attacked, bool defended, bool inRepair) => defenseType switch
        {
            //reduce with dmg pct?
            DefenseType.Hits => 0, //need to fix Hits repair system
            DefenseType.Shield => 1,
            DefenseType.Armor => inRepair ? (defended ? 1 : 2) : 0,
            _ => throw new Exception(),
        };
        internal static double GetRegenCostMult(DefenseType defenseType, bool isAttacker, out bool mass)
        {
            double result;
            switch (defenseType)
            {
                case DefenseType.Shield:
                    mass = false;
                    result = Consts.EnergyPerShield;
                    break;
                case DefenseType.Armor:
                    mass = true;
                    result = Consts.MassPerArmor;
                    break;
                default: throw new Exception();
            };
            if (!isAttacker)
                result /= Consts.RegenCostPassiveDiv;
            return result;
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
            DefenseType.Hits => .78,
            DefenseType.Shield => 1.3,
            DefenseType.Armor => .91,
            _ => throw new Exception(),
        };
        internal static double EnergyCostRatio(AttackType attackType) => attackType switch
        {
            AttackType.Kinetic => .21,
            AttackType.Energy => .78,
            AttackType.Explosive => .52,
            _ => throw new Exception(),
        };
        internal static double EnergyCostRatio(DefenseType defenseType) => defenseType switch
        {
            DefenseType.Hits => .26,
            DefenseType.Shield => .91,
            DefenseType.Armor => .13,
            _ => throw new Exception(),
        };
    }
}
