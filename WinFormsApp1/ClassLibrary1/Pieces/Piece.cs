using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public abstract class Piece
    {
        public readonly Game Game;
        public readonly ISide _side;

        [NonSerialized]
        private Map.Tile _tile;

        public ISide Side => _side;
        public Map.Tile Tile => _tile;

        public bool IsPlayer => Side != null && Side == Game.Player;

        internal Piece(Game game, ISide side, Map.Tile tile)
        {
            this.Game = game;
            this._side = side;
            this._tile = tile;
        }

        internal void SetTile(Map.Tile tile)
        {
            Game.Map.RemovePiece(this);
            this._tile = tile;
            Game.Map.AddPiece(this);
        }

        internal abstract void EndTurn();
    }
}
