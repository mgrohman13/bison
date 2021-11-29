using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces.Terrain
{
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
            energy = 1000;
            mass = 750;
        }

        public override void GenerateResources(ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc, ref double researchUpk)
        {
            researchInc += Value;
            massInc += Value / Consts.ArtifactMassIncDiv;
            energyUpk += Value / Consts.ArtifactEnergyUpkDiv;
        }
        public override string GetResourceName()
        {
            return "Research";
        }
    }
}
