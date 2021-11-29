using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces.Terrain
{
    public class Biomass : Resource
    {
        private Biomass(Map.Tile tile)
            : base(tile, Consts.BiomassEnergyInc, Consts.BiomassSustain)
        {
        }
        internal static Biomass NewBiomass(Map.Tile tile)
        {
            Biomass obj = new(tile);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }
        public override void GetCost(out double energy, out double mass)
        {
            energy = 250;
            mass = 250;
        }

        public override void GenerateResources(ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc, ref double researchUpk)
        {
            energyInc += Value;
            researchInc += Value / Consts.BiomassResearchIncDiv;
        }
        public override string GetResourceName()
        {
            return "Energy";
        }
    }
}
