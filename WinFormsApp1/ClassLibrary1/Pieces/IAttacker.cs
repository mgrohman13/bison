using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using Attack = ClassLibrary1.Pieces.Attacker.Attack;

namespace ClassLibrary1.Pieces
{
    public interface IAttacker : IBehavior
    {
        public IReadOnlyCollection<Attack> Attacks { get; }

        void Upgrade(IEnumerable<Values> values);
        public bool Fire(IKillable killable);
        internal bool EnemyFire(IKillable killable);

        [Serializable]
        public struct Values
        {
            private readonly int _damage;
            private readonly double _armorPierce, _shieldPierce, _dev, _range;
            public Values(int _damage, double _armorPierce, double _shieldPierce, double _dev, double _range)
            {
                this._damage = _damage;
                this._armorPierce = _armorPierce;
                this._shieldPierce = _shieldPierce;
                this._dev = _dev;
                this._range = _range;
            }
            public double Range => _range;
            public int Damage => _damage;
            public double ArmorPierce => _armorPierce;
            public double ShieldPierce => _shieldPierce;
            public double Dev => _dev;
        }
    }
}
