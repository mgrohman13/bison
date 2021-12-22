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
        private IKillable.Values _values;

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
        public T GetBehavior<T>() where T : class, IBehavior
        {
            return _piece.GetBehavior<T>();
        }

        public double HitsCur => _hitsCur;
        public double HitsMax => _values.HitsMax;
        public double Resilience => _values.Resilience;
        public double Armor => _values.Armor;
        public double ShieldCur => _shieldCur;
        public double ShieldInc => Consts.GetDamagedValue(Piece, ShieldIncBase, 0);
        public double ShieldIncBase => _values.ShieldInc;
        public double ShieldMax => _values.ShieldMax;
        public double ShieldLimit => _values.ShieldLimit;
        public bool Dead => HitsCur <= 0.05;

        void IKillable.Upgrade(IKillable.Values killable)
        {
            double hitsPct = HitsCur / HitsMax;
            this._values = killable;
            _hitsCur = HitsMax * hitsPct;
        }

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

        void IKillable.Repair(bool doRepair, out double hitsInc, out double massCost)
        {
            Repair(doRepair, out hitsInc, out massCost);
        }
        internal void Repair(bool doRepair, out double hitsInc, out double massCost)
        {
            if (Piece is IKillable.IRepairable repairable && HitsCur < HitsMax)
            {
                double[] repairs = Piece.Side.PiecesOfType<IRepair>()
                  .Where(r => Piece != r.Piece && Piece.Side == r.Piece.Side && Piece.Tile.GetDistance(r.Piece.Tile) <= r.Range)
                  .Select(r => this.HitsMax * r.Rate)
                  .Concat(repairable.AutoRepair ? new double[] { HitsMax * Consts.AutoRepairPct + Consts.AutoRepair } : Array.Empty<double>())
                  .OrderByDescending(v => v)
                  .ToArray();

                hitsInc = 0;
                for (int a = 0; a < repairs.Length; a++)
                    hitsInc += repairs[a] / (a + 1.0);

                if (doRepair)
                    hitsInc = Game.Rand.GaussianCapped(hitsInc, Consts.HitsIncDev);

                hitsInc = Math.Min(hitsInc, HitsMax - HitsCur);
                massCost = repairable.RepairCost * hitsInc / HitsMax;

                if (doRepair)
                    _hitsCur += hitsInc;
            }
            else
            {
                hitsInc = 0;
                massCost = 0;
            }
        }

        public double GetInc()
        {
            return IncShield(false);
        }
        private double IncShield(bool doInc)
        {
            double shieldInc = Consts.IncValueWithMaxLimit(ShieldCur, ShieldInc, Consts.HitsIncDev, ShieldMax, ShieldLimit, Consts.ShieldLimitPow, doInc);
            if (doInc)
                this._shieldCur += shieldInc;
            return shieldInc;
        }
        void IBehavior.GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            EndTurn(false, ref energyUpk, ref massUpk);
        }
        void IBehavior.EndTurn(ref double energyUpk, ref double massUpk)
        {
            EndTurn(true, ref energyUpk, ref massUpk);
        }
        private void EndTurn(bool doEndTurn, ref double energyUpk, ref double massUpk)
        {
            energyUpk += IncShield(doEndTurn) * Consts.UpkeepPerShield;

            Repair(doEndTurn, out _, out double massCost);
            massUpk += massCost;
        }
    }
}
