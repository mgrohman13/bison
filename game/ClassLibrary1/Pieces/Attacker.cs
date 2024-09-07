using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Tile = ClassLibrary1.Map.Map.Tile;
using Values = ClassLibrary1.Pieces.IAttacker.Values;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public class Attacker : IAttacker, IDeserializationCallback
    {
        private readonly Piece _piece;
        private readonly List<Attack> _attacks;

        public Piece Piece => _piece;
        public IReadOnlyList<Attack> Attacks => CombatTypes.OrderAtt(_attacks);

        bool IAttacker.Attacked
        {
            get => Attacks.Any(a => a.Attacked);
            set
            {
                foreach (var att in Attacks)
                    att.Attacked = value;
            }
        }
        bool IAttacker.RestrictMove => Attacks.Any(a => a.RestrictMove);

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

            double energy = 0;
            while (this.Attacks.Count > attacks.Length)
            {
                var cur = Game.Rand.SelectValue(this.Attacks);
                _attacks.Remove(cur);

                energy += Consts.StatValue(cur.AttackCur) * Consts.EnergyPerAttack;
            }
            // need method 
            Piece.Side.Spend(Game.Rand.Round(-energy), 0);

            for (int a = 0; a < attacks.Length; a++)
            {
                var upg = attacks[a];
                if (a >= this.Attacks.Count)
                    _attacks.Add(new(Piece, upg));
                else
                    _attacks[a].Upgrade(upg);
            }
        }

        bool IAttacker.Fire(IKillable target)
        {
            bool fire = (Piece.IsPlayer && target != null && target.Piece.IsEnemy && target.Piece.Tile.Visible);
            bool fired = Fire(fire, target);
            Piece.Tile.Map.Game.SaveGame();
            return fired;
        }
        bool IAttacker.EnemyFire(IKillable target, Attack attack)
        {
            bool fire = (Piece.IsEnemy && target != null && target.Piece.IsPlayer);
            return Fire(fire, target, attack);
        }
        private bool Fire(bool fire, IKillable target, Attack useAtt = null)
        {
            bool retVal = false;
            if (fire)
                foreach (Attack attack in (useAtt == null ? Game.Rand.Iterate(Attacks) : new[] { useAtt }))
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
            internal void RaiseAttackEvent(Attack attack, IKillable killable, Tile targetTile) =>

                AttackEvent?.Invoke(this, new AttackEventArgs(attack, killable, targetTile));
        }
        public class AttackEventArgs
        {
            public readonly Tile From, To;
            public readonly Attack Attack;
            public readonly IKillable Killable;
            public AttackEventArgs(Attack attack, IKillable killable, Tile targetTile)
            {
                this.Attack = attack;
                this.Killable = killable;
                this.From = attack.Piece.Tile;
                this.To = targetTile;
            }
        }
        void IAttacker.RaiseAttackEvent(Attack attack, IKillable killable, Tile targetTile) => Event.RaiseAttackEvent(attack, killable, targetTile);

        public void OnDeserialization(object sender)
        {
            //base.OnDeserialization(sender);
            _event ??= new();
        }
    }
}
