using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Terrain;

namespace ClassLibrary1.Pieces.Players
{
    public abstract class FoundationPiece : PlayerPiece
    {
        internal FoundationPiece(Map.Tile tile, double vision)
            : base(tile, vision)
        {
        }

        internal override void Die()
        {
            Map.Tile tile = this.Tile;
            base.Die();
            Foundation.NewFoundation(tile);
        }
    }
}