using System;
using System.Collections.Generic;
using static ClassLibrary1.Pieces.Attacker;
using AttackType = ClassLibrary1.Pieces.CombatTypes.AttackType;

namespace ClassLibrary1.Pieces
{
    public interface IAttacker : IBehavior
    {
        public IReadOnlyCollection<Attack> Attacks { get; }
        public bool Attacked { get; }

        //public bool Attacked => Attacks.Any(a => a.Attacked);
        //public double TotalAttackCur2 { get; }
        //public double TotalAttackMax2 { get; }
        //public double TotalAttackCurValue2 { get; }
        //public double TotalAttackMaxValue2 { get; }

        void Upgrade(IEnumerable<Values> values);
        public bool Fire(IKillable killable);
        internal bool EnemyFire(IKillable killable);

        //public interface IAttack 
        //{ 
        //}

        //[NonSerialized]
        //static

        public Events Event { get; }
        internal void RaiseAttackEvent(Attack attack, IKillable killable);


        [Serializable]
        public readonly struct Values
        {
            public readonly AttackType Type;
            private readonly int _attack;
            //private readonly int _numAttacks;

            private readonly double _range;//_armorPierce, _shieldPierce, _dev,
            public Values(AttackType type, int attack) : this(type, attack, Pieces.Attack.MELEE_RANGE) { }
            public Values(AttackType type, int attack, double range)//, double _armorPierce, double _shieldPierce, double _dev)
            {
                this.Type = type;
                if (attack < 1)
                    attack = 1;
                this._attack = attack;
                //this._armorPierce = _armorPierce;
                //this._shieldPierce = _shieldPierce;
                //this._dev = _dev;
                this._range = range;
                //if (Dev < .0052)
                //    this._dev = 0;
            }
            public double Range => _range;
            public int Attack => _attack;
            //public double ArmorPierce => _armorPierce;
            //public double ShieldPierce => _shieldPierce;
            //public double Dev => _dev;
        }

    }
}
