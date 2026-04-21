using ClassLibrary1.Pieces.Behavior.Combat;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using static ClassLibrary1.Map.Map;
using IEnemySpawn = ClassLibrary1.Map.Map.IEnemySpawn;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Enemies
{
    [Serializable]
    [DataContract(IsReference = true)]
    public abstract class EnemyPiece : Piece, IDeserializationCallback
    {
        protected AIState _state;
        internal AIState State => _state;// hide behind research

        //cant use these tiles as references...
        private Tile lastMove = null, curMove = null;
        protected IEnemySpawn _spawn;
        //should be Point?
        public Tile LastMove => lastMove;

        private int numAtts = 0;
        private readonly List<Tuple<Tile, Tile>> lastAttacks = [];
        public ReadOnlyCollection<Tuple<Tile, Tile>> LastAttacks => lastAttacks.AsReadOnly();

        internal IEnemySpawn Spawn => _spawn;
        internal abstract double Cost { get; }

        internal EnemyPiece(Tile tile, AIState state, IEnemySpawn spawn = null)
            : base(tile.Map.Game.Enemy, tile)
        {
            this._state = state;
            this._spawn = spawn;

            OnDeserialization(this);
        }
        public virtual void OnDeserialization(object sender)
        {
            //base.OnDeserialization(sender);
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

            bool showAtt = LastAttacks.Count > 0;
            this.lastMove = curMove != null && (showAtt || curMove.ShowMove() || Tile.ShowMove()) ? curMove : null;
            this.curMove = Tile;

            if (showAtt || LastMove != null)
                Tile.Map.UpdateVision([lastMove, curMove]);
        }

        internal virtual AIState TurnState(double difficulty, bool clearPaths, Dictionary<Tile, double> playerAttacks, HashSet<Tile> moveTiles, HashSet<IKillable> killables,
            out List<Point> path)
        {
            path = null;
            return _state;
        }

        internal override void Die(out Tile tile, out double treasure)
        {
            foreach (EnemyPiece piece in Game.Rand.Iterate(Side.Pieces.OfType<EnemyPiece>()))
                piece.OnDeath(this);
            base.Die(out tile, out treasure);
        }

        protected virtual void OnDeath(EnemyPiece enemyPiece)
        {
        }

        internal protected enum AIState
        {
            Heal,
            Retreat,
            Fight,
            Patrol,
            Harass,
            Rush,
        }
        [Serializable]
        [DataContract(IsReference = true)]
        internal class PieceSpawn : IEnemySpawn
        {
            private readonly SpawnChance _spawn = new();
            [NonSerialized]
            private Func<Tile> GetTile;
            public SpawnChance Spawner => _spawn;
            public void OnDeserialization(Func<Tile> SpawnTile) => this.GetTile = SpawnTile;
            public int SpawnChance(int turn, double? enemyMove) => Spawner.Chance;
            public Tile SpawnTile(Map.Map map) => GetTile();
        }
    }
}
