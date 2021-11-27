using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces; 

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Mech : PlayerPiece, IMovable
    {
        private readonly IMovable movable;

        private Mech(Player player, Map.Tile tile, double vision, double moveInc, double moveMax, double moveLimit) : base(player.Game, tile, vision)
        {
            movable = new Movable(this, moveInc, moveMax, moveLimit);
        }
        internal static Mech NewMech(Player player, Map.Tile tile, double vision, double moveInc, double moveMax, double moveLimit)
        {
            Mech obj = new(player, tile, vision, moveInc, moveMax, moveLimit);
            player.Game.AddPiece(obj);
            return obj;
        }
        void IMovable.EndTurn()
        {
            EndTurn();
        }
        internal override void EndTurn()
        {
            base.EndTurn();
            movable.EndTurn();
        }

        #region IMovable

        public double MoveCur => movable.MoveCur;
        public double MoveInc => movable.MoveInc;
        public double MoveMax => movable.MoveMax;
        public double MoveLimit => movable.MoveLimit;

        public bool Move(Map.Tile to)
        {
            return movable.Move(to);
        }

        #endregion IMovable
    }
}
