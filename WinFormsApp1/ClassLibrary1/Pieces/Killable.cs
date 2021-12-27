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

        private int _hitsCur;
        private double _shieldCur;

        public Piece Piece => _piece;

        public Killable(Piece piece, IKillable.Values values)
            : this(piece, values, values.HitsMax, 0)
        {
        }
        public Killable(Piece piece, IKillable.Values values, int hitsCur, double shieldCur)
        {
            this._piece = piece;
            this._values = values;

            this._hitsCur = hitsCur;
            this._shieldCur = shieldCur;
        }
        public T GetBehavior<T>() where T : class, IBehavior
        {
            return _piece.GetBehavior<T>();
        }

        public int HitsCur => _hitsCur;
        public int HitsMax => _values.HitsMax;
        public double Resilience => _values.Resilience;
        public double Armor => _values.Armor;
        public double ShieldCur => _shieldCur;
        public double ShieldInc => Consts.GetDamagedValue(Piece, ShieldIncBase, 0);
        public double ShieldIncBase => _values.ShieldInc;
        public int ShieldMax => _values.ShieldMax;
        public int ShieldLimit => _values.ShieldLimit;
        public bool Dead => HitsCur <= 0.05;

        void IKillable.Upgrade(IKillable.Values values)
        {
            double hitsPct = HitsCur / (double)HitsMax;
            double oldShield = ShieldLimit;

            this._values = values;
            _hitsCur = Game.Rand.Round(HitsMax * hitsPct);

            if (ShieldLimit < oldShield)
                _shieldCur = _shieldCur * ShieldLimit / oldShield;
        }

        void IKillable.Damage(int damage, double shieldDmg)
        {
            Damage(damage, shieldDmg);
        }
        internal void Damage(int damage, double shieldDmg)
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
                //each additional repairer contributes a reduced amount 
                for (int a = 0; a < repairs.Length; a++)
                    hitsInc += repairs[a] / (a + 1.0);

                if (doRepair)
                    hitsInc = Game.Rand.GaussianCappedInt(hitsInc, Consts.HitsIncDev);

                hitsInc = Math.Min(hitsInc, HitsMax - HitsCur);
                massCost = repairable.RepairCost * hitsInc / (double)HitsMax;

                if (doRepair)
                {
                    _hitsCur += (int)hitsInc;
                    if ((int)hitsInc != hitsInc || _hitsCur > HitsMax)
                        throw new Exception();
                }
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
