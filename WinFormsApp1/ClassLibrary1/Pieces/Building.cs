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
    [Serializable]
    public class Building : IBuilding
    {
        private readonly Piece _piece;

        public Building(Piece piece)
        {
            this._piece = piece;
        }

        public void Build(Player player, Map.Tile tile, double vision, double moveInc, double moveMax, double moveLimit)
        {
            Mech.NewMech(player, tile, vision, moveInc, moveMax, moveLimit);
        }

        void IBuilding.EndTurn()
        {
            EndTurn();
        }
        internal void EndTurn()
        {
        }
    }
}
