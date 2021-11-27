using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;

namespace ClassLibrary1.Pieces
{
    public interface IMovable
    {
        public double MoveCur { get; }
        public double MoveInc { get; }
        public double MoveMax { get; }
        public double MoveLimit { get; }

        public bool Move(Map.Tile to);
        public void EndTurn();
    }
}
