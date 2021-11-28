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
    public class Side : ISide
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
        void ISide.RemovePiece(Piece piece)
        {
            RemovePiece(piece);
        }
        internal void RemovePiece(Piece piece)
        {
            this._pieces.Remove(piece);
        }

        void ISide.EndTurn()
        {
            foreach (Piece piece in Pieces)
                piece.EndTurn();
        }

        public ReadOnlyCollection<Piece> Pieces => _pieces.AsReadOnly();

        public Game Game => _game;
    }
}
