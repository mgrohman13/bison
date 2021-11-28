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

        public Piece Piece => _piece;

        public Building(Piece piece)
        {
            this._piece = piece;
        }

        public void Build(ISide side, Map.Tile tile, double vision, IKillable.Values killable, List<IAttacker.Values> attacks, IMovable.Values movable)
        {
            Mech.NewMech(side.Game, tile, vision, killable, attacks, movable);
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
