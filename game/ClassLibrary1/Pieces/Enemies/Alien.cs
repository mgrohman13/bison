using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using System.Collections.ObjectModel;

namespace ClassLibrary1.Pieces.Enemies
{
    [Serializable]
    public class Alien : EnemyPiece 
    {
        private readonly IKillable killable;
        private readonly IAttacker attacker;
        private readonly IMovable movable;

        public Piece Piece => this;

        private Alien(Map.Tile tile, IKillable.Values killable, IEnumerable<IAttacker.Values> attacks, IMovable.Values movable)
            : base(tile)
        {
            this.killable = new Killable(this, killable);
            this.attacker = new Attacker(this, attacks);
            this.movable = new Movable(this, movable);
            SetBehavior(this.killable, this.attacker, this.movable);
        }
        internal static Alien NewAlien(Map.Tile tile, IKillable.Values killable, IEnumerable<IAttacker.Values> attacks, IMovable.Values movable)
        {
            Alien obj = new(tile, killable, attacks, movable);
            tile.Map.Game.AddPiece(obj);
            return obj;
        } 

        public override string ToString()
        {
            return "Alien " + PieceNum;
        } 
    }
}
