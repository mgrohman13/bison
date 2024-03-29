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

        internal void Upgrade(Values values)
        {
            double attPct = DefenseCur / (double)DefenseMax;
            this._values = values;
            this._defenseCur = Game.Rand.Round(DefenseMax * attPct);
        }

        internal void Damage(Attack attack)
        {
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
                //??? - needs simpler system
                hitsInc = GetRepair();

                if (doEndTurn)
                    hitsInc = Game.Rand.GaussianCappedInt(hitsInc, Consts.HitsIncDev);

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
            double hitsInc = 0;
            if (Piece is IKillable.IRepairable repairable)
            {
                //check blocks
                double[] repairs = Piece.Side.PiecesOfType<IRepair>()
                  .Where(r => Piece != r.Piece && Piece.Side == r.Piece.Side && Piece.Tile.GetDistance(r.Piece.Tile) <= r.Range)
                  .Select(r => this.DefenseMax * r.Rate)
                  .Concat(repairable.AutoRepair ? new double[] { DefenseMax * Consts.AutoRepairPct + Consts.AutoRepair } : Array.Empty<double>())
                  .OrderByDescending(v => v)
                  .ToArray();
                //each additional repairer contributes a reduced amount 
                for (int a = 0; a < repairs.Length; a++)
                    hitsInc += repairs[a] / (a + 1.0);
            }
            return hitsInc;
        }

        public void GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            EndTurn(false, ref energyUpk, ref massUpk);
        }
        internal void EndTurn(ref double energyUpk, ref double massUpk)
        {
            EndTurn(true, ref energyUpk, ref massUpk);
        }
        private void EndTurn(bool doEndTurn, ref double energyUpk, ref double massUpk)
        {
            int newValue = IncDefense(ref energyUpk, ref massUpk);
            if (doEndTurn)
                this._defenseCur = newValue;

            if (CombatTypes.Repair(Type))
            {
                Repair(doEndTurn, out _, out double massCost);
                massUpk += massCost;
            }
        }
        public int GetRegen()
        {
            double energyUpk = 0, massUpk = 0;
            return IncDefense(ref energyUpk, ref massUpk) - DefenseCur;
        }
        private int IncDefense(ref double energyUpk, ref double massUpk)
        {
            //bool moved = Piece.HasBehavior(out IMovable movable) && movable.Moved;
            return Consts.IncDefense(Type, DefenseCur, DefenseMax, GetRepair() > 0, ref energyUpk, ref massUpk);
        }
    }
}
