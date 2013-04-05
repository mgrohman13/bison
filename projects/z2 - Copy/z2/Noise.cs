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
            int min = MTRandom.Round(value - Consts.NoiseSmoothDist, GetDouble(roundVal, s1));
            int max = MTRandom.Round(value + Consts.NoiseSmoothDist, GetDouble(roundVal, s2));

            double retVal = 0, div = 0;

            for (int valInt = min ; valInt <= max ; ++valInt)
            {
                double dist = Math.Abs(valInt - value);
                dist = 1 / ( dist * dist + Consts.NoiseSmooth );

                retVal += GetDoubleFull((uint)valInt, seed) * dist;
                div += dist;
            }

            return retVal / div;
        }

        private static uint Combine(double value, uint seed)
        {
            ulong valULong = (ulong)BitConverter.DoubleToInt64Bits(value);
            return ( seed + (uint)valULong + (uint)( valULong >> 32 ) );
        }

        private static double GetDouble(uint value, uint seed)
        {
            return GetDouble(value, seed, 0x100000000);
        }
        private static double GetDoubleFull(uint value, uint seed)
        {
            return GetDouble(value, seed, 0xFFFFFFFF);
        }

        private static double GetDouble(uint value, uint seed, double div)
        {
            uint ret1, ret2;
            if (CheckBit(value + seed, 1))
            {
                ret1 = LCG(value, ref seed);
                ret2 = LFSR(value, ref seed);
            }
            else
            {
                ret1 = LFSR(value, ref seed);
                ret2 = LCG(value, ref seed);
            }
            if (CheckBit(value + seed, 2))
            {
                uint temp = ret1;
                ret1 = ret2;
                ret2 = temp;
            }
            ret1 = MTRandom.ShiftVal(ret1, seed);
            ret2 = MTRandom.ShiftVal(ret2, seed);
            seed = ret1 + ret2;
            return MTRandom.ShiftVal(MTRandom.MWCs(ref ret1, ref ret2), seed) / div;
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
