using System;
using System.Collections.Generic;
using System.Linq;
using AttackType = ClassLibrary1.Pieces.CombatTypes.AttackType;
using Tile = ClassLibrary1.Map.Tile;
using Values = ClassLibrary1.Pieces.IAttacker.Values;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public class Attack
    {
        public const double MELEE_RANGE = 1.5;
        public const double MIN_RANGED = MELEE_RANGE * 2;

        public readonly Piece Piece;
        private Values _values;

        private int _attackCur;
        private bool _attacked;

        public AttackType Type => _values.Type;
        public bool Attacked => _attacked;
        public int AttackCur => _attackCur;
        public int AttackMax => _values.Attack;
        public double Range => Consts.GetDamagedValue(Piece, RangeBase, MELEE_RANGE);
        public double RangeBase => _values.Range;

        public double Reload =>
            CombatTypes.GetReload(this, Attacked, Piece.HasBehavior(out IKillable killable) ? killable.Hits.GetRepair() : 0);
        public int ReloadBase => _values.Reload;

        internal Attack(Piece piece, Values values)
        {
            this.Piece = piece;
            this._values = values;

            this._attackCur = CombatTypes.GetStartCur(values.Type, values.Attack);
            this._attacked = true;
        }

        internal void Upgrade(Values values)
        {
            double attPct = Consts.StatValue(AttackCur) / Consts.StatValue(AttackMax);
            this._values = values;
            this._attackCur = Game.Rand.Round(Consts.StatValueInverse(Consts.StatValue(AttackMax) * attPct));
        }

        internal void Damage()
        {
            if (AttackCur > 0)
                this._attackCur--;
        }

        public bool CanAttack() => !Attacked && AttackCur > 0;
        public IEnumerable<IKillable> GetDefenders(Piece target, Tile attackFrom = null)//Dictionary<IKillable, int>
        {
            attackFrom ??= Piece.Tile;
            if (!CanAttack(target, attackFrom))
            {
                var adjacent = AdjacentPieces(target).Where(p => CanAttack(p, attackFrom));
                if (adjacent.Any())
                    target = Game.Rand.SelectValue(adjacent);
                else
                    return Enumerable.Empty<IKillable>();
            }

            var defenders = AdjacentPieces(target)
                .Where(p => CanAttack(p, null, false) && p.HasBehavior<IAttacker>())
                //.Concat(new[] { target })
                .Select(p => p.GetBehavior<IKillable>());

            //int GetMaxDef(IKillable k) => k.GetDefenses(attack).Max(d => d?.DefenseCur) ?? 0;
            //int GetSufficientDef(IKillable k)
            //{
            //    int maxDef = GetMaxDef(k);
            //    if (maxDef < Math.Ceiling(attack.Rounds))
            //        maxDef = 0;
            //    return maxDef;
            //}
            //friendly = Game.Rand.Iterate(friendly).OrderByDescending(GetSufficientDef)
            //    .ThenByDescending(k => k.DefenseCur)
            //    .ThenByDescending(GetMaxDef)
            //    .First();

            int? maxDef = defenders.Max(MaxDef);
            defenders = defenders.Where(k => MaxDef(k) >= maxDef).ToHashSet();
            if (!defenders.Any())
                defenders = new[] { target.GetBehavior<IKillable>() };
            return defenders;

            bool CanAttack(Piece target, Tile attackFrom, bool checkRange = true) => this.CanAttack()
                && target != null && target.Side != this.Piece.Side
                && target.HasBehavior(out IKillable killable) && !killable.Dead
                && (!checkRange || attackFrom.GetDistance(target.Tile) <= this.Range);
            static IEnumerable<Piece> AdjacentPieces(Piece target) => target.Tile.GetAdjacentTiles().Select(t => t.Piece).Where(p => p?.Side == target.Side);
            static int? MaxDef(IKillable killable) => killable.TotalDefenses.Max(d => d?.DefenseCur);
        }

        internal bool Fire(IKillable target)
        {
            var defenders = GetDefenders(target.Piece);
            if (defenders.Any())
            {
                target = Game.Rand.SelectValue(defenders);
                bool DoAtt() => this.AttackCur > 0 && !target.Dead;
                if (DoAtt())
                {
                    target.OnAttacked();
                    int startAttack = this.AttackCur;
                    Dictionary<Defense, int> startDefense = target.TotalDefenses.ToDictionary(d => d, d => d.DefenseCur);

                    int rounds = this.AttackCur;
                    for (int a = 0; a < rounds && DoAtt(); a++)
                        if (Game.Rand.Bool())
                        {
                            Defense defense = Game.Rand.Iterate(target.TotalDefenses.Where(d => !d.Dead)).OrderBy(CombatTypes.CompareDef).First();
                            bool activeDefense = target.HasBehavior<IAttacker>();

                            if (Game.Rand.Next(AttackCur + defense.DefenseCur) < AttackCur)
                            {
                                defense.Damage(this);
                            }
                            else
                            {
                                if (activeDefense)
                                    this._attackCur--;
                                if (Game.Rand.Bool())
                                    rounds--;
                            }
                        }

                    this._attacked = true;

                    Piece.GetBehavior<IAttacker>().RaiseAttackEvent(this, target);
                    Piece.Game.Log.LogAttack(this, startAttack, target, startDefense);
                    return true;
                }
            }
            return false;
        }

        public void GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            EndTurn(false, ref energyUpk, ref massUpk);
        }
        internal void StartTurn()
        {
            this._attacked = false;
        }
        internal void EndTurn(ref double energyUpk, ref double massUpk)
        {
            EndTurn(true, ref energyUpk, ref massUpk);
        }
        private void EndTurn(bool doEndTurn, ref double energyUpk, ref double massUpk)
        {
            double newValue = Consts.IncStatValue(doEndTurn, AttackCur, AttackMax, Reload, Consts.EnergyPerAttack, ref energyUpk);
            if (doEndTurn)
            {
                if (newValue != (int)newValue)
                    throw new Exception();
                this._attackCur = (int)newValue;
            }
        }
    }
}
