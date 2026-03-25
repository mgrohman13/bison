using System;
using System.Runtime.Serialization;
using Tile = ClassLibrary1.Map.Map.Tile;


namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Block(Map.Map.Tile tile, double value) : Terrain(tile)
    {
        public readonly double Value = value;

        public override string ToString()
        {
            return $"NH₃ - {4 * (.5 - Value):P0}";
        }
    }
}
