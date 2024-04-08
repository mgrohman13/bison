using System;
using Tile = ClassLibrary1.Map.Tile;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    public class Biomass : Resource
    {
        private Biomass(Tile tile)
            : base(tile, Consts.BiomassEnergyInc, Consts.BiomassSustain)
        {
        }
        internal static Biomass NewBiomass(Tile tile)
        {
            Biomass obj = new(tile);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }
        internal override void GetCost(double costMult, out int energy, out int mass)
        {
            GetCost(costMult, Consts.BiomassEnergyInc, Consts.BiomassExtractorEnergyCost, Consts.BiomassExtractorMassCost, out energy, out mass);
        }

        protected override void GenerateResources(double value, ref double energyInc, ref double massInc, ref double researchInc)
        {
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
