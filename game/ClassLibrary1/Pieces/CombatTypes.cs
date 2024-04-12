using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary1.Pieces
{
    public static class CombatTypes
    {
        public enum AttackType
        {
            Explosive,
            Energy,
            Kinetic,
        }
        public enum DefenseType
        {
            Hits,
            Armor,
            Shield,
        }

        internal static int GetStartCur(AttackType type, int attack) =>
            type == AttackType.Energy ? 0 : attack;
        internal static int GetStartCur(DefenseType type, int defense) =>
            type == DefenseType.Shield ? 0 : defense;

        internal static double GetDamageMult(AttackType type) => type switch
        {
            AttackType.Kinetic => .91,
            AttackType.Energy => 1,
            AttackType.Explosive => 1.3,
            _ => throw new Exception()
        };
        internal static int GetReloadBase(AttackType attackType, int attack)
        {
            double avg = ReloadAvg(attack) - 1;
            avg *= attackType switch
            {
                AttackType.Kinetic => 1.69,
                AttackType.Energy => 1,
                AttackType.Explosive => .65,
                _ => throw new Exception(),
            };
            avg++;

            int lowerCap = 1;
            int upperCap = Math.Max(attack - 1, lowerCap);
            return Math.Min(Math.Max(lowerCap, Game.Rand.GaussianInt(avg, .13)), upperCap);
        }
        internal static double ReloadAvg(int attack) => Math.Sqrt(attack);

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

        internal static IReadOnlyList<IAttacker.Values> OrderAtt(IEnumerable<IAttacker.Values> attacks) =>
            OrderAtt(attacks, a => a.Attack, a => a.Range, a => a.Type);
        internal static IReadOnlyList<Attack> OrderAtt(IEnumerable<Attack> attacks) =>
            OrderAtt(attacks, a => a.AttackMax, a => a.RangeBase, a => a.Type);
        private static IReadOnlyList<T> OrderAtt<T>(IEnumerable<T> attacks, Func<T, int> AttMax, Func<T, double> Range, Func<T, AttackType> Type) =>
            Game.Rand.Iterate(attacks).OrderByDescending(AttMax).ThenByDescending(Range).ThenBy(Type).ToList().AsReadOnly();
        internal static IReadOnlyList<IKillable.Values> OrderDef(IEnumerable<IKillable.Values> killable) =>
            OrderDef(killable, d => d.Type);
        internal static IReadOnlyList<Defense> OrderDef(IEnumerable<Defense> killable) =>
            OrderDef(killable, d => d.Type);
        private static IReadOnlyList<T> OrderDef<T>(IEnumerable<T> killable, Func<T, DefenseType> Type) =>
            Game.Rand.Iterate(killable).OrderBy(Type).ToList().AsReadOnly();

        internal static bool DoSplash(AttackType type) => type == AttackType.Explosive;
        internal static bool SplashAgainst(Defense defense)
        {
            if (defense.Type == DefenseType.Hits)
                return defense.Piece.GetBehavior<IKillable>().TotalDefenses.Any(d => d.Type == DefenseType.Armor && !d.Dead);
            return true;
        }

        internal static double GetReload(Attack attack, bool attacked, double repairAmt)
        {
            double reload = attack.Type switch
            {
                AttackType.Kinetic or AttackType.Explosive => attacked ? 0 : attack.ReloadBase + repairAmt,
                AttackType.Energy => attack.ReloadBase + (attacked ? 0 : 1),
                _ => throw new Exception(),
            };
            return Consts.GetDamagedValue(attack.Piece, reload, 0);
        }
        internal static double GetRegen(DefenseType defenseType, double repairAmt) => defenseType switch
        {
            DefenseType.Hits => 0,
            DefenseType.Shield => 1, //make variable, reduce with dmg pct
            DefenseType.Armor => repairAmt,
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
        internal static bool Repair(DefenseType defenseType) =>
            defenseType == DefenseType.Hits;

        internal static double Cost(AttackType attackType) => attackType switch
        {
            AttackType.Kinetic => .78,
            AttackType.Energy => 1.3,
            AttackType.Explosive => 1,
            _ => throw new Exception(),
        };
        internal static double Cost(DefenseType defenseType) => defenseType switch
        {
            DefenseType.Hits => .78,
            DefenseType.Shield => 1.3,
            DefenseType.Armor => 1,
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
