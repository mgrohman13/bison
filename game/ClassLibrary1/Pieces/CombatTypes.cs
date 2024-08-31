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
        internal static int GetReload(AttackType attackType, int attack)
        {
            double avg = ReloadAvg(attackType, attack);
            int lowerCap = 1;
            int upperCap = Math.Max(attack - 1, lowerCap);
            return Math.Min(Math.Max(lowerCap, Game.Rand.GaussianInt(avg, .13)), upperCap);
        }
        internal static double ReloadAvg(AttackType attackType, int attack)
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
            return avg;
        }
        internal static double ReloadAvg(int attack) => Math.Sqrt(attack);

        internal static int GetDefenceChance(Defense defense)
        {
            if (defense.DefenseCur == 0)
                return 0;

            bool hits = defense.Type == DefenseType.Hits;
            double offset = hits ? .5 : 0;
            double cur = Consts.StatValue(defense.DefenseCur - offset);
            double pct = cur / Consts.StatValue(defense.DefenseMax - offset);

            double chance = cur;
            switch (defense.Type)
            {
                case DefenseType.Hits:
                    chance /= 4.0;
                    break;
                case DefenseType.Shield:
                    double mult = (1 + pct);
                    chance *= mult * mult;
                    if (pct >= 1)
                        chance *= 2;
                    break;
            }
            return Game.Rand.Round(1 + chance);
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
            if (defense.Type == DefenseType.Hits && Game.Rand.Bool())
                return !defense.Piece.GetBehavior<IKillable>().Protection.Any(d => d.Type == DefenseType.Armor && !d.Dead);
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
            AttackType.Energy => .65,
            AttackType.Explosive => .39,
            _ => throw new Exception(),
        };
        internal static double EnergyCostRatio(DefenseType defenseType) => defenseType switch
        {
            DefenseType.Hits => .26,
            DefenseType.Shield => .78,
            DefenseType.Armor => .13,
            _ => throw new Exception(),
        };
    }
}
