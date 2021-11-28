﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;

namespace ClassLibrary1.Pieces
{
    public interface IKillable
    {
        public Piece Piece { get; }
        public double HitsCur { get; }
        public double HitsMax { get; }
        public double Armor { get; }
        public double ShieldCur { get; }
        public double ShieldInc { get; }
        public double ShieldMax { get; }
        public double ShieldLimit { get; }

        internal void Damage(double damage, double shieldDmg);

        public void EndTurn();

        public class Values
        {
            private readonly double _hitsMax, _armor, _shieldInc, _shieldMax, _shieldLimit;
            public Values(double hitsMax) : this(hitsMax, 0) { }
            public Values(double hitsMax, double armor) : this(hitsMax, armor, 0, 0, 0) { }
            public Values(double hitsMax, double shieldInc, double shieldMax, double shieldLimit) : this(hitsMax, 0, shieldInc, shieldMax, shieldLimit) { }
            public Values(double hitsMax, double armor, double shieldInc, double shieldMax, double shieldLimit)
            {
                this._hitsMax = hitsMax;
                this._armor = armor;
                this._shieldInc = shieldInc;
                this._shieldMax = shieldMax;
                this._shieldLimit = shieldLimit;
            }
            public double HitsMax => _hitsMax;
            public double Armor => _armor;
            public double ShieldInc => _shieldInc;
            public double ShieldMax => _shieldMax;
            public double ShieldLimit => _shieldLimit;
        }
    }
}