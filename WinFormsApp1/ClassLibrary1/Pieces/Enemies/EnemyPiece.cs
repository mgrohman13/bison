using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces.Enemies
{
    public abstract class EnemyPiece : Piece
    {
        internal EnemyPiece(Game game, Map.Tile tile) : base(game, game.Enemy, tile)
        {
        }

        internal override void EndTurn()
        {
        }
    }
}
