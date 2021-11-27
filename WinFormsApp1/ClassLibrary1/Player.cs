using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;

namespace ClassLibrary1
{
    [Serializable]
    public class Player : ISide
    {
        private readonly ISide side;

        internal Player(Game game)
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

            if (Pieces.Count() == 1)
            {
                Core core = Pieces.OfType<Core>().First();

                Map.Tile tile = null;
                do
                {
                    tile = Game.Map.GetTile(core.Tile.X + Game.Rand.GaussianInt(3), core.Tile.Y + Game.Rand.GaussianInt(3));
                } while (tile == null || tile.Piece != null);

                double move = Game.Rand.GaussianOE(2.6, .13, .13, 1);
                double max = Game.Rand.GaussianOE(move * 2, .13, .13, move);
                double limit = Game.Rand.GaussianOE(max + move, .13, .13, max);
                core.Build(this, tile, Game.Rand.GaussianOE(6.5, .13, .13, 1), move, max, limit);
            }
        }

        #endregion ISide
    }
}
