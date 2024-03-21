using System;
using System.Collections.Generic;

namespace ClassLibrary1.Pieces
{
    public interface IAttacker : IBehavior
    {
        public IReadOnlyCollection<Attack> Attacks { get; }

        void Upgrade(IEnumerable<Values> values);
        public bool Fire(IKillable killable);
        internal bool EnemyFire(IKillable killable);

        //public interface IAttack 
        //{ 
        //}

        [Serializable]
        public readonly struct Values
        {
            private readonly int _attack;
            //private readonly int _numAttacks;

            //private readonly double _armorPierce, _shieldPierce, _dev, _range;
            public Values(int attack)//, double _armorPierce, double _shieldPierce, double _dev, double _range)
            {
                if (attack < 1)
                    attack = 1;
                this._attack = attack;
                //this._armorPierce = _armorPierce;
                //this._shieldPierce = _shieldPierce;
                //this._dev = _dev;
                //this._range = _range;
                //if (Dev < .0052)
                //    this._dev = 0;
            }
            public double Range => 1.5;
            public int Attack => _attack;
            //public double ArmorPierce => _armorPierce;
            //public double ShieldPierce => _shieldPierce;
            //public double Dev => _dev;
        }

    }
}
