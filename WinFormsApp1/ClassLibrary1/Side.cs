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
    class Side : ISide
    {
        private readonly Game _game;
        private readonly List<Piece> _pieces;

        internal Side(Game game)
        {
            this._pieces = new List<Piece>();
            this._game = game;
        }

        void ISide.AddPiece(Piece piece)
        {
            AddPiece(piece);
        }
        internal void AddPiece(Piece piece)
        {
            this._pieces.Add(piece);
        }

        public void EndTurn()
        {
            foreach (Piece piece in Pieces)
                piece.EndTurn();
        }

        public ReadOnlyCollection<Piece> Pieces => _pieces.AsReadOnly();

        public Game Game => _game;
    }
}
