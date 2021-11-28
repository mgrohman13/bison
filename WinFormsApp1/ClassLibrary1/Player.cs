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
    public class Player : ISide
    {
        private readonly ISide side;

        private Core _core;

        internal Player(Game game)
        {
            this.side = new Side(game);
        }
        internal void CreateCore()
        {
            this._core = Core.NewCore(Game);
        }

        #region ISide

        public Game Game => side.Game;
        public Core Core => _core;
        public ReadOnlyCollection<Piece> Pieces => side.Pieces;

        void ISide.AddPiece(Piece piece)
        {
            AddPiece(piece);
        }

        internal void AddPiece(Piece piece)
        {
            side.AddPiece(piece);
        }
        void ISide.RemovePiece(Piece piece)
        {
            RemovePiece(piece);
        }
        internal void RemovePiece(Piece piece)
        {
            side.RemovePiece(piece);
        }

        void ISide.EndTurn()
        {
            EndTurn();
        }
        internal void EndTurn()
        {
            side.EndTurn();
        }

        #endregion ISide
    }
}
