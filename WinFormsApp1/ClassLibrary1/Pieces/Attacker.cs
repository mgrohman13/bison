using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MattUtil;
using ClassLibrary1;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using Values = ClassLibrary1.Pieces.IAttacker.Values;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public class Attacker : IAttacker
    {
        private readonly Piece _piece;
        private readonly List<Attack> _attacks;

        public Piece Piece => _piece;
        public IReadOnlyCollection<Attack> Attacks => _attacks.AsReadOnly();

        internal Attacker(Piece piece, IEnumerable<Values> attacks)
        {
            this._piece = piece;
            this._attacks = attacks.Select(a => new Attack(Piece, a)).ToList();
        }

        bool IAttacker.Fire(IKillable target)
        {
            bool fire = (Piece.IsPlayer && target != null && target.Piece.IsEnemy && target.Piece.Tile.Visible);
            return Fire(fire, target);
        }
        bool IAttacker.EnemyFire(IKillable target)
        {
            bool fire = (Piece.IsEnemy && target != null && target.Piece.IsPlayer);
            return Fire(fire, target);
        }
        private bool Fire(bool fire, IKillable target)
        {
            bool retVal = false;
            if (fire)
                foreach (Attack attack in Game.Rand.Iterate(Attacks))
                {
                    retVal |= attack.Fire(target);
                    if (target.Dead)
                        break;
                }
            return retVal;
        }

        void IBehavior.GetUpkeep(ref double energy, ref double mass)
        {
            var used = Attacks.Where(a => a.Attacked);
            //return (used.Sum(a => Math.Pow(a.Damage, Consts.WeaponDamageUpkeepPow)) + used.Count) * Consts.WeaponRechargeUpkeep;
            energy += used.Count() * Consts.WeaponRechargeUpkeep;
        }
        void IBehavior.EndTurn()
        {
            foreach (Attack attack in Attacks)
                attack.EndTurn();
        }

        [Serializable]
        public class Attack
        {
            public readonly Piece Piece;
            private readonly double _damage, _armorPierce, _shieldPierce, _dev, _range;
            private bool _attacked;

            public bool Attacked => _attacked;
            public double Damage => Consts.GetDamagedValue(Piece, _damage, 0);
            public double ArmorPierce => _armorPierce;
            public double ShieldPierce => _shieldPierce;
            public double Dev => _dev;
            public double Range => Consts.GetDamagedValue(Piece, _range, 1);

            internal Attack(Piece piece, Values values)
            {
                this.Piece = piece;
                this._damage = values.Damage;
                this._armorPierce = values.ArmorPierce;
                this._shieldPierce = values.ShieldPierce;
                this._dev = values.Dev;
                this._range = values.Range;
                this._attacked = true;
            }

            internal bool Fire(IKillable target)
            {
                if (!_attacked && Piece.Side != target.Piece.Side && Piece.Tile.GetDistance(target.Piece.Tile) <= Range)
                {
                    double shieldDmg = 0;
                    if (target.ShieldCur > 0)
                        shieldDmg = Damage * (1 - ShieldPierce);
                    double damage = Damage - shieldDmg;

                    damage = Rand(damage);
                    shieldDmg = Rand(shieldDmg);

                    if (shieldDmg > target.ShieldCur)
                    {
                        damage += shieldDmg - target.ShieldCur;
                        shieldDmg = target.ShieldCur;
                    }
                    if (target.Armor > 0)
                        damage *= 1 - target.Armor * (1 - ArmorPierce);

                    target.Damage(damage, shieldDmg);

                    Debug.WriteLine("{0} -> {1} {2:0.0} -{3:0.0}{4}", this.Piece, target.Piece, target.HitsCur, damage,
                        shieldDmg > 0 ? string.Format(" , {0:0.0} -{1:0.0}", target.ShieldCur, shieldDmg) : "");

                    this._attacked = true;
                    return true;
                }
                return false;
            }
            private double Rand(double v)
            {
                if (v > 0)
                    return Game.Rand.GaussianOE(v, Dev, Dev);
                return 0;
            }

            internal void EndTurn()
            {
                this._attacked = false;
            }
        }
    }
}
