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
    public interface IBuilding
    {
        public void Build(Player player, Map.Tile tile, double vision, double moveInc, double moveMax, double moveLimit);
        internal void EndTurn();
    }
}
