using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    internal class PlayerPiece : Piece, IPlayerPiece
    {
        private double _vision;
        public double Vision => _vision;

        internal PlayerPiece(Game game, Map.Tile tile, double vision) : base(game, game.Player, tile)
        {
            this._vision = vision;
        }
    }
}
