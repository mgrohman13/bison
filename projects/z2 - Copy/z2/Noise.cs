using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MattUtil;

namespace z2
{
    public class Noise
    {
        private readonly uint s1, s2;

        public Noise()
        {
            this.s1 = Game.Random.NextUInt();
            this.s2 = Game.Random.NextUInt();
        }

        public double GetNoise(double value, uint seed)
        {
            uint roundVal = Combine(value, seed);
            int min = MTRandom.Round(value - Consts.NoiseSmoothDist, NextDouble(roundVal, s1));
            int max = MTRandom.Round(value + Consts.NoiseSmoothDist, NextDouble(roundVal, s2));

            double retVal = 0, div = 0;

            for (int valInt = min ; valInt <= max ; ++valInt)
            {
                double dist = valInt - value;
                dist = 1 / ( dist * dist + Consts.NoiseSmooth );

                retVal += DoubleHalf((uint)valInt, seed) * dist;
                div += dist;
            }

            return retVal / div;
        }

        public static uint Combine(double value, uint seed)
        {
            ulong valULong = (ulong)BitConverter.DoubleToInt64Bits(value);
            return ( seed + (uint)valULong + (uint)( valULong >> 32 ) );
        }

        private double NextDouble(uint value, uint seed)
        {
            return GetDouble(value, seed, 0, 0x100000000);
        }
        private double DoubleHalf(uint value, uint seed)
        {
            return GetDouble(value, seed, 1, 0x100000001);
        }

        private double GetDouble(uint value, uint seed, double add, double div)
        {
            uint ret1, ret2, temp;
            if (CheckBit(value + seed, 1))
            {
                ret1 = LCG(value, ref seed);
                seed = MTRandom.ShiftVal(ret1, seed);
                ret2 = LFSR(value, ref seed);
            }
            else
            {
                ret1 = LFSR(value, ref seed);
                seed = MTRandom.ShiftVal(ret1, seed);
                ret2 = LCG(value, ref seed);
            }
            if (CheckBit(value + seed, 2))
            {
                temp = ret1;
                ret1 = ret2;
                ret2 = temp;
            }

            temp = s1 + ret1 + ret2;
            seed += s2;
            ret1 = MTRandom.ShiftVal(ret1, seed);
            ret2 = MTRandom.ShiftVal(ret2, seed);
            temp = MTRandom.ShiftVal(MTRandom.MWCs(ref ret1, ref ret2), temp);
            return ( temp + add ) / div;
        }
        private static bool CheckBit(uint value, uint bit)
        {
            return ( ( value & bit ) == bit );
        }

        private static uint LCG(uint value, ref uint seed)
        {
            return MTRandom.LCG(seed = MTRandom.ShiftVal(value, seed));
        }
        private static uint LFSR(uint value, ref uint seed)
        {
            return MTRandom.LFSR(seed = MTRandom.ShiftVal(value, seed));
        }
    }
}
