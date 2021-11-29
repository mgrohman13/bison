﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces.Terrain
{
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

        public abstract void GenerateResources(ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc, ref double researchUpk);

        internal void Extract()
        {
            double power = Consts.ExtractPower / (Consts.ExtractPower + Math.Pow(Sustain, Consts.ExtractSustainPow));
            double extract = Math.Pow(Value / Sustain / Consts.ExtractTurns + 1, power) - 1;
            this._value -= extract;
        }

        internal override void EndTurn()
        {
        }

        public abstract void GetCost(out double energy, out double mass);
        public abstract string GetResourceName();
    }
}
