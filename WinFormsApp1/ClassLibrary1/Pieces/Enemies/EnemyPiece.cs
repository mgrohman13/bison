using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces.Enemies
{
    public abstract class EnemyPiece : Piece
    {
        internal EnemyPiece(Map.Tile tile)
            : base(tile.Map.Game.Enemy, tile)
        {
        }
    }
}
