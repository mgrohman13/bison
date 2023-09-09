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
            //var used = Attacks.Where(a => a.Attacked);
            //return (used.Sum(a => Math.Pow(a.Damage, Consts.WeaponDamageUpkeepPow)) + used.Count) * Consts.WeaponRechargeUpkeep;
            energyUpk += Attacks.Sum(a => a.Upkeep);
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

            private double _attacked;

            public bool Attacked => _attacked > 0;
            public double Upkeep => _attacked * Consts.WeaponRechargeUpkeep;
            public double Damage => Consts.GetDamagedValue(Piece, DamageBase, 0);
            public int DamageBase => _values.Damage;
            public double ArmorPierce => _values.ArmorPierce;
            public double ShieldPierce => _values.ShieldPierce;
            public double Dev => _values.Dev;
            public double Range => Consts.GetDamagedValue(Piece, RangeBase, 1);
            public double RangeBase => _values.Range;

            internal Attack(Piece piece, Values values)
            {
                this.Piece = piece;
                this._values = values;
                this._attacked = 1;
            }

            internal void Upgrade(Values values)
            {
                this._values = values;
                //this._attacked = true;
            }

            internal bool Fire(IKillable target)
            {
                if (!Attacked && Piece.Side != target.Piece.Side && Piece.Tile.GetDistance(target.Piece.Tile) <= Range)
                {
                    // randomize damage first as an integer, though shields and armor may convert it back to a double
                    int randDmg = Game.Rand.GaussianOEInt(Damage, Dev, Dev);
                    double damage = randDmg;

                    double shieldDmg = Math.Min(damage * (1 - ShieldPierce), target.ShieldCur);
                    damage -= shieldDmg;

                    damage *= 1 - target.Armor * (1 - ArmorPierce);

                    // round again since shields and armor may convert it back to a double
                    int hitsDmg = Game.Rand.Round(damage);
                    this._attacked = target.Damage(hitsDmg, shieldDmg);

                    Piece.Game.Log.LogAttack(Piece.GetBehavior<IAttacker>(), target, Damage, randDmg, hitsDmg, shieldDmg);

                    return true;
                }
                return false;
            }

            internal void EndTurn()
            {
                this._attacked = 0;
            }
        }
    }
}
