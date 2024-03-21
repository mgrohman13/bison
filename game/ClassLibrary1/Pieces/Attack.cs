using ClassLibrary1.Pieces.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using Tile = ClassLibrary1.Map.Tile;
using Values = ClassLibrary1.Pieces.IAttacker.Values;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public class Attack
    {
        public readonly Piece Piece;
        private Values _values;

        private int _attackCur;
        private double _attacked;

        public bool Attacked => _attacked > 0 || AttackCur == 0;
        //public double Upkeep => _attacked * Consts.WeaponRechargeUpkeep;
        public int AttackCur => _attackCur;
        //public int AttackCur => Consts.GetDamagedValue(Piece, AttackBase, 0);
        public int AttackMax => _values.Attack;
        //public double ArmorPierce => _values.ArmorPierce;
        //public double ShieldPierce => _values.ShieldPierce;
        //public double Dev => _values.Dev;
        public double Range => RangeBase;
        public double RangeBase => 1.5;

        public double Rounds => Math.Sqrt(AttackCur);

        internal Attack(Piece piece, Values values)
        {
            this.Piece = piece;
            this._values = values;

            this._attackCur = values.Attack;
            this._attacked = 1;
        }

        internal void Upgrade(Values values)
        {
            double hitsPct = AttackCur / (double)AttackMax;
            this._values = values;
            this._attackCur = Game.Rand.Round(AttackMax * hitsPct);
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
        public IEnumerable<IKillable> GetDefenders(Piece attackBy, Piece target)
        {
            return GetDefenders(attackBy.Tile, attackBy, target);
        }
        internal IEnumerable<IKillable> GetDefenders(Tile attackFrom, Piece attackBy, Piece target)
        {
            if (!CanAttack(attackFrom, target, out IKillable killable))
                return Enumerable.Empty<IKillable>();

            IEnumerable<IKillable> friendly = target.Tile.GetTilesInRange(1.5, false, null)
                .Select(t => t.Piece)
                .Where(p => p?.Side == target.Side && p.HasBehavior<IAttacker>())
                .Select(p => p?.GetBehavior<IKillable>())
                .Where(k => k != null && !k.Dead);
            int? maxDef = friendly.Max(k => k?.DefenseCur);
            friendly = friendly.Where(k => k != null && k.DefenseCur >= (maxDef ?? 0));

            if (!friendly.Any())
                friendly = Enumerable.Repeat(killable, 1);
            return friendly;
        }
        internal bool Fire(IKillable target)
        {
            var defenders = GetDefenders(Piece, target.Piece);
            if (defenders.Any())
            {
                target = Game.Rand.SelectValue(defenders);

                int dmgPos = 0, dmgNeg = 0;
                int rounds = Game.Rand.Round(Rounds);
                for (int a = 0; a < rounds && AttackCur > 0 && !target.Dead; a++)
                {
                    this._attacked = 1;
                    int total = AttackCur + target.DefenseCur;
                    if (Game.Rand.Next(total) < AttackCur)
                    {
                        dmgPos++;
                        target.Damage(1);
                    }
                    else if (target.HasBehavior<IAttacker>())
                    {
                        dmgNeg++;
                        this._attackCur--;

                        //if (AttackCur < target.DefenseCur)
                        //    break;
                    }
                }

                //if ( dmg > 0 )
                //{

                //}

                //// randomize damage first as an integer, though shields and armor may convert it back to a double
                //int randDmg = Game.Rand.GaussianOEInt(Damage, Dev, Dev);
                //double damage = randDmg;

                //double shieldDmg = Math.Min(damage * (1 - ShieldPierce), target.ShieldCur);
                //damage -= shieldDmg;

                //damage *= 1 - target.Armor * (1 - ArmorPierce);

                //// round again since shields and armor may convert it back to a double
                //int hitsDmg = Game.Rand.Round(damage);
                //this._attacked = target.Damage(hitsDmg, shieldDmg);

                if (this.Attacked)
                    Piece.Game.Log.LogAttack(Piece.GetBehavior<IAttacker>(), target, AttackCur, AttackMax, target.DefenseCur, target.DefenseMax, dmgPos, dmgNeg);

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
            this._attacked = 0;
        }
        private void EndTurn(bool doEndTurn, ref double energyUpk, ref double massUpk)
        {
            if (AttackCur < AttackMax && this._attacked == 0)
            {
                double cost = MechBlueprint.StatValue(AttackCur + 1) - MechBlueprint.StatValue(AttackCur);
                cost /= Math.Sqrt(Consts.MechStatMult);

                energyUpk += cost;

                if (doEndTurn)
                    this._attackCur++;
            }
        }
    }
}
