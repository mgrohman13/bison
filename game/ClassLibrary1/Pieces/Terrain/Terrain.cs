using System;
using System.Runtime.Serialization;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Terrain(Map.Map.Tile tile) : Piece(null, tile)
    {
    }
}
