using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces.Terrain
{
    public class Artifacts : Resource
    {
        private Artifacts(Map.Tile tile)
            : base(tile, Consts.ArtifactsResearchInc, Consts.ArtifactsSustain)
        {
        }
        internal Artifacts NewArtifacts(Map.Tile tile)
        {
            Artifacts artifacts = new(tile);
            Game.AddPiece(artifacts);
            return artifacts;
        }

        public override void GenerateResources(ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc, ref double researchUpk)
        {
            researchInc += Value;
            massInc += Value / Consts.ArtifactsMassIncDiv;
            energyUpk += Value / Consts.ArtifactsEnergyUpkDiv;
        }
    }
}
