using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
namespace ClassLibrary1
{
    [Serializable]
    class Side : ISide
    {
        private readonly Game _game;
        private readonly List<IPiece> _pieces;

        internal Side(Game game)
        {
            this._pieces = new List<IPiece>();
            this._game = game;
        }

        void ISide.AddPiece(IPiece piece)
        {
            AddPiece(piece);
        }
        internal void AddPiece(IPiece piece)
        {
            this._pieces.Add(piece);
        }

        public void EndTurn()
        {
            foreach (IPiece piece in Pieces)
                piece.EndTurn();
        }

        public IEnumerable<IPiece> Pieces => _pieces.AsReadOnly();

        public Game Game => _game;
    }
}
