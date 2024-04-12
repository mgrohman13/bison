using System;
using Tile = ClassLibrary1.Map.Map.Tile;

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

        protected override void GenerateResources(double value, ref double energyInc, ref double massInc, ref double researchInc)
        {
            researchInc += value;
            massInc += value / Consts.ArtifactMassIncDiv;
            energyInc -= value * Consts.ArtifactEnergyUpkMult;
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
