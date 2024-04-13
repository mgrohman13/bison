using System;
using System.Data;
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
            double defPct = Consts.StatValue(DefenseCur) / Consts.StatValue(DefenseMax);
            this._values = values;
            this._defenseCur = Game.Rand.Round(Consts.StatValueInverse(Consts.StatValue(DefenseMax) * defPct));
        }

        internal void Damage(Attack attack)
        {
            if (CombatTypes.DoSplash(attack.Type))
                foreach (Defense defense in Game.Rand.Iterate(Piece.Tile.GetAdjacentTiles()
                        .Select(t => t?.Piece)
                        .Where(p => p?.Side != attack.Piece.Side)
                        .Select(p => p?.GetBehavior<IKillable>())
                        .Where(k => k != null && !k.Dead)
                        .SelectMany(k => k.TotalDefenses)))
                    if (defense != this && CombatTypes.SplashAgainst(defense))
                        defense.Damage(GetAdditionalDmg(defense.Piece, defense.DefenseCur));

            Damage(1);

            Piece.GetBehavior<IKillable>().RaiseDamagedEvent(attack, this);
        }
        private void Damage(int damage)
        {
            for (int a = 0; a < damage; a++)
            {
                if (Type == DefenseType.Hits && Piece.HasBehavior(out IAttacker attacker))
                    foreach (Attack attack in Game.Rand.Iterate(attacker.Attacks))
                        attack.Damage(GetAdditionalDmg(null, attack.AttackCur));//null to do reduced damage

                if (!this.Dead)
                    this._defenseCur--;

                if (Type == DefenseType.Hits && this.Dead)
                    Piece.Die();
            }
        }
        private int GetAdditionalDmg(Piece piece, int cur)
        {
            double dmgValue = Consts.StatValue(this.DefenseCur);
            double trgValue = Consts.StatValue(cur);
            double avg = trgValue / dmgValue;
            if (piece != this.Piece)
                avg /= 2.0;
            avg = Math.Sqrt(avg);
            return Game.Rand.GaussianCappedInt(avg, .26);

            //double dmgChance = 0;
            //if (cur > 0)
            //    if (cur > DefenseCur)
            //        dmgChance = DmgValue(this.DefenseCur) / (double)DmgValue(cur);
            //    else
            //        dmgChance = (cur + 1) / (double)(this.DefenseCur + 2);
            //if (piece != this.Piece)
            //    dmgChance /= 2;
            //return Game.Rand.Bool(dmgChance);
        }

        //private static double DmgValue(int cur) => Consts.StatValue(cur) - Consts.StatValue(cur - 1);

        internal void Repair(bool doEndTurn, out double hitsInc, out double massCost)
        {
            if (Piece is IKillable.IRepairable repairable && DefenseCur < DefenseMax && CombatTypes.Repair(Type))
            {
                hitsInc = GetRepair();
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
