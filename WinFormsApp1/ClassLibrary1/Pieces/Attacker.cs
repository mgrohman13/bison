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
    public class Attacker : IAttacker
    {
        private readonly Piece _piece;
        private readonly List<Attack> _attacks;

        public Piece Piece => _piece;
        public ReadOnlyCollection<Attack> Attacks => _attacks.AsReadOnly();

        internal Attacker(Piece piece, List<Values> attacks)
        {
            this._piece = piece;
            this._attacks = new List<Attack>(attacks.Count);
            for (int a = 0; a < attacks.Count; a++)
                this._attacks.Add(new Attack(Piece, attacks[a]));
        }

        bool IAttacker.Fire(IKillable target)
        {
            bool fire = (Piece.IsPlayer && target != null && target.Piece.IsEnemy && Piece.Game.Map.Visible(target.Piece.Tile));
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
                    retVal |= attack.Fire(target);
            return retVal;
        }

        double IBehavior.GetUpkeep()
        {
            return Attacks.Count(a => !a.Attacked) * Consts.WeaponRechargeUpkeep;
        }
        void IBehavior.EndTurn()
        {
            foreach (Attack attack in Attacks)
                attack.EndTurn();
        }

        public class Attack
        {
            public readonly Piece Piece;
            private readonly double _damage, _armorPierce, _shieldPierce, _dev, _range;
            private bool _attacked;

            public bool Attacked => _attacked;
            public double Damage => _damage;
            public double ArmorPierce => _armorPierce;
            public double ShieldPierce => _shieldPierce;
            public double Dev => _dev;
            public double Range => _range;

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
                    if (target.Armor > 0)
                        damage *= 1 - target.Armor * (1 - ArmorPierce);

                    damage = Rand(damage);
                    shieldDmg = Rand(shieldDmg);

                    target.Damage(ref damage, ref shieldDmg);

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
