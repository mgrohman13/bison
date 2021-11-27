using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    internal class Piece : IPiece
    {
        public readonly Game Game;
        public readonly ISide _side;

        [NonSerialized]
        public Map.Tile _tile;

        public ISide Side => _side;
        public Map.Tile Tile => _tile;

        public bool IsPlayer => Side != null && Side == Game.Player;

        public Piece(Game game, ISide side, Map.Tile tile)
        {
            this.Game = game;
            this._side = side;
            this._tile = tile;
        }

        void IPiece.SetTile(Map.Tile tile)
        {
            SetTile(tile);
        }
        internal void SetTile(Map.Tile tile)
        {
            Game.Map.RemovePiece(this);
            this._tile = tile;
            Game.Map.AddPiece(this);
        }
        public void EndTurn()
        {
        }
    }
}
