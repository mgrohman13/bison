using MattUtil;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Enemies
{
    [Serializable]
    public class Alien : EnemyPiece, IDeserializationCallback
    {
        private readonly IKillable killable;
        private readonly IAttacker attacker;
        private readonly IMovable movable;

        public Piece Piece => this;

        private List<Point> path;

        //cant use these tiles as references...
        private Tile lastMove = null, curMove = null;
        private int numAtts = 0;
        private readonly List<Tuple<Tile, Tile>> lastAttacks = new();

        //should be Point?
        public Tile LastMove => lastMove;
        public List<Tuple<Tile, Tile>> LastAttacks => lastAttacks;

        private Alien(Tile tile, List<Point> path,
            IEnumerable<IKillable.Values> killable, double resilience, IEnumerable<IAttacker.Values> attacks, IMovable.Values movable)
            : base(tile)
        {
            this.path = path;

            this.killable = new Killable(this, killable, resilience);
            this.attacker = new Attacker(this, attacks);
            this.movable = new Movable(this, movable);
            SetBehavior(this.killable, this.attacker, this.movable);

            OnDeserialization(this);
        }
        internal static Alien NewAlien(Tile tile, List<Point> path,
            IEnumerable<IKillable.Values> killable, double resilience, IEnumerable<IAttacker.Values> attacks, IMovable.Values movable)
        {
            Alien obj = new(tile, path, killable, resilience, attacks, movable);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }

        public void OnDeserialization(object sender)
        {
            ((Attacker)this.attacker).OnDeserialization(this);
            this.attacker.Event.AttackEvent += Attacker_AttackEvent;
        }

        private void Attacker_AttackEvent(object sender, Attacker.AttackEventArgs e)
        {
            this.lastAttacks.Add(Tuple.Create(this.Tile, e.Killable.Piece.Tile));
            this.numAtts++;
        }

        internal override void StartTurn()
        {
            base.StartTurn();

            this.lastMove = Tile.Visible ? curMove : null;
            this.curMove = Tile;

            int remove = lastAttacks.Count - numAtts;
            if (remove > 0)
                this.lastAttacks.RemoveRange(0, remove);
            this.numAtts = 0;
        }

        internal static double GetPathFindingMovement(IMovable.Values movable)
        {
            return (movable.MoveInc + movable.MoveMax) / 2.0;
        }

        public override string ToString()
        {
            return "Alien " + PieceNum;
        }
    }
}
