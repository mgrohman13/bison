using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using static ClassLibrary1.Map.Map;
using static ClassLibrary1.Pieces.Behavior.Combat.Attacker;
using AttackType = ClassLibrary1.Pieces.Behavior.Combat.CombatTypes.AttackType;

namespace ClassLibrary1.Pieces.Behavior.Combat
{
    public interface IAttacker : IBehavior
    {
        public IReadOnlyList<Attack> Attacks { get; }
        public bool Attacked { get; internal set; }

        //public bool Attacked => Attacks.Any(a => a.Attacked);
        //public double TotalAttackCur2 { get; }
        //public double TotalAttackMax2 { get; }
        //public double TotalAttackCurValue2 { get; }
        //public double TotalAttackMaxValue2 { get; }

        void Upgrade(IEnumerable<Values> values, bool resetFlags = false, IReadOnlyList<int> setCur = null);
        public bool Fire(IKillable killable);
        internal bool EnemyFire(IKillable killable, Attack attack = null);

        //public interface IAttack 
        //{ 
        //}

        //[NonSerialized]
        //static

        public Events Event { get; }
        internal bool RestrictMove { get; }

        internal void RaiseAttackEvent(Attack attack, IKillable killable, Tile targetTile);

        [Serializable]
        [DataContract(IsReference = true)]
        public readonly struct Values
        {
            public readonly AttackType Type;
            private readonly int _attack, _reload;

            private readonly double _range;

            public Values(AttackType type, int attack, double range, int reload)
                : this(null, type, attack, range, reload)
            { }
            public Values(CombatTypes combatTypes, AttackType type, int attack, double range)
                : this(combatTypes, type, attack, range, null)
            { }
            private Values(CombatTypes combatTypes, AttackType type, int attack, double range, int? reload)
            {
                Type = type;
                if (attack < 1)
                    attack = 1;
                _attack = attack;
                _range = range;
                _reload = reload ?? combatTypes.GetReload(type, attack);
                if (Attack < 1 || Range < 1 || Reload < 1 || Attack < Reload)
                    throw new Exception();
            }
            public Values(CombatTypes combatTypes, Attack attack)
                : this(combatTypes, attack.Type, attack.AttackMax, attack.RangeBase, attack.ReloadBase)
            { }
            public Values(CombatTypes combatTypes, Values attack)
                : this(combatTypes, attack.Type, attack.Attack, attack.Range, attack.Reload)
            { }

            public int Attack => _attack;
            public double Range => _range;
            public int Reload => _reload;

            public static bool operator !=(Values left, Values right) => !(left == right);
            public static bool operator ==(Values left, Values right) => left.Equals(right);

            public override bool Equals([NotNullWhen(true)] object obj)
            {
                if (obj == null)
                    return false;
                Values other = (Values)obj;
                return Type == other.Type && _attack == other._attack
                    && _reload == other._reload && _range == other._range;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Type, Attack, Reload, Range);
            }
        }

    }
}
