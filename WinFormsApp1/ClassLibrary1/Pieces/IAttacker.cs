﻿using System;
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
    public interface IAttacker
    {
        public Piece Piece { get; }
        public ReadOnlyCollection<Attack> Attacks { get; }

        public bool Fire(IKillable killable);
        internal void EndTurn();

        public class Values
        {
            private readonly double _damage, _armorPierce, _shieldPierce, _dev, _range;
            public Values(double _damage, double _armorPierce, double _shieldPierce, double _dev, double _range)
            {
                this._damage = _damage;
                this._armorPierce = _armorPierce;
                this._shieldPierce = _shieldPierce;
                this._dev = _dev;
                this._range = _range;
            }
            public double Range => _range;
            public double Damage => _damage;
            public double ArmorPierce => _armorPierce;
            public double ShieldPierce => _shieldPierce;
            public double Dev => _dev;
        }
    }
}