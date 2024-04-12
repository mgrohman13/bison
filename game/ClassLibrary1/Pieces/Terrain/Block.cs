using System;
using Tile = ClassLibrary1.Map.Map.Tile;


namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    public class Block : Terrain
    {
        public readonly double Value;
        public Block(Tile tile, double value) : base(tile)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return $"NH₃ - {4 * (.5 - Value):P0}";
        }
    }
}
