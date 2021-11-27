using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;

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
        public IEnumerable<IPiece> Pieces => side.Pieces;

        void ISide.AddPiece(IPiece piece)
        {
            AddPiece(piece);
        }
        internal void AddPiece(IPiece piece)
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
