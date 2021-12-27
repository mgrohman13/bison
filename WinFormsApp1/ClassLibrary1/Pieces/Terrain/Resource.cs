using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MattUtil;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    public abstract class Resource : Piece
    {
        public readonly double Sustain;

        private readonly double _energyMult, _massMult, _rounding;

        private double _value;
        public double Value => _value;

        internal double Rounding => _rounding;

        internal Resource(Map.Tile tile, double baseValue, double sustainMult)
            : base(null, tile)
        {
            double value = Game.Rand.GaussianOE(baseValue, Consts.ResourceDev, Consts.ResourceOE);
            double sustain = Game.Rand.GaussianOE(sustainMult, Consts.ResourceDev, Consts.ResourceOE);
            sustain /= Math.Pow(value / baseValue, Consts.ResourceSustainValuePow);
            double distance = Math.Pow((tile.GetDistance(0, 0) + Consts.ResourceDistAdd) / Consts.ResourceDistDiv, Consts.ResourceDistPow);
            value *= distance;

            this._energyMult = Game.Rand.GaussianOE(1, .065, .021);
            this._massMult = Game.Rand.GaussianOE(1, .039, .013);
            this._rounding = Game.Rand.NextDouble();
            this._value = value;
            this.Sustain = sustain;
        }

        public abstract void GenerateResources(Piece piece, double valueMult, ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc);

        internal void Extract(Piece piece, double sustainMult)
        {
            sustainMult *= Sustain;
            double power = Consts.ExtractPower / (Consts.ExtractPower + Math.Pow(sustainMult, Consts.ExtractSustainPow));
            double extract = Math.Pow(Consts.GetDamagedValue(piece, Value, 0) / sustainMult / Consts.ExtractTurns + 1, power) - 1;
            this._value -= extract;
        }

        protected void GetCost(double costMult, double inc, double baseEnergy, double baseMass, out int energy, out int mass)
        {
            double mult = Math.Sqrt(inc);
            mult = Math.Pow((this.Value + mult) / (inc + mult), .91);
            mult *= Math.Pow(Sustain, .26);
            mult *= costMult;
            energy = MTRandom.Round(baseEnergy * _energyMult * mult, _rounding);
            mass = MTRandom.Round(baseMass * _massMult * mult, _rounding);
        }

        internal abstract void GetCost(double costMult, out int energy, out int mass);
        public abstract string GetResourceName();
    }
}
