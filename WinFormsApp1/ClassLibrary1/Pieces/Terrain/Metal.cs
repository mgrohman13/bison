﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    public class Metal : Resource
    {
        private Metal(Map.Tile tile)
            : base(tile, Consts.MetalMassInc, Consts.MetalSustain)
        {
        }
        internal static Metal NewMetal(Map.Tile tile)
        {
            Metal obj = new(tile);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }
        internal override void GetCost(double costMult, out int energy, out int mass)
        {
            GetCost(costMult, Consts.MetalMassInc, Consts.MetalExtractorEnergyCost, Consts.MetalExtractorMassCost, out energy, out mass); 
        }

        public override void GenerateResources(Piece piece, double valueMult, ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc)
        {
            double value = Consts.GetDamagedValue(piece, Value * valueMult, 0);
            massInc += value;
            energyUpk += value / Consts.MetalEnergyUpkDiv;
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
