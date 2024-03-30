using System;
using Tile = ClassLibrary1.Map.Tile;

namespace ClassLibrary1.Pieces.Enemies
{
    [Serializable]
    public abstract class EnemyPiece : Piece
    {
        internal EnemyPiece(Tile tile)
            : base(tile.Map.Game.Enemy, tile)
        {
        }

        //internal override void Die()
        //{
        //    Game.CountKill();
        //}
    }    
}
