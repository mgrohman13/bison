using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;

namespace ClassLibrary1.Pieces
{
    public class Killable : IKillable
    {
        private readonly Piece _piece;
        private readonly IKillable.Values values;

        private double _hitsCur, _shieldCur;

        public Piece Piece => _piece;

        public Killable(Piece piece, IKillable.Values values)
        {
            this._piece = piece;
            this.values = values;

            this._hitsCur = values.HitsMax;
            this._shieldCur = 0;
        }

        public double HitsCur => _hitsCur;
        public double HitsMax => values.HitsMax;
        public double Armor => values.Armor;
        public double ShieldCur => _shieldCur;
        public double ShieldInc => values.ShieldInc;
        public double ShieldMax => values.ShieldMax;
        public double ShieldLimit => values.ShieldLimit;

        void IKillable.Damage(ref double damage, ref double shieldDmg)
        {
            Damage(ref damage, ref shieldDmg);
        }
        internal void Damage(ref double damage, ref double shieldDmg)
        {
            this._shieldCur -= shieldDmg;
            if (ShieldCur < 0)
            {
                shieldDmg += ShieldCur;
                damage -= ShieldCur;
                this._shieldCur = 0;
            }
            this._hitsCur -= damage;
            if (HitsCur < 0)
                Piece.Die();
        }

        double IBehavior.GetUpkeep()
        {
            return (GetInc(false) - ShieldCur) * Consts.UpkeepPerShield;
        }
        void IBehavior.EndTurn()
        {
            this._shieldCur = GetInc(true);
        }
        private double GetInc(bool rand)
        {
            return Consts.IncValueWithMaxLimit(ShieldCur, ShieldInc, Consts.ShielDev, ShieldMax, ShieldLimit, Consts.ShieldLimitPow, rand);
        }
    }
}
