using game2.map;

namespace game2.pieces.enemy
{
    internal class EnemyPiece : Piece
    {
        internal EnemyPiece(Tile tile) : base(tile.Map.Game.Enemy, tile) { }
    }
}
