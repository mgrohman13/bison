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
        internal static Metal NewMetal(Map.Tile tile)
        {
            Metal obj = new(tile);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }
        public override void GetCost(out double energy, out double mass)
        {
            double mult = CostMult(Consts.MetalMassInc);
            energy = 1000 * mult;
            mass = 250 * mult;
        }

        public override void GenerateResources(ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc, ref double researchUpk)
        {
            massInc += Value;
            energyUpk += Value / Consts.MetalEnergyUpkDiv;
        }
        public override string GetResourceName()
        {
            return "Mass";
        }
    }
}
