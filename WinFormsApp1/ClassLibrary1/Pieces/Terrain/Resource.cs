using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    public abstract class Resource : Piece
    {
        public readonly double Sustain;

        private double _value;
        public double Value => _value;

        internal Resource(Map.Tile tile, double baseValue, double sustainMult)
            : base(null, tile)
        {
            double value = Game.Rand.GaussianOE(baseValue, Consts.ResourceDev, Consts.ResourceOE);
            double sustain = Game.Rand.GaussianOE(sustainMult, Consts.ResourceDev, Consts.ResourceOE);
            sustain /= Math.Pow(value / baseValue, Consts.ResourceSustainValuePow);
            double distance = Math.Pow((tile.GetDistance(0, 0) + Consts.ResourceDistAdd) / Consts.ResourceDistDiv, Consts.ResourceDistPow);
            value *= distance;

            this._value = value;
            this.Sustain = sustain;
        }

        public abstract void GenerateResources(Piece piece, ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc, ref double researchUpk);

        internal void Extract(Piece piece)
        {
            double power = Consts.ExtractPower / (Consts.ExtractPower + Math.Pow(Sustain, Consts.ExtractSustainPow));
            double extract = Math.Pow(Consts.GetDamagedValue(piece, Value, 0) / Sustain / Consts.ExtractTurns + 1, power) - 1;
            this._value -= extract;
        }

        internal override void EndTurn()
        {
        }

        protected double CostMult(double baseValue)
        {
            double min = Math.Sqrt(baseValue);
            return Math.Pow((this.Value + min) / (baseValue + min), .91);
        }

        public abstract void GetCost(out double energy, out double mass);
        public abstract string GetResourceName();
    }
}
