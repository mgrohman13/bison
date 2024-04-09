using System;
using System.Linq;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;
using Values = ClassLibrary1.Pieces.IKillable.Values;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public class Defense
    {
        public readonly Piece Piece;
        private Values _values;

        private int _defenseCur;
        private bool _defended, _resetDefended;

        public DefenseType Type => _values.Type;
        public int DefenseCur => _defenseCur;
        public int DefenseMax => _values.Defense;

        public bool Dead => _defenseCur < 1;

        public bool Defended => _defended;

        internal Defense(Piece piece, Values values)
        {
            this.Piece = piece;
            this._values = values;

            this._defenseCur = CombatTypes.GetStartCur(values.Type, values.Defense);
            this._defended = true;
            this._resetDefended = false;
        }
        public T GetBehavior<T>() where T : class, IBehavior
        {
            return Piece.GetBehavior<T>();
        }

        internal void Upgrade(Values values)
        {
            double defPct = Consts.StatValue(DefenseCur) / Consts.StatValue(DefenseMax);
            this._values = values;
            this._defenseCur = Game.Rand.Round(Consts.StatValueInverse(Consts.StatValue(DefenseMax) * defPct));
        }

        internal void Damage(Attack attack)
        {
            this._defended = true;
            this._resetDefended = false;

            if (CombatTypes.DoSplash(attack.Type, this.Type))
                foreach (Defense defense in Game.Rand.Iterate(Piece.Tile.GetAdjacentTiles()
                        .Select(t => t?.Piece)
                        .Where(p => p?.Side != attack.Piece.Side)
                        .Select(p => p?.GetBehavior<IKillable>())
                        .Where(k => k != null && !k.Dead)
                        .SelectMany(k => k.TotalDefenses)))
                    if (defense != this && CombatTypes.SplashAgainst(defense) && DoAdditionalDmg(defense.Piece, defense.DefenseCur))
                        defense.Damage();

            Damage();

            Piece.GetBehavior<IKillable>().RaiseDamagedEvent(attack, this);
        }
        private void Damage()
        {
            if (Type == DefenseType.Hits && Piece.HasBehavior(out IAttacker attacker))
                foreach (Attack attack in Game.Rand.Iterate(attacker.Attacks))
                    if (DoAdditionalDmg(attack.Piece, attack.AttackCur))
                        attack.Damage();

            if (!this.Dead)
                this._defenseCur--;

            if (Type == DefenseType.Hits && this.Dead)
                Piece.Die();
        }
        private bool DoAdditionalDmg(Piece piece, int cur)
        {
            double dmgChance = 0;
            if (cur > 0)
                if (cur > DefenseCur)
                    dmgChance = DmgValue(this.DefenseCur) / (double)DmgValue(cur);
                else
                    dmgChance = (cur + 1) / (double)(this.DefenseCur + 2);
            if (piece != this.Piece)
                dmgChance /= 2;
            return Game.Rand.Bool(dmgChance);
        }

        private static double DmgValue(int cur) => Consts.StatValue(cur) - Consts.StatValue(cur - 1);

        internal void Repair(bool doEndTurn, out double hitsInc, out double massCost)
        {
            if (Piece is IKillable.IRepairable repairable && DefenseCur < DefenseMax && CombatTypes.Repair(Type))
            {
                hitsInc = GetRepair();

                if (doEndTurn)
                    hitsInc = Game.Rand.Round(hitsInc);// Game.Rand.GaussianCappedInt(hitsInc, Consts.HitsIncDev);

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
        internal double GetRepair()
        {
            double repairInc = 0;
            if (Piece is IKillable.IRepairable repairable && repairable.CanRepair())
            {
                //check blocks
                int[] repairs = Piece.Side.PiecesOfType<IRepair>()
                    .Where(r => Piece != r.Piece && Piece.Side == r.Piece.Side && Piece.Tile.GetDistance(r.Piece.Tile) <= r.Range)
                    .Select(r => r?.Rate)
                    .Concat(repairable.AutoRepair ? new int?[] { Consts.AutoRepair } : Array.Empty<int?>())
                    .Where(v => v.HasValue)
                    .Select(v => v.Value)
                    .OrderByDescending(v => v)
                    .ToArray();
                //each additional repairer contributes a reduced amount 
                for (int a = 0; a < repairs.Length; a++)
                    repairInc += repairs[a] / (a + 1.0);
            }
            return repairInc;
        }

        public void GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            EndTurn(false, ref energyUpk, ref massUpk);
        }
        internal void StartTurn()
        {
            if (this._resetDefended)
                _defended = false;
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

                this._resetDefended = true;
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
            bool moved = Piece.GetBehavior<IMovable>()?.Moved ?? false;
            bool attacked = Piece.GetBehavior<IAttacker>()?.Attacked ?? false;
            bool defended = Piece.GetBehavior<IKillable>()?.Defended ?? false;
            return Consts.IncDefense(doEndTurn, Type, Piece.HasBehavior<IAttacker>(), DefenseCur, DefenseMax, GetRepair(), ref energyUpk, ref massUpk);
        }
    }
}
