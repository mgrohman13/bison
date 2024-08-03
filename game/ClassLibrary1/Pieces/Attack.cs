using System;
using System.Collections.Generic;
using System.Linq;
using AttackType = ClassLibrary1.Pieces.CombatTypes.AttackType;
using Tile = ClassLibrary1.Map.Map.Tile;
using Values = ClassLibrary1.Pieces.IAttacker.Values;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public class Attack
    {
        public const double MELEE_RANGE = 1.5;
        public const double MIN_RANGED = MELEE_RANGE * Math.PI;

        public readonly Piece Piece;
        private Values _values;

        private int _attackCur;
        private bool _attacked, _restrictMove;

        public AttackType Type => _values.Type;
        internal bool RestrictMove => _restrictMove;
        public bool Attacked
        {
            get => _attacked;
            internal set => _attacked = value;
        }
        public int AttackCur => _attackCur;
        public int AttackMax => _values.Attack;
        public double Range => RangeBase > MELEE_RANGE ? Consts.GetDamagedValue(Piece, RangeBase, 2) : MELEE_RANGE;
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
            this._restrictMove = false;
        }

        internal void Upgrade(Values values)
        {
            double attPct = Consts.StatValue(AttackCur) / Consts.StatValue(AttackMax);
            this._values = values;
            this._attackCur = Game.Rand.Round(Consts.StatValueInverse(Consts.StatValue(AttackMax) * attPct));
        }

        internal void Damage()//int damage)
        {
            this._attackCur = Math.Max(0, AttackCur - 1);// damage);
        }

        public bool CanAttack() => !Attacked && AttackCur > 0
                && !(Range > MELEE_RANGE && Piece.HasBehavior(out IMovable movable) && movable.Moved);
        public Dictionary<IKillable, int> GetDefenders(Piece target, Tile attackFrom = null)
        {
            attackFrom ??= Piece.Tile;
            bool movingRangeCheck = attackFrom == Piece.Tile || Range == MELEE_RANGE; //for AI 
            if (movingRangeCheck && CanAttack())
                return GetDefenders(Piece.Side, target, t => attackFrom.GetDistance(t.Tile) <= Range);
            return new();
        }
        public static Dictionary<IKillable, int> GetDefenders(Side attacker, Piece target) =>
            GetDefenders(attacker, target, _ => true);
        private static Dictionary<IKillable, int> GetDefenders(Side attacker, Piece target, Func<Piece, bool> InRange)
        {
            if (target == null)
                return new();

            if (!CanAttack(target, true))
            {
                var adjacent = AdjacentPieces(target).Where(p => CanAttack(p, true));
                if (adjacent.Any())
                    target = Game.Rand.SelectValue(adjacent);
                else
                    return new();
            }

            var defenders = AdjacentPieces(target)
                .Where(p => CanAttack(p, false) && p.HasBehavior<IAttacker>())
                .Select(p => p.GetBehavior<IKillable>());

            if (!defenders.Any())
                defenders = new[] { target.GetBehavior<IKillable>() };
            return defenders.ToDictionary(k => k, v => v.AllDefenses.Max(CombatTypes.GetDefenceChance));

            bool CanAttack(Piece target, bool checkRange) => target != null && target.Side != attacker
                && target.HasBehavior(out IKillable killable) && !killable.Dead
                && (!checkRange || InRange(target));
            static IEnumerable<Piece> AdjacentPieces(Piece target) =>
                target.Tile?.GetAdjacentTiles().Select(t => t.Piece).Where(p => p?.Side == target.Side)
                ?? Enumerable.Empty<Piece>();
        }

        internal bool Fire(IKillable target)
        {
            Tile targetTile = target.Piece.Tile;

            var defenders = GetDefenders(target.Piece);
            if (defenders.Any())
            {
                target = Game.Rand.SelectValue(defenders);
                bool DoAtt() => this.AttackCur > 0 && !target.Dead;
                if (DoAtt())
                {
                    target.OnAttacked();
                    int startAttack = this.AttackCur;
                    Dictionary<Defense, int> startDefense = target.AllDefenses.ToDictionary(d => d, d => d.DefenseCur);

                    int rounds = this.AttackCur;
                    for (int a = 0; a < rounds && DoAtt(); a++)
                        if (a == 0 || Game.Rand.Bool())
                        {
                            //Defense defense = Game.Rand.Iterate(target.TotalDefenses.Where(d => !d.Dead)).OrderBy(CombatTypes.CompareDef).First();
                            Defense defense = Game.Rand.SelectValue(target.AllDefenses, CombatTypes.GetDefenceChance);
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
                    if (Piece.HasBehavior(out IMovable movable) && movable.Moved)
                        this._restrictMove = true;

                    Piece.GetBehavior<IAttacker>().RaiseAttackEvent(this, target, targetTile);
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
            this._restrictMove = false;
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
