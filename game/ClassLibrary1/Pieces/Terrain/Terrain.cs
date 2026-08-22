using System;
using System.Runtime.Serialization;
using static ClassLibrary1.Map.Map;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Island(Tile tile, double height) : Terain(tile)
    {
        //TODO: Consts (internal hidden multiplier not used for painting)
        public const double HEIGHT = 16.9; 

        //public const int RangedAtt = 1;
        public readonly double Height = height;

        public override string ToString() => "Plateau";
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
        //dou Tile { get; }
    }
}
