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
    public interface ISide
    {
        public Game Game { get; }
        internal ReadOnlyCollection<Piece> Pieces { get; }

        internal void AddPiece(Piece piece);
        internal void RemovePiece(Piece piece);

        internal void EndTurn();
    }
}
