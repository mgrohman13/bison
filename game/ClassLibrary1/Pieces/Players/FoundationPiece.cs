using ClassLibrary1.Pieces.Terrain;
using System;
using System.Runtime.Serialization;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    [DataContract(IsReference = true)]
    public abstract class FoundationPiece : PlayerPiece
    {
        internal FoundationPiece(Tile tile, double vision)
            : base(tile, vision)
        {
        }

        internal override void Die(out Tile tile, out double treasure)
        {
            base.Die(out tile, out treasure);
            Foundation.NewFoundation(tile);
        }
    }
}
