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
        public readonly ISide _side;

        Piece IBehavior.Piece => this;
        protected IReadOnlyCollection<IBehavior> behavior;
        protected void SetBehavior(params IBehavior[] behavior)
        {
            this.behavior = behavior.ToList().AsReadOnly();
        }

        [NonSerialized]
        private Map.Tile _tile;

        public ISide Side => _side;
        public Map.Tile Tile => _tile;

        public bool IsPlayer => Side != null && Side == Game.Player;
        public bool IsEnemy => Side != null && Side == Game.Enemy;

        internal Piece(ISide side, Map.Tile tile)
        {
            this.Game = tile.Map.Game;
            this._side = side;
            this._tile = tile;
        }

        internal virtual void Die()
        {
            Game.RemovePiece(this);
        }

        internal void SetTile(Map.Tile tile)
        {
            Game.Map.RemovePiece(this);
            this._tile = tile;
            Game.Map.AddPiece(this);
        }

        double IBehavior.GetUpkeep()
        {
            return GetUpkeep();
        }
        public virtual double GetUpkeep()
        {
            return behavior.Sum(b => b.GetUpkeep());
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
