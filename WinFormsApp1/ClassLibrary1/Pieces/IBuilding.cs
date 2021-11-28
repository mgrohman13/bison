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
        public Piece Piece { get; }
        public void Build(ISide side, Map.Tile tile, double vision, IKillable.Values killable, List<IAttacker.Values> attacks, IMovable.Values movable);
        internal void EndTurn();
    }
}
