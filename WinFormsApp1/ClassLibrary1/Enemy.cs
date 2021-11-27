using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;

namespace ClassLibrary1
{
    [Serializable]
    public class Enemy : ISide
    {
        private readonly ISide side;

        internal Enemy(Game game)
        {
            this.side = new Side(game);
        }

        #region ISide

        public Game Game => side.Game;
        public ReadOnlyCollection<Piece> Pieces => side.Pieces;

        void ISide.AddPiece(Piece piece)
        {
            AddPiece(piece);
        }
        internal void AddPiece(Piece piece)
        {
            side.AddPiece(piece);
        }

        public void EndTurn()
        {
            side.EndTurn();
        }

        #endregion ISide
    }
}
