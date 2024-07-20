using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Enemies
{
    [Serializable]
    public abstract class EnemyPiece : Piece, IDeserializationCallback
    {
        protected AIState _state;
        //private AIState State => _state;// hide behind research

        //cant use these tiles as references...
        private Tile lastMove = null, curMove = null;
        //should be Point?
        public Tile LastMove => lastMove;

        private int numAtts = 0;
        private readonly List<Tuple<Tile, Tile>> lastAttacks = new();
        public List<Tuple<Tile, Tile>> LastAttacks => lastAttacks;

        internal EnemyPiece(Tile tile, AIState state)
            : base(tile.Map.Game.Enemy, tile)
        {
            this._state = state;
            SetAttackEvent(this);
        }
        public virtual void OnDeserialization(object sender)
        {
            SetAttackEvent(this);
        }
        private void SetAttackEvent(object sender)
        {
            if (this.HasBehavior(out IAttacker attacker))
            {
                ((Attacker)attacker).OnDeserialization(this);
                attacker.Event.AttackEvent += Attacker_AttackEvent;
            }
        }
        private void Attacker_AttackEvent(object sender, Attacker.AttackEventArgs e)
        {
            this.lastAttacks.Add(Tuple.Create(e.From, e.To));
            this.numAtts++;
        }

        internal override void StartTurn()
        {
            base.StartTurn();

            int remove = lastAttacks.Count - numAtts;
            if (remove > 0)
                this.lastAttacks.RemoveRange(0, remove);
            this.numAtts = 0;

            bool showAtt = LastAttacks.Any();
            static bool ShowMove(Tile tile) => tile.Visible &&
                Tile.GetAllPointsInRange(tile.Map, tile.Location, Attack.MELEE_RANGE)
                    .Where(tile.Map.Visible).Skip(3).Any();
            this.lastMove = curMove != null && (showAtt || ShowMove(curMove) || ShowMove(Tile)) ? curMove : null;
            this.curMove = Tile;

            if (showAtt || LastMove != null)
                Tile.Map.UpdateVision(new[] { lastMove, curMove });
        }

        internal virtual AIState TurnState(double difficulty, Dictionary<Tile, double> playerAttacks, HashSet<Tile> moveTiles, HashSet<IKillable> killables,
            out List<Point> path)
        {
            path = null;
            return _state;
        }

        internal override void Die()
        {
            foreach (EnemyPiece piece in Side.Pieces.OfType<EnemyPiece>())
                piece.OnDeath(this);
            base.Die();
            //Game.CountKill();
        }

        protected virtual void OnDeath(EnemyPiece enemyPiece)
        {
        }

        internal protected enum AIState// internal protected
        {
            Heal,
            Retreat,
            Fight,
            Patrol,
            Harass,
            Rush,
        }
    }
}
