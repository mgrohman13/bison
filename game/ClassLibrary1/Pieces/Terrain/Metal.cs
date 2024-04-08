using System;
using Tile = ClassLibrary1.Map.Tile;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    public class Metal : Resource
    {
        private Metal(Tile tile)
            : base(tile, Consts.MetalMassInc, Consts.MetalSustain)
        {
        }
        internal static Metal NewMetal(Tile tile)
        {
            Metal obj = new(tile);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }
        internal override void GetCost(double costMult, out int energy, out int mass)
        {
            GetCost(costMult, Consts.MetalMassInc, Consts.MetalExtractorEnergyCost, Consts.MetalExtractorMassCost, out energy, out mass);
        }

        protected override void GenerateResources(double value, ref double energyInc, ref double massInc, ref double researchInc)
        {
            massInc += value;
            energyInc -= value / Consts.MetalEnergyUpkDiv;
        }
        public override string GetResourceName()
        {
            return "Mass";
        }
        public override string ToString()
        {
            return "Metal";
        }
    }
}
