using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;

namespace ClassLibrary1
{
    public static class Consts
    {
        public const double MapCoordSize = 16.9;
        public const double MapDev = .13;
        public const int MinMapCoord = 9;

        public const double MoveLimitPow = 1.3;
        public const double ShieldLimitPow = 1.3;
        public const double MoveDev = .013;
        public const double ShielDev = .065;

        public static double IncValueWithMaxLimit(double cur, double inc, double dev, double max, double limit, double pow)
        {
            if (inc > 0)
            {
                double startMax = Math.Max(cur, max);
                cur += Game.Rand.GaussianCapped(inc, dev, dev);

                double extra = cur - startMax;
                if (extra > 0)
                {
                    limit -= startMax;
                    double mult = limit / (limit + max);
                    extra *= Math.Pow(mult, pow);
                    extra += startMax;

                    cur = extra;
                }

                //Debug.WriteLine(cur);
            }
            return cur;
        }
    }
}
