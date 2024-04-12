using System;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    public class Terrain : Piece
    {
        public Terrain(Tile tile) : base(null, tile) { }
    }
}
