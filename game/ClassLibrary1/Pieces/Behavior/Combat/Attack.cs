using System;
using System.Collections.Generic;
using System.Linq;
using AttackType = ClassLibrary1.Pieces.Behavior.Combat.CombatTypes.AttackType;
using Tile = ClassLibrary1.Map.Map.Tile;
using Values = ClassLibrary1.Pieces.Behavior.Combat.IAttacker.Values;

namespace ClassLibrary1.Pieces.Behavior.Combat
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

        public double Reload => Math.Min(AttackMax,
            CombatTypes.GetReload(this, Attacked, Piece.HasBehavior(out IKillable killable) ? killable.Hits.GetRepair() : 0));
        public int ReloadBase => _values.Reload;

        internal Attack(Piece piece, Values values)
        {
            Piece = piece;
            _values = values;

            _attackCur = CombatTypes.GetStartCur(values.Type, values.Attack);
            _attacked = true;
            _restrictMove = false;
        }

        internal void Upgrade(Values values)
        {
            double attPct = Consts.StatValue(AttackCur) / Consts.StatValue(AttackMax);
            _values = values;
            _attackCur = Game.Rand.Round(Consts.StatValueInverse(Consts.StatValue(AttackMax) * attPct));
        }

        internal void Damage()//int damage)
        {
            _attackCur = Math.Max(0, AttackCur - 1);// damage);
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
                if (adjacent.Any() && attacker.IsEnemy)
                    target = Game.Rand.SelectValue(adjacent);
                else
                    return new();
            }

            var defenders = AdjacentPieces(target)
                .Where(p => CanAttack(p, false) && p.HasBehavior<IAttacker>())
                .Select(p => p.GetBehavior<IKillable>());

            if (!defenders.Any())
                defenders = new[] { target.GetBehavior<IKillable>() };
            return defenders.ToDictionary(k => k, CombatTypes.GetPieceDefenceChance);

            bool CanAttack(Piece target, bool checkRange) => target != null && target.Side != attacker
                && target.HasBehavior(out IKillable killable) && !killable.Dead
                && (!checkRange || InRange(target));
            static IEnumerable<Piece> AdjacentPieces(Piece target) =>
                target.Tile?.GetAdjacentTiles().Select(t => t.Piece).Where(p => p?.Side == target.Side)
                ?? Enumerable.Empty<Piece>();
        }

        internal bool Missile(IKillable target, double attMult)
        {
            if (Piece.HasBehavior<IMissileSilo>())
            {
                double att = AttackCur * attMult;
                _attackCur = Game.Rand.GaussianCappedInt(att, 1 / att, 1);
                return DoFire(target);
            }
            return false;
        }
        internal bool Fire(IKillable target)
        {
            var defenders = GetDefenders(target.Piece);
            if (defenders.Any())
                return DoFire(Game.Rand.SelectValue(defenders));
            return false;
        }
        private bool DoFire(IKillable target)
        {
            bool DoAtt() => AttackCur > 0 && !target.Dead;
            if (DoAtt())
            {
                Piece.Game.Map.UpdateVision(new[] { Piece, target.Piece }.Select(p => p.Tile));

                target.OnAttacked();
                int startAttack = AttackCur;
                Dictionary<Defense, int> startDefense = target.AllDefenses.ToDictionary(d => d, d => d.DefenseCur);

                int rounds = AttackCur;
                for (int a = 0; a < rounds && DoAtt(); a++)
                    if (a == 0 || Game.Rand.Bool())
                    {
                        //Defense defense = Game.Rand.Iterate(target.TotalDefenses.Where(d => !d.Dead)).OrderBy(CombatTypes.CompareDef).First();
                        Defense defense = Game.Rand.SelectValue(target.AllDefenses, CombatTypes.GetDefenceChance);
                        bool activeDefense = target.HasBehavior<IAttacker>();

                        if (Game.Rand.Next(AttackCur + defense.DefenseCur) < AttackCur)
                        {
                            defense.DoDamage(this);
                        }
                        else
                        {
                            if (activeDefense)
                                _attackCur--;
                            if (Game.Rand.Bool())
                                rounds--;
                        }
                    }

                _attacked = true;
                if (Piece.HasBehavior(out IMovable movable) && movable.Moved)
                    _restrictMove = true;

                if (Piece.HasBehavior(out IAttacker attacker))
                    attacker.RaiseAttackEvent(this, target, target.Piece.Tile);
                Piece.Game.Log.LogAttack(this, startAttack, target, startDefense);
                return true;
            }
            return false;
        }

        public void GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            EndTurn(false, ref energyUpk, ref massUpk);
        }
        internal void StartTurn()
        {
            _attacked = false;
            _restrictMove = false;
        }
        internal void EndTurn(ref double energyUpk, ref double massUpk)
        {
            EndTurn(true, ref energyUpk, ref massUpk);
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private void EndTurn(bool doEndTurn, ref double energyUpk, ref double massUpk)
        {
            double newValue = Consts.IncStatValue(doEndTurn, AttackCur, AttackMax, Reload, Consts.EnergyPerAttack, ref energyUpk);
            if (doEndTurn)
            {
                if (newValue != (int)newValue)
                    throw new Exception();
                _attackCur = (int)newValue;
            }
        }
    }
}
