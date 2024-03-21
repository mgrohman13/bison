using ClassLibrary1.Pieces.Players;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public class Killable : IKillable
    {
        private readonly Piece _piece;
        //private readonly List<Defense> _defenses;
        private IKillable.Values _values;

        private int _defenseCur;

        public Piece Piece => _piece;

        public Killable(Piece piece, IKillable.Values values)
            : this(piece, values, values.Defense)
        {
        }
        public Killable(Piece piece, IKillable.Values values, int defense)
        {
            this._piece = piece;
            this._values = values;

            this._defenseCur = defense;
        }
        public T GetBehavior<T>() where T : class, IBehavior
        {
            return _piece.GetBehavior<T>();
        }

        public int DefenseCur => _defenseCur;
        public int DefenseMax => _values.Defense;

        //public int HitsCur => _hitsCur;
        //public int HitsMax => _values.HitsMax;
        public double Resilience => _values.Resilience;
        //public double Armor => _values.Armor;
        //public double ShieldCur => _shieldCur;
        //public double ShieldInc => Consts.GetDamagedValue(Piece, ShieldIncBase, 0);
        //public double ShieldIncBase => _values.ShieldInc;
        //public int ShieldMax => _values.ShieldMax;
        //public int ShieldLimit => _values.ShieldLimit;
        public bool Dead => _defenseCur < 1;

        //IReadOnlyCollection<Defense> IKillable.Defenses => _defenses.AsReadOnly();

        void IKillable.Upgrade(IKillable.Values values)
        {
            double hitsPct = DefenseCur / (double)DefenseMax;
            this._values = values;
            this._defenseCur = Game.Rand.Round(DefenseMax * hitsPct);
        }

        void IKillable.Damage(int damage)//, double shieldDmg)
        {
            this._defenseCur -= damage;
            if (this.Dead)
                Piece.Die();

            //damage attacks/piece?

            //return Damage(damage, shieldDmg);
        }
        //internal double Damage(int damage, double shieldDmg)
        //{
        //    double pct = 1;
        //    if (HitsCur < damage)
        //        pct = HitsCur / (double)damage;
        //    this._hitsCur -= damage;
        //    this._shieldCur -= shieldDmg;
        //    if (this.Dead)
        //        Piece.Die();
        //    return pct;
        //}

        void IKillable.Repair(bool doRepair, out double hitsInc, out double massCost)
        {
            Repair(doRepair, out hitsInc, out massCost);
        }
        internal void Repair(bool doRepair, out double hitsInc, out double massCost)
        {
            if (Piece is IKillable.IRepairable repairable && DefenseCur < DefenseMax)
            {
                //???
                double[] repairs = Piece.Side.PiecesOfType<IRepair>()
                  .Where(r => Piece != r.Piece && Piece.Side == r.Piece.Side && Piece.Tile.GetDistance(r.Piece.Tile) <= r.Range)
                  .Select(r => this.DefenseMax * r.Rate)
                  .Concat(repairable.AutoRepair ? new double[] { DefenseMax * Consts.AutoRepairPct + Consts.AutoRepair } : Array.Empty<double>())
                  .OrderByDescending(v => v)
                  .ToArray();

                hitsInc = 0;
                //each additional repairer contributes a reduced amount 
                for (int a = 0; a < repairs.Length; a++)
                    hitsInc += repairs[a] / (a + 1.0);

                if (doRepair)
                    hitsInc = Game.Rand.GaussianCappedInt(hitsInc, Consts.HitsIncDev);

                hitsInc = Math.Min(hitsInc, DefenseMax - DefenseCur);

                double valCur = MechBlueprint.StatValue(DefenseCur);
                double valAfter = MechBlueprint.StatValue(DefenseCur + hitsInc);
                double valMax = MechBlueprint.StatValue(DefenseMax);
                massCost = repairable.RepairCost * (valAfter - valCur) / valMax;

                if (doRepair)
                {
                    _defenseCur += (int)hitsInc;
                    if ((int)hitsInc != hitsInc || _defenseCur > DefenseMax)
                        throw new Exception();
                }
            }
            else
            {
                hitsInc = 0;
                massCost = 0;
            }
        }

        //public double GetInc()
        //{
        //    return IncShield(false);
        //}
        //private double IncShield(bool doInc)
        //{
        //    double shieldInc = Consts.IncValueWithMaxLimit(ShieldCur, ShieldInc, Consts.HitsIncDev, ShieldMax, ShieldLimit, Consts.ShieldLimitPow, doInc);
        //    if (doInc)
        //        this._shieldCur += shieldInc;
        //    return shieldInc;
        //}
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
            //energyUpk += IncShield(doEndTurn) * Consts.UpkeepPerShield;

            Repair(doEndTurn, out _, out double massCost);
            massUpk += massCost;
        }
    }
}
