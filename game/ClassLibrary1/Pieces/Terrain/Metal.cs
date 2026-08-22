using System;
using System.Runtime.Serialization;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Metal : Resource
    {
        private Metal(Tile tile)
            : base(tile, tile.Map.Game.Consts.MetalMassInc, tile.Map.Game.Consts.MetalSustain)
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
            GetCost(costMult, Game.Consts.MetalMassInc, Game.Consts.MetalExtractorEnergyCost, Game.Consts.MetalExtractorMassCost, out energy, out mass);
        }

        protected override void GenerateResources(double value, out double energyInc, out double massInc, out double researchInc)
        {
            energyInc = -value / Game.Consts.MetalEnergyUpkDiv;
            massInc = value;
            researchInc = 0;
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
