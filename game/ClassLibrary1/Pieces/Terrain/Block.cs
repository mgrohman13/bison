using System;
using System.Runtime.Serialization;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Block(Map.Map.Tile tile, double value) : Piece(null, tile), ITerrain
    {
        public readonly double Value = 4 * (.5 - value);
        public static readonly string FullString = Format(double.PositiveInfinity);
        public override string ToString() => Format(Value);
        private static string Format(double v) => 
            $"NH₃ - {(double.IsPositiveInfinity(v) ? 1 : .01 + .98 * v):P0}";
    }
}
