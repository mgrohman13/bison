using MattUtil;
using System;
using System.Collections.Generic;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Enemies
{
    [Serializable]
    public class Alien : EnemyPiece
    {
        private readonly IKillable killable;
        private readonly IAttacker attacker;
        private readonly IMovable movable;

        public Piece Piece => this;

        private Tile lastMove = null, curMove = null;
        private List<Point> path;

        //should be Point?
        public Tile LastMove => lastMove;

        private Alien(Tile tile, List<Point> path,
            IEnumerable<IKillable.Values> killable, double resilience, IEnumerable<IAttacker.Values> attacks, IMovable.Values movable)
            : base(tile)
        {
            this.path = path;

            this.killable = new Killable(this, killable, resilience);
            this.attacker = new Attacker(this, attacks);
            this.movable = new Movable(this, movable);
            SetBehavior(this.killable, this.attacker, this.movable);
        }
        internal static Alien NewAlien(Tile tile, List<Point> path,
            IEnumerable<IKillable.Values> killable, double resilience, IEnumerable<IAttacker.Values> attacks, IMovable.Values movable)
        {
            Alien obj = new(tile, path, killable, resilience, attacks, movable);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }

        internal override void StartTurn()
        {
            base.StartTurn();

            this.lastMove = Tile.Visible ? curMove : null;
            this.curMove = Tile;
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
