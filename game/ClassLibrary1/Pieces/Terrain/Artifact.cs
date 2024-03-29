﻿using System;
using Tile = ClassLibrary1.Map.Tile;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    public class Artifact : Resource
    {
        private Artifact(Tile tile)
            : base(tile, Consts.ArtifactResearchInc, Consts.ArtifactSustain)
        {
        }
        internal static Artifact NewArtifact(Tile tile)
        {
            Artifact artifact = new(tile);
            tile.Map.Game.AddPiece(artifact);
            return artifact;
        }
        internal override void GetCost(double costMult, out int energy, out int mass)
        {
            GetCost(costMult, Consts.ArtifactResearchInc, Consts.ArtifactExtractorEnergyCost, Consts.ArtifactExtractorMassCost, out energy, out mass);
        }

        public override void GenerateResources(Piece piece, double valueMult, ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc)
        {
            double value = Consts.GetDamagedValue(piece, Value * valueMult, 0);
            researchInc += value;
            massInc += value / Consts.ArtifactMassIncDiv;
            energyUpk += value / Consts.ArtifactEnergyUpkDiv;
        }

        public override string GetResourceName()
        {
            return "Research";
        }
        public override string ToString()
        {
            return "Artifact";
        }
    }
}
