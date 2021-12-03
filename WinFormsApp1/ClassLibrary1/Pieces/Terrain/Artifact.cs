using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    public class Artifact : Resource
    {
        private Artifact(Map.Tile tile)
            : base(tile, Consts.ArtifactResearchInc, Consts.ArtifactSustain)
        {
        }
        internal static Artifact NewArtifact(Map.Tile tile)
        {
            Artifact artifact = new(tile);
            tile.Map.Game.AddPiece(artifact);
            return artifact;
        }
        public override void GetCost(out double energy, out double mass)
        {
            double mult = CostMult(Consts.ArtifactResearchInc);
            energy = 1000 * mult;
            mass = 750 * mult;
        }

        public override void GenerateResources(Piece piece, ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc, ref double researchUpk)
        {
            double value = Consts.GetDamagedValue(piece, Value, 0);
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
