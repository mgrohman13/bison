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
        protected IReadOnlyCollection<IBehavior> behavior = Array.Empty<IBehavior>();

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
        public T GetBehavior<T>() where T : class, IBehavior
        {
            return behavior.OfType<T>().SingleOrDefault();
        }
        public bool HasBehavior<T>(out T behavior) where T : class, IBehavior
        {
            return (behavior = GetBehavior<T>()) != null;
        }
        public bool HasBehavior<T>() where T : class, IBehavior
        {
            return behavior.OfType<T>().Any();
        }
        protected void SetBehavior(params IBehavior[] behavior)
        {
            if (this.behavior.Any(b => behavior.Any(b2 => b.GetType() == b2.GetType())))
                throw new Exception();
            this.behavior = this.behavior.Concat(behavior).ToList().AsReadOnly();
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
