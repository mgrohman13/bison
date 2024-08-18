using ClassLibrary1.Pieces.Players;
using MattUtil;
using System;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    public abstract class Resource : Piece
    {
        public readonly double Sustain;

        private readonly double _energyMult, _massMult, _rounding;

        private double _value;
        public double Value => _value;
        public double GetValue(Extractor extractor, double valueMult) => Consts.GetDamagedValue(extractor, this.Value * valueMult, 0);

        internal double Rounding => _rounding;

        internal Resource(Tile tile, double baseValue, double sustainMult, bool limit = false)
            : base(null, tile)
        {
            double distMult = Math.Pow((tile.GetDistance(0, 0) + Consts.ResourceDistAdd) / Consts.ResourceDistDiv, Consts.ResourceDistPow);
            if (limit)
                distMult = Math.Sqrt(distMult);
            double oeDiv = limit ? Math.Sqrt(baseValue) : 1;

            baseValue *= distMult;
            double value = Game.Rand.GaussianOE(baseValue, Consts.ResourceDev, Consts.ResourceOE / oeDiv, 1.3);

            sustainMult *= Math.Pow(baseValue / value, Consts.ResourceSustainValuePow);
            double sustain = Game.Rand.GaussianOE(sustainMult, Consts.ResourceDev, Consts.ResourceOE, .05);
            if (Game.Rand.Bool(.26 * sustain / (sustain + 1)))
            {
                double mult = Game.Rand.Next(sustain > 1 ? 2 : 3) == 0 ? 10 : 20;
                sustain = Game.Rand.Round(sustain * mult) / mult;
                const double offset = .01;
                sustain += Game.Rand.GaussianCapped(offset, 1 / 3.0, offset / 2.0) - offset;
            }

            this._energyMult = Game.Rand.GaussianOE(1, .065, .021);
            this._massMult = Game.Rand.GaussianOE(Math.Sqrt(1 / this._energyMult), .039, .013);
            this._rounding = Game.Rand.NextDouble();
            this._value = value;
            this.Sustain = sustain;
        }

        public void GenerateResources(ref double energyInc, ref double massInc, ref double researchInc) =>
            GenerateResources(Value, ref energyInc, ref massInc, ref researchInc); //not using valueMult - values will increase when building extractor
        internal void GenerateResources(Extractor extractor, double valueMult, ref double energyInc, ref double massInc, ref double researchInc) =>
            GenerateResources(GetValue(extractor, valueMult), ref energyInc, ref massInc, ref researchInc);
        protected abstract void GenerateResources(double value, ref double energyInc, ref double massInc, ref double researchInc);

        internal void Extract(Extractor extractor, double valueMult, double sustainMult)
        {
            sustainMult *= Sustain;
            double pow = Consts.ExtractPow / (Consts.ExtractPow + Math.Pow(sustainMult, Consts.ExtractSustainPow));
            double extract = Math.Pow(GetValue(extractor, valueMult) / sustainMult / Consts.ExtractTurns + 1, pow) - 1;
            double cap = Math.Max(0, 2 * extract - this._value);
            this._value -= Game.Rand.GaussianCapped(extract, .13, cap);
        }

        protected void GetCost(double costMult, double inc, double baseEnergy, double baseMass, out int energy, out int mass)
        {
            double mult = Math.Sqrt(inc);
            mult = Math.Pow((this.Value + mult) / (inc + mult), Consts.ExtractorCostPow);
            mult *= Math.Pow(Sustain, Consts.ExtractorSustainCostPow);
            mult *= costMult;
            energy = MTRandom.Round(baseEnergy * _energyMult * mult, 1 - _rounding);
            mass = MTRandom.Round(baseMass * _massMult * mult, _rounding);
        }

        internal abstract void GetCost(double costMult, out int energy, out int mass);
        public abstract string GetResourceName();
    }
}
