using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
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
        internal override void GetCost(double costMult, out int energy, out int mass)
        {
            GetCost(costMult, Consts.BiomassEnergyInc, Consts.BiomassExtractorEnergyCost, Consts.BiomassExtractorMassCost, out energy, out mass); 
        }

        public override void GenerateResources(Piece piece, double valueMult, ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc)
        {
            double value = Consts.GetDamagedValue(piece, Value * valueMult, 0);
            energyInc += value;
            researchInc += value / Consts.BiomassResearchIncDiv;
        }
        public override string GetResourceName()
        {
            return "Energy";
        }
        public override string ToString()
        {
            return "Biomass";
        }
    }
}
