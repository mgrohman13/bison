using System;
using System.Runtime.Serialization;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Artifact : Resource
    {
        private Artifact(Consts consts, Tile tile, double mult)
            : base(tile, consts.ArtifactResearchInc * mult, consts.ArtifactSustain, true)
        {
        }
        internal static Artifact NewArtifact(Tile tile)
        {
            Consts consts = tile.Map.Game.Consts;
            double caveDist = tile.Map.ClosestCaveDistSqr(tile);
            double factor = consts.CaveSize * consts.CaveSize;
            caveDist = 1 + factor / (caveDist + factor);
            while (caveDist > 1 && Game.Rand.Bool())
                caveDist = Math.Sqrt(caveDist);
            Artifact artifact = new(consts, tile, caveDist);
            tile.Map.Game.AddPiece(artifact);
            return artifact;
        }
        internal override void GetCost(double costMult, out int energy, out int mass)
        {
            GetCost(costMult, Game.Consts.ArtifactResearchInc, Game.Consts.ArtifactExtractorEnergyCost, Game.Consts.ArtifactExtractorMassCost, out energy, out mass);
        }

        protected override void GenerateResources(double value, out double energyInc, out double massInc, out double researchInc)
        {
            energyInc = -value * Game.Consts.ArtifactEnergyUpkMult;
            massInc = value / Game.Consts.ArtifactMassIncDiv;
            researchInc = value;
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
