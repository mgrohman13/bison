using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public abstract class PlayerPiece : Piece
    {
        private readonly double _vision;
        public double Vision => _vision;

        internal PlayerPiece(Game game, Map.Tile tile, double vision) : base(game, game.Player, tile)
        {
            this._vision = vision;
        }

        internal override void EndTurn()
        {
        }
    }
}
