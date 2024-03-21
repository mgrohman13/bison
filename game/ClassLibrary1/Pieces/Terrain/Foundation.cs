using System;
using Tile = ClassLibrary1.Map.Tile;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    public class Foundation : Piece
    {
        internal Foundation(Tile tile)
            : base(null, tile)
        {
        }
        internal static Foundation NewFoundation(Tile tile)
        {
            Foundation obj = new(tile);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }

        public override string ToString()
        {
            return "Foundation";
        }
    }
}
