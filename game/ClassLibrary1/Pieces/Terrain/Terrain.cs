using System;
using System.Runtime.Serialization;
using static ClassLibrary1.Map.Map;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Island(Tile tile, double vision) : Terain(tile)
{
        public const double MAX_VISION = 16.9;

        public const int Defense = 1;
        public readonly double Vision = vision;

        public override string ToString() => "Hill";
    }
    [Serializable]
    [DataContract(IsReference = true)]
    public class Terain(Tile tile) : ITerrain
    {
        private readonly Tile _tile = tile;
        public Tile Tile => _tile;
    }
    public interface ITerrain
    {
        Tile Tile { get; }
    }
}
