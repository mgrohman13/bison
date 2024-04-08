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
        //public double Upkeep => _attacked * Consts.WeaponRechargeUpkeep;
        public int AttackCur => _attackCur;
        //public int AttackCur => Consts.GetDamagedValue(Piece, AttackBase, 0);
        public int AttackMax => _values.Attack;
        //public double ArmorPierce => _values.ArmorPierce;
        //public double ShieldPierce => _values.ShieldPierce;
        //public double Dev => _values.Dev;
        public double Range => RangeBase;
        public double RangeBase => Consts.GetDamagedValue(Piece, _values.Range, MELEE_RANGE);

        public double Rounds => Math.Sqrt(AttackCur);// AttackCur;// Math.Sqrt(AttackCur);

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
        public IEnumerable<IKillable> GetDefenders(Piece target, Tile attackFrom = null)
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

                int startAttack = this.AttackCur;
                Dictionary<Defense, int> startDefense = target.TotalDefenses.ToDictionary(d => d, d => d.DefenseCur);

                int rounds = Game.Rand.Round(Rounds);
                for (int a = 0; a < rounds && AttackCur > 0 && !target.Dead; a++)
                {
                    Defense defense = Game.Rand.Iterate(target.TotalDefenses.Where(d => !d.Dead)).OrderBy(CombatTypes.CompareDef).First();
                    bool activeDefense = target.HasBehavior<IAttacker>();

                    //int att = Game.Rand.RangeInt(0, AttackCur);
                    //int def = Game.Rand.RangeInt(0, defense.DefenseCur);
                    //if (att > def || (att == def && !activeDefense))
                    double attChance = AttackCur / (double)(AttackCur + defense.DefenseCur);
                    if (Game.Rand.Bool(attChance))
                        defense.Damage(this);
                    else if (activeDefense)//&& def < att
                        this._attackCur--;
                    this._attacked = true;
                }

                if (this.Attacked)
                {
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
            int newValue = Consts.IncStatValue(AttackCur, AttackMax, GetRegen(), Consts.EnergyPerAttack, ref energyUpk);
            if (doEndTurn)
            {
                this._attackCur = newValue;
                //this._attacked = false;
            }
        }
        public int GetRegen()
        {
            //check blocks
            bool inBuild = Piece.Side.PiecesOfType<IBuilder.IBuildMech>()
                .Any(r => Piece != r.Piece && Piece.Side == r.Piece.Side && Piece.Tile.GetDistance(r.Piece.Tile) <= r.Range);
            bool moved = Piece.GetBehavior<IMovable>()?.Moved ?? false;
            bool defended = Piece.GetBehavior<IKillable>()?.Defended ?? false;

            int regen = CombatTypes.GetRegen(Type, moved, Attacked, defended, inBuild);
            regen = Math.Min(regen, AttackMax - AttackCur);
            return regen;
        }
    }
}
