using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ClassLibrary1.Map
{
    public partial class Map
    {
        [Serializable]
        [DataContract(IsReference = true)]
        private class Elevation(PointD center)
        {
            private readonly PointD _center = center;
            private readonly List<double> _steps = [];
            private readonly double _rounding = Game.Rand.NextDouble();
            private readonly double _fudge = Game.Rand.Gaussian();
            //private readonly double[] _fudge =
            //    [Game.Rand.Gaussian(), Game.Rand.Gaussian(), Game.Rand.Gaussian(), Game.Rand.Gaussian()];

            public static IEnumerable<Elevation> GeneratePlateaus(Consts consts, double curExplore, ref double nextElevation)
            {
                double generationBuffer = 2 * consts.ElevationMaxEffectDist;
                if (Game.TEST_MAP_GEN.HasValue)
                    generationBuffer += Game.TEST_MAP_GEN.Value;
                curExplore += generationBuffer;

                List<Elevation> ret = [];
                while (curExplore > nextElevation)
                {
                    double angle = Game.Rand.NextDouble() * TWO_PI;
                    PointD next = GetPoint(angle, nextElevation);
                    nextElevation += Game.Rand.OE(consts.ElevationDensity / (nextElevation / consts.ElevationDensity + 1));

                    ret.Add(new Elevation(next));
                }
                return ret;
            }

            public double Dist(Point p, Tuple<float, float> evaluate)
            {
                double h = Math.Pow(2 * evaluate.Item1, .65) - 1;
                h *= 16.9;
                return Tile.GetDistanceD(_center.X, _center.Y, p.X, p.Y) + _fudge + h;
            }
            public static double Evaluate(Consts consts, double dist) =>
                1.75 * consts.ElevationMaxEffectDist / (2 * Math.Min(dist + 1, consts.ElevationMaxEffectDist) + consts.ElevationMaxEffectDist);
            public double Round(Consts consts, double height, IEnumerable<Elevation> all)
            {
                List<double> adjacent = null;

                double max = _steps.Count > 0 ? _steps[^1] : 0;
                while (height > max)
                {
                    //TODO: Consts
                    double next = 1 + Game.Rand.Weighted(.91) + Game.Rand.OE(.39) + Game.Rand.GaussianCapped(.78, .65); //3.08
                    max += next;

                    adjacent ??= [.. all.Where(e => e != this && Tile.GetDistanceD(e._center, this._center) < consts.ElevationMaxEffectDist * 2)
                        .SelectMany(e => e._steps)];
                    var match = adjacent.Where(s => Math.Abs(s - max) < Game.Rand.GaussianCapped(1.3, .13)).ToArray();
                    if (!match.Any())
                        match = all.SelectMany(e => e._steps).Where(s => Math.Abs(s - max) < Game.Rand.GaussianCapped(.169, .052, .104)).ToArray();
                    if (match.Any())
                        max = Game.Rand.SelectValue(match);

                    _steps.Add(max);
                }
                for (int a = 0; a < _steps.Count; a++)
                {
                    double low = a == 0 ? 0 : _steps[a - 1];
                    double high = _steps[a];
                    if (height >= low && height <= high)
                    {
                        double value = (height - low) / (high - low);
                        if (_rounding < value)
                            return high;
                        return low;
                    }
                }

                throw new Exception();
            }
        }
    }
}
