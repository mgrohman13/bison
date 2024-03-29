using System;
using Tile = ClassLibrary1.Map.Tile;


namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    public class Chasm : Terrain
    {
        public Chasm(Tile tile) : base(tile) { }
    }
}
