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

        private Alien(Tile tile, IEnumerable<IKillable.Values> killable, double resilience, IEnumerable<IAttacker.Values> attacks, IMovable.Values movable)
            : base(tile)
        {
            this.killable = new Killable(this, killable, resilience);
            this.attacker = new Attacker(this, attacks);
            this.movable = new Movable(this, movable);
            SetBehavior(this.killable, this.attacker, this.movable);
        }
        internal static Alien NewAlien(Tile tile, IEnumerable<IKillable.Values> killable, double resilience, IEnumerable<IAttacker.Values> attacks, IMovable.Values movable)
        {
            Alien obj = new(tile, killable, resilience, attacks, movable);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }

        public override string ToString()
        {
            return "Alien " + PieceNum;
        }
    }
}
