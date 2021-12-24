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
        public T GetBehavior<T>() where T : class, IBehavior
        {
            return _piece.GetBehavior<T>();
        }

        void IAttacker.Upgrade(IEnumerable<Values> values)
        {
            Values[] attacks = values.ToArray();
            if (_attacks.Count != attacks.Length)
                throw new Exception();
            for (int a = 0; a < attacks.Length; a++)
                _attacks[a].Upgrade(attacks[a]);
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

        void IBehavior.GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            var used = Attacks.Where(a => a.Attacked);
            //return (used.Sum(a => Math.Pow(a.Damage, Consts.WeaponDamageUpkeepPow)) + used.Count) * Consts.WeaponRechargeUpkeep;
            energyUpk += used.Count() * Consts.WeaponRechargeUpkeep;
        }
        void IBehavior.EndTurn(ref double energyUpk, ref double massUpk)
        {
            ((IBehavior)this).GetUpkeep(ref energyUpk, ref massUpk);
            foreach (Attack attack in Attacks)
                attack.EndTurn();
        }

        [Serializable]
        public class Attack
        {
            public readonly Piece Piece;
            private Values _values;

            private bool _attacked;

            public bool Attacked => _attacked;
            public double Damage => Consts.GetDamagedValue(Piece, DamageBase, 0);
            public double DamageBase => _values.Damage;
            public double ArmorPierce => _values.ArmorPierce;
            public double ShieldPierce => _values.ShieldPierce;
            public double Dev => _values.Dev;
            public double Range => Consts.GetDamagedValue(Piece, RangeBase, 1);
            public double RangeBase => _values.Range;

            internal Attack(Piece piece, Values values)
            {
                this.Piece = piece;
                this._values = values;
                this._attacked = true;
            }

            internal void Upgrade(Values values)
            {
                this._values = values;
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
                    double randDmg = damage;
                    double randShieldDmg = shieldDmg;

                    if (shieldDmg > target.ShieldCur)
                    {
                        damage += shieldDmg - target.ShieldCur;
                        shieldDmg = target.ShieldCur;
                    }
                    if (target.Armor > 0)
                        damage *= 1 - target.Armor * (1 - ArmorPierce);

                    target.Damage(damage, shieldDmg);

                    Piece.Game.Log.LogAttack(Piece.GetBehavior<IAttacker>(), target, Damage, randDmg, randShieldDmg, damage, shieldDmg);

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
