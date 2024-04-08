using ClassLibrary1.Pieces.Players;
using MattUtil;
using System;
using Tile = ClassLibrary1.Map.Tile;

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

        internal Resource(Tile tile, double baseValue, double sustainMult)
            : base(null, tile)
        {
            baseValue *= Math.Pow((tile.GetDistance(0, 0) + Consts.ResourceDistAdd) / Consts.ResourceDistDiv, Consts.ResourceDistPow);
            double value = Game.Rand.GaussianOE(baseValue, Consts.ResourceDev, Consts.ResourceOE, 1.3);

            sustainMult *= Math.Pow(baseValue / value, Consts.ResourceSustainValuePow);
            double sustain = Game.Rand.GaussianOE(sustainMult, Consts.ResourceDev, Consts.ResourceOE, .039);

            this._energyMult = Game.Rand.GaussianOE(1, .065, .021);
            this._massMult = Game.Rand.GaussianOE(1, .039, .013);
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
            this._value -= extract;
        }

        protected void GetCost(double costMult, double inc, double baseEnergy, double baseMass, out int energy, out int mass)
        {
            double mult = Math.Sqrt(inc);
            mult = Math.Pow((this.Value + mult) / (inc + mult), .91);
            mult *= Math.Pow(Sustain, .26);//.65? Consts
            mult *= costMult;
            energy = MTRandom.Round(baseEnergy * _energyMult * mult, _rounding);
            mass = MTRandom.Round(baseMass * _massMult * mult, _rounding);
        }

        internal abstract void GetCost(double costMult, out int energy, out int mass);
        public abstract string GetResourceName();
    }
}
