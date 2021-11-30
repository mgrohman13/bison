using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public class Builder : IBuilder
    {
        private readonly Piece _piece;

        public Piece Piece => _piece;

        public Builder(Piece piece)
        {
            this._piece = piece;
        }

        double IBehavior.GetUpkeep()
        {
            return 0;
        }
        void IBehavior.EndTurn()
        {
        }

        private bool Validate(Map.Tile tile)
        {
            return (tile != null && tile.Visible && tile.GetDistance(this.Piece.Tile) == 1);
        }

        public class BuildConstructor : Builder, IBuilder.IBuildConstructor
        {
            public BuildConstructor(Piece piece)
                : base(piece)
            {
            }
            public Constructor Build(Map.Tile tile)
            {
                if (Validate(tile))
                {
                    Constructor.Cost(Piece.Game, out double energy, out double mass);
                    if (Piece.Game.Player.Spend(energy, mass))
                    {
                        return Constructor.NewConstructor(tile);
                    }
                }
                return null;
            }
        }
        public class BuildExtractor : Builder, IBuilder.IBuildExtractor
        {
            public BuildExtractor(Piece piece)
                : base(piece)
            {
            }
            public Extractor Build(Resource resource)
            {
                if (resource != null && Validate(resource.Tile))
                {
                    Extractor.Cost(out double energy, out double mass, resource);
                    if (Piece.Game.Player.Spend(energy, mass))
                    {
                        return Extractor.NewExtractor(resource);
                    }
                }
                return null;
            }
        }
        public class BuildMech : Builder, IBuilder.IBuildMech
        {
            public BuildMech(Piece piece)
                : base(piece)
            {
            }
            public Mech Build(Map.Tile tile, MechBlueprint blueprint)
            {
                if (Validate(tile))
                {
                    Mech.Cost(Piece.Game, out double energy, out double mass, blueprint);
                    if (Piece.Game.Player.Spend(energy, mass))
                    {
                        return Mech.NewMech(tile, blueprint);
                    }
                }
                return null;
            }
        }
    }
}
