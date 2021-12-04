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
    [Serializable]
    public class Killable : IKillable
    {
        private readonly Piece _piece;
        private readonly IKillable.Values _values;

        private double _hitsCur, _shieldCur;

        public Piece Piece => _piece;

        public Killable(Piece piece, IKillable.Values values)
            : this(piece, values, 0)
        {
        }
        public Killable(Piece piece, IKillable.Values values, double shieldCur)
        {
            this._piece = piece;
            this._values = values;

            this._hitsCur = values.HitsMax;
            this._shieldCur = shieldCur;
        }

        double IKillable.RepairCost => throw new NotImplementedException();

        public double HitsCur => _hitsCur;
        public double HitsMax => _values.HitsMax;
        public double Resilience => _values.Resilience;
        public double Armor => _values.Armor;
        public double ShieldCur => _shieldCur;
        public double ShieldInc => Consts.GetDamagedValue(Piece, _values.ShieldInc, 0);
        public double ShieldMax => _values.ShieldMax;
        public double ShieldLimit => _values.ShieldLimit;
        public bool Dead => HitsCur <= 0.05;

        void IKillable.Damage(double damage, double shieldDmg)
        {
            Damage(damage, shieldDmg);
        }
        internal void Damage(double damage, double shieldDmg)
        {
            this._hitsCur -= damage;
            this._shieldCur -= shieldDmg;
            if (this.Dead)
                Piece.Die();
        }
        void IKillable.Repair(double hits)
        {
            Repair(hits);
        }
        internal void Repair(double hits)
        {
            this._hitsCur += hits;
            if (HitsCur > HitsMax)
                this._hitsCur = HitsMax;
        }

        void IBehavior.GetUpkeep(ref double energy, ref double mass)
        {
            energy += GetInc(false) * Consts.UpkeepPerShield;
        }
        void IBehavior.EndTurn()
        {
            this._shieldCur += GetInc(true);
        }
        public double GetInc()
        {
            return GetInc(false);
        }
        private double GetInc(bool rand)
        {
            return Consts.IncValueWithMaxLimit(ShieldCur, ShieldInc, Consts.HitsIncDev, ShieldMax, ShieldLimit, Consts.ShieldLimitPow, rand);
        }
    }
}
