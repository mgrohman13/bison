using System;
using System.Collections.Generic;
using System.Linq;
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
            foreach (Attack attack in Attacks)
                attack.GetUpkeep(ref energyUpk, ref massUpk);
        }
        void IBehavior.EndTurn(ref double energyUpk, ref double massUpk)
        {
            ((IBehavior)this).GetUpkeep(ref energyUpk, ref massUpk);
            foreach (Attack attack in Attacks)
                attack.EndTurn(ref energyUpk, ref massUpk);
        }
    }
}
