using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;

namespace ClassLibrary1
{
    public interface ISide
    {
        public Game Game { get; }
        public IEnumerable<IPiece> Pieces { get; }

        internal void AddPiece(IPiece piece);

        public void EndTurn();
    }
}
