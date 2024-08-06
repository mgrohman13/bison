using System;
using System.Data;
using System.Linq;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;
using Values = ClassLibrary1.Pieces.IKillable.Values;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public class Defense
    {
        public readonly Piece Piece;
        private Values _values;

        private int _defenseCur;

        public DefenseType Type => _values.Type;
        public int DefenseCur => _defenseCur;
        public int DefenseMax => _values.Defense;

        public bool Dead => _defenseCur < 1;

        internal Defense(Piece piece, Values values)
        {
            this.Piece = piece;
            this._values = values;

            this._defenseCur = CombatTypes.GetStartCur(values.Type, values.Defense);
        }
        public T GetBehavior<T>() where T : class, IBehavior
        {
            return Piece.GetBehavior<T>();
        }

        internal void SetHits(int cur, int max)
        {
            this._defenseCur = cur;
            if (max != DefenseMax)
                this._values = new(Type, max);
        }
        internal void Upgrade(Values values)
        {
            double defPct = Consts.StatValue(DefenseCur) / Consts.StatValue(DefenseMax);
            this._values = values;
            //if (Type != DefenseType.Shield) //move to CombatTypes
            this._defenseCur = Game.Rand.Round(Consts.StatValueInverse(Consts.StatValue(DefenseMax) * defPct));
            if (DefenseCur < 1)
                this._defenseCur = 1;
        }

        internal void Damage(Attack attack)
        {
            if (CombatTypes.DoSplash(attack.Type))
                foreach (Defense defense in Game.Rand.Iterate(Piece.Tile.GetAdjacentTiles()
                        .Select(t => t?.Piece)
                        .Where(p => p?.Side != attack.Piece.Side)
                        .Select(p => p?.GetBehavior<IKillable>())
                        .Where(k => k != null && !k.Dead)
                        .SelectMany(k => k.AllDefenses)))
                    if (defense != this && CombatTypes.SplashAgainst(defense) && DoCollateralDamage(defense.Piece, defense.DefenseCur))
                        defense.Damage();

            Tile tile = Piece.Tile;
            Damage();

            Piece.GetBehavior<IKillable>().RaiseDamagedEvent(attack, this, tile);
        }
        private void Damage()//int damage)
        {
            //for (int a = 0; a < damage; a++)
            //{
            if (Type == DefenseType.Hits && Piece.HasBehavior(out IAttacker attacker))
                foreach (Attack attack in Game.Rand.Iterate(attacker.Attacks))
                    if (DoCollateralDamage(null, attack.AttackCur))//null to do reduced damage
                        attack.Damage();

            if (!this.Dead)
                this._defenseCur--;

            if (Type == DefenseType.Hits && this.Dead)
                Piece.Die();
            //}
        }
        private bool DoCollateralDamage(Piece piece, int defCur)
        {
            double dmgVal = this.DefenseCur;
            double otherVal = defCur;
            double dmgChance = 0;
            if (otherVal > 0)
                if (otherVal > dmgVal)
                    dmgChance = dmgVal / otherVal;
                else
                    dmgChance = (otherVal - .5) / (double)(dmgVal + .5);

            double baseChance = .75;
            if (piece != this.Piece)
                baseChance = .50;

            return Game.Rand.Bool(baseChance * dmgChance);
        }

        internal void Repair(bool doEndTurn, out double hitsInc, out double massCost)
        {
            if (Piece is IKillable.IRepairable repairable && DefenseCur < DefenseMax && CombatTypes.Repair(Type))
            {
                hitsInc = GetRepair(doEndTurn, DefenseMax - DefenseCur);
                if (doEndTurn)
                    hitsInc = Game.Rand.Round(hitsInc);
                hitsInc = Math.Min(hitsInc, DefenseMax - DefenseCur);

                double valCur = Consts.StatValue(DefenseCur);
                double valAfter = Consts.StatValue(DefenseCur + hitsInc);
                double valMax = Consts.StatValue(DefenseMax);
                massCost = repairable.RepairCost * (valAfter - valCur) / valMax;

                if (doEndTurn)
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
        internal double GetRepair(bool doEndTurn = false, int max = 0)
        {
            double repairInc = 0;
            if (Piece is IKillable.IRepairable repairable && repairable.CanRepair())
            {
                //check blocks
                var repairers = Piece.Side.PiecesOfType<IRepair>()
                    .Where(r => Piece != r.Piece && Piece.Side == r.Piece.Side && Piece.Tile.GetDistance(r.Piece.Tile) <= r.Range);
                double[] repairs = repairers
                    .Select(r => (r?.Rate))
                    .Concat(repairable.AutoRepair ? new double?[] { Consts.AutoRepair } : Array.Empty<double?>())
                    .Where(v => v.HasValue)
                    .Select(v => v.Value)
                    .OrderByDescending(v => v)
                    .ToArray();
                //each additional repairer contributes a reduced amount 
                for (int a = 0; a < repairs.Length; a++)
                    repairInc += repairs[a] / (a + 1.0);

                if (doEndTurn)
                {
                    double pct = Math.Min(1, max / repairInc);
                    foreach (var r in repairers)
                        //not the best way to accomplish it, but the *.91 multiplier makes repairing multiple targets matter slightly
                        if (Game.Rand.Bool(pct * .91))
                            r.Repaired = true;
                        else
                            ;
                }
            }
            return repairInc;
        }

        public void GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            EndTurn(false, ref energyUpk, ref massUpk);
        }
        internal void StartTurn()
        {
        }
        internal void EndTurn(ref double energyUpk, ref double massUpk)
        {
            EndTurn(true, ref energyUpk, ref massUpk);
        }
        private void EndTurn(bool doEndTurn, ref double energyUpk, ref double massUpk)
        {
            double newValue = IncDefense(doEndTurn, ref energyUpk, ref massUpk);
            if (doEndTurn)
            {
                if (newValue != (int)newValue)
                    throw new Exception();
                this._defenseCur = (int)newValue;
            }

            if (CombatTypes.Repair(Type))
            {
                Repair(doEndTurn, out _, out double massCost);
                massUpk += massCost;
            }
        }
        public double GetRegen()
        {
            double energyUpk = 0, massUpk = 0;
            return IncDefense(false, ref energyUpk, ref massUpk) - DefenseCur;
        }
        private double IncDefense(bool doEndTurn, ref double energyUpk, ref double massUpk)
        {
            return Consts.IncDefense(doEndTurn, Type, Piece.HasBehavior<IAttacker>(), DefenseCur, DefenseMax, GetRepair(), ref energyUpk, ref massUpk);
        }
    }
}
