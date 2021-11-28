using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces.Terrain
{
    public class Metal : Resource
    {
        private Metal(Map.Tile tile)
            : base(tile, Consts.MetalMassInc, Consts.MetalSustain)
        {
        }
        internal Metal NewArtifacts(Map.Tile tile)
        {
            Metal obj = new(tile);
            Game.AddPiece(obj);
            return obj;
        }

        public override void GenerateResources(ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc, ref double researchUpk)
        {
            massInc += Value;
            energyUpk += Value / Consts.MetalEnergyUpkDiv;
        }
    }
}
