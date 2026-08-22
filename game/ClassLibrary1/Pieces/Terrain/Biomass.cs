using System;
using System.Runtime.Serialization;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Biomass : Resource
    {
        private Biomass(Tile tile)
            : base(tile, tile.Map.Game.Consts.BiomassEnergyInc, tile.Map.Game.Consts.BiomassSustain)
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
            GetCost(costMult,Game. Consts.BiomassEnergyInc, Game.Consts.BiomassExtractorEnergyCost, Game.Consts.BiomassExtractorMassCost, out energy, out mass);
        }

        protected override void GenerateResources(double value, out double energyInc, out double massInc, out double researchInc)
        {
            energyInc = value;
            massInc = 0;
            researchInc = Math.Pow(value / Game.Consts.BiomassResearchDiv + 1, Game.Consts.BiomassResearchPow) - 1;
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
