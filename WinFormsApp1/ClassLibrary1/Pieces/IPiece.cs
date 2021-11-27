using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;

namespace ClassLibrary1.Pieces
{
    public interface IPiece
    {
        public ISide Side { get; }
        public Map.Tile Tile { get; }
        bool IsPlayer { get; }

        internal void SetTile(Map.Tile tile);

        public void EndTurn();
    }
}
