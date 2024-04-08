using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Values = ClassLibrary1.Pieces.IAttacker.Values;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public class Attacker : IAttacker, IDeserializationCallback
    {
        private readonly Piece _piece;
        private readonly List<Attack> _attacks;


        public Piece Piece => _piece;
        public IReadOnlyCollection<Attack> Attacks => CombatTypes.OrderAtt(_attacks);
        public bool Attacked => Attacks.Any(a => a.Attacked);

        //public double TotalAttackCur2 => Consts.SumStats(Attacks.Select(a => a.AttackCur));
        //public double TotalAttackMax2 => Consts.SumStats(Attacks.Select(a => a.AttackMax));
        //public double TotalAttackCurValue2 => Consts.StatValue(TotalAttackCur2);
        //public double TotalAttackMaxValue2 => Consts.StatValue(TotalAttackMax2);

        internal Attacker(Piece piece, IEnumerable<Values> attacks)
        {
            this._piece = piece;
            this._attacks = attacks.Select(a => new Attack(Piece, a)).ToList();

            OnDeserialization(this);
        }

        public T GetBehavior<T>() where T : class, IBehavior
        {
            return _piece.GetBehavior<T>();
        }

        void IAttacker.Upgrade(IEnumerable<Values> values)
        {
            Values[] attacks = values.ToArray();
            ////fix
            //if (_attacks.Count != attacks.Length)
            //    throw new Exception();
            for (int a = 0; a < attacks.Length; a++)
                if (a < _attacks.Count)
                    _attacks[a].Upgrade(attacks[a]);
                else
                    _attacks.Add(new(Piece, attacks[a]));
        }

        bool IAttacker.Fire(IKillable target)
        {
            bool fire = (Piece.IsPlayer && target != null && target.Piece.IsEnemy && target.Piece.Tile.Visible);
            bool fired = Fire(fire, target);
            Piece.Tile.Map.Game.SaveGame();
            return fired;
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
            foreach (Attack attack in Attacks)
                attack.GetUpkeep(ref energyUpk, ref massUpk);
        }
        void IBehavior.StartTurn()
        {
            foreach (Attack attack in Game.Rand.Iterate(Attacks))
                attack.StartTurn();
        }
        void IBehavior.EndTurn(ref double energyUpk, ref double massUpk)
        {
            foreach (Attack attack in Game.Rand.Iterate(Attacks))
                attack.EndTurn(ref energyUpk, ref massUpk);
        }

        [NonSerialized]
        private Events _event = new();
        public Events Event => _event;
        public class Events
        {
            public delegate void AttackEventHandler(object sender, AttackEventArgs e);
            public event AttackEventHandler AttackEvent;
            internal void RaiseAttackEvent(Attack attack, IKillable killable) =>

                AttackEvent?.Invoke(this, new AttackEventArgs(attack, killable));
        }
        public class AttackEventArgs
        {
            public readonly Attack Attack;
            public readonly IKillable Killable;
            public AttackEventArgs(Attack attack, IKillable killable)
            {
                this.Attack = attack;
                this.Killable = killable;
            }
        }
        void IAttacker.RaiseAttackEvent(Attack attack, IKillable killable) => Event.RaiseAttackEvent(attack, killable);

        public void OnDeserialization(object sender)
        {
            _event ??= new();
        }
    }
}
