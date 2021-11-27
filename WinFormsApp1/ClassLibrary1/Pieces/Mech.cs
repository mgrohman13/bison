using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public class Mech : IPlayerPiece, IMovable
    {
        private readonly IPlayerPiece piece;
        private readonly IMovable movable;

        private Mech(Player player, Map.Tile tile, double vision, double moveInc, double moveMax, double moveLimit)
        {
            piece = new PlayerPiece(player.Game, tile, vision);
            movable = new Movable(this, moveInc, moveMax, moveLimit);
        }
        internal static Mech NewMech(Player player, Map.Tile tile, double vision, double moveInc, double moveMax, double moveLimit)
        {
            Mech obj = new Mech(player, tile, vision, moveInc, moveMax, moveLimit);
            player.Game.Map.AddPiece(obj);
            return obj;
        }

        public void EndTurn()
        {
            piece.EndTurn();
            movable.EndTurn();
        }

        #region IPlayerPiece

        public ISide Side => piece.Side;
        public Map.Tile Tile => piece.Tile;
        public double Vision => piece.Vision;

        public bool IsPlayer => piece.IsPlayer;

        void IPiece.SetTile(Map.Tile tile)
        {
            SetTile(tile);
        }
        internal void SetTile(Map.Tile tile)
        {
            piece.SetTile(tile);
        }

        #endregion IPlayerPiece

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
