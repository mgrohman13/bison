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
        public bool Attacked => _attacked || AttackCur == 0;
        //public double Upkeep => _attacked * Consts.WeaponRechargeUpkeep;
        public int AttackCur => _attackCur;
        //public int AttackCur => Consts.GetDamagedValue(Piece, AttackBase, 0);
        public int AttackMax => _values.Attack;
        //public double ArmorPierce => _values.ArmorPierce;
        //public double ShieldPierce => _values.ShieldPierce;
        //public double Dev => _values.Dev;
        public double Range => RangeBase;
        public double RangeBase => Consts.GetDamagedValue(Piece, _values.Range, MELEE_RANGE);

        public double Rounds => Math.Sqrt(AttackCur);

        internal Attack(Piece piece, Values values)
        {
            this.Piece = piece;
            this._values = values;

            this._attackCur = CombatTypes.GetStartCur(values.Type, values.Attack);
            this._attacked = true;
        }

        internal void Upgrade(Values values)
        {
            double attPct = AttackCur / (double)AttackMax;
            this._values = values;
            this._attackCur = Game.Rand.Round(AttackMax * attPct);
        }

        internal void Damage()
        {
            if (AttackCur > 0)
                this._attackCur--;
        }

        private bool CapableAttack(Piece target, out IKillable killable)
        {
            killable = target.GetBehavior<IKillable>();
            bool capableAttack = killable != null && Piece.Side != killable.Piece.Side;
            if (!capableAttack)
                killable = null;
            return capableAttack;
        }
        private bool CanAttack(Tile attackFrom, Piece target, out IKillable killable)
        {
            //check blocks
            bool canAttack = CapableAttack(target, out killable) && !Attacked && attackFrom.GetDistance(killable.Piece.Tile) <= Range;
            if (!canAttack)
                killable = null;
            return canAttack;
        }
        public IEnumerable<IKillable> GetDefenders(Piece target)
        {
            return GetDefenders(Piece.Tile, target);
        }
        internal IEnumerable<IKillable> GetDefenders(Tile attackFrom, Piece target)
        {
            if (!CanAttack(attackFrom, target, out IKillable killable))
                return Enumerable.Empty<IKillable>();

            IEnumerable<IKillable> friendly = target.Tile.GetTilesInRange(MELEE_RANGE, false, null)
                .Select(t => t.Piece)
                .Where(p => p?.Side == target.Side && p.HasBehavior<IAttacker>())
                .Select(p => p?.GetBehavior<IKillable>())
                .Where(k => k != null && !k.Dead);

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

            static int? MaxDef(IKillable k) => k.TotalDefenses.Max(d => d?.DefenseCur);
            int maxDef = friendly.Max(MaxDef) ?? 0;
            friendly = friendly.Where(k => k != null && MaxDef(k) >= maxDef);

            if (!friendly.Any())
                friendly = new[] { killable };
            return friendly;

        }
        internal bool Fire(IKillable target)
        {
            var defenders = GetDefenders(target.Piece);
            if (defenders.Any())
            {
                target = Game.Rand.SelectValue(defenders);

                int startAttack = this.AttackCur;
                Dictionary<Defense, int> startDefense = target.TotalDefenses.ToDictionary(d => d, d => d.DefenseCur);

                int rounds = Game.Rand.Round(Rounds);
                for (int a = 0; a < rounds && AttackCur > 0 && !target.Dead; a++)
                {
                    Defense defense = Game.Rand.Iterate(target.TotalDefenses.Where(d => d.DefenseCur > 0)).OrderBy(CombatTypes.OrderBy).First();

                    double attChance = AttackCur / (double)(AttackCur + defense.DefenseCur);
                    if (Game.Rand.Bool(attChance))
                        defense.Damage(this);
                    else if (target.HasBehavior<IAttacker>())
                        this._attackCur--;
                    this._attacked = true;
                }

                if (this.Attacked)
                    Piece.Game.Log.LogAttack(this, startAttack, target, startDefense);

                return true;
            }
            return false;
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
            int newValue = Consts.IncStatValue(AttackCur, AttackMax, GetRegen(), Consts.EnergyPerAttack, ref energyUpk);
            if (doEndTurn)
            {
                this._attackCur = newValue;
                this._attacked = false;
            }
        }
        public int GetRegen()
        {
            //check blocks
            bool inBuild = Piece.Side.PiecesOfType<IBuilder.IBuildMech>()
                .Any(r => Piece != r.Piece && Piece.Side == r.Piece.Side && Piece.Tile.GetDistance(r.Piece.Tile) <= r.Range);
            //bool moved = Piece.HasBehavior(out IMovable movable) && movable.Moved;

            int regen = CombatTypes.GetRegen(Type, this._attacked, inBuild);
            regen = Math.Min(regen, AttackMax - AttackCur);
            return regen;
        }
    }
}
