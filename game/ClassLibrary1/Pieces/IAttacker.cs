using System;
using System.Collections.Generic;
using static ClassLibrary1.Map.Map;
using static ClassLibrary1.Pieces.Attacker;
using AttackType = ClassLibrary1.Pieces.CombatTypes.AttackType;

namespace ClassLibrary1.Pieces
{
    public interface IAttacker : IBehavior
    {
        public IReadOnlyList<Attack> Attacks { get; }
        public bool Attacked { get; }

        //public bool Attacked => Attacks.Any(a => a.Attacked);
        //public double TotalAttackCur2 { get; }
        //public double TotalAttackMax2 { get; }
        //public double TotalAttackCurValue2 { get; }
        //public double TotalAttackMaxValue2 { get; }

        void Upgrade(IEnumerable<Values> values);
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
        public readonly struct Values
        {
            public readonly AttackType Type;
            private readonly int _attack, _reload;

            private readonly double _range;
            public Values(AttackType type, int attack, double range, int? reload = null)
            {
                this.Type = type;
                if (attack < 1)
                    attack = 1;
                this._attack = attack;
                this._range = range;
                this._reload = reload ?? CombatTypes.GetReloadBase(type, attack);
                if (Attack < 1 || Range < 1 || Reload < 1 || Attack < Reload)
                    throw new Exception();
            }
            public int Attack => _attack;
            public double Range => _range;
            public int Reload => _reload;
        }

    }
}
