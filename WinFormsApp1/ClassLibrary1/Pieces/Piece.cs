using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public abstract class Piece : IBehavior
    {
        public readonly Game Game;
        public readonly Side _side;
        public readonly int PieceNum;

        Piece IBehavior.Piece => this;
        protected IReadOnlyCollection<IBehavior> behavior;
        protected void SetBehavior(params IBehavior[] behavior)
        {
            this.behavior = behavior.ToList().AsReadOnly();
        }

        private Map.Tile _tile;

        public Side Side => _side;
        public Map.Tile Tile => _tile;

        public bool IsPlayer => Side != null && Side == Game.Player;
        public bool IsEnemy => Side != null && Side == Game.Enemy;

        internal Piece(Side side, Map.Tile tile)
        {
            this.Game = tile.Map.Game;
            this._side = side;
            this._tile = tile;
            this.PieceNum = Game.GetPieceNum(this.GetType());
        }

        internal virtual void Die()
        {
            Game.RemovePiece(this);
        }

        internal void SetTile(Map.Tile tile)
        {
            if (this.Tile != null)
                Game.Map.RemovePiece(this);
            this._tile = tile;
            if (tile != null)
                Game.Map.AddPiece(this);
        }

        void IBehavior.GetUpkeep(ref double energy, ref double mass)
        {
            GetUpkeep(ref energy, ref mass);
        }
        internal virtual void GetUpkeep(ref double energy, ref double mass)
        {
            foreach (IBehavior behavior in this.behavior)
                behavior.GetUpkeep(ref energy, ref mass);
        }
        void IBehavior.EndTurn()
        {
            EndTurn();
        }
        internal virtual void EndTurn()
        {
            foreach (IBehavior behavior in behavior)
                behavior.EndTurn();
        }
    }
}
