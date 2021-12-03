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
        public override void GetCost(out double energy, out double mass)
        {
            double mult = CostMult(Consts.BiomassEnergyInc);
            energy = 500 * mult;
            mass = 500 * mult;
        }

        public override void GenerateResources(Piece piece, ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc, ref double researchUpk)
        {
            double value = Consts.GetDamagedValue(piece, Value, 0);
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
