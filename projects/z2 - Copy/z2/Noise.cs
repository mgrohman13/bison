using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace z2
{
    public class Noise
    {
        private const double Smooth = 1.3, SmoothDist = 2.6;

        private const double DOUBLE_DIV = 0xFFFFFFFF;
        public static double GetNoise(uint seed, double value, double amp)
        {
            int min = Game.Random.Round(value - SmoothDist);
            int max = Game.Random.Round(value + SmoothDist);
            double retVal = 0, div = 0;
            for (int a = min ; a <= max ; ++a)
            {
                double dist = Math.Abs(a - value);
                dist = 1 / ( dist * dist + Smooth );
                retVal += ShiftVal((uint)a + seed) / DOUBLE_DIV * dist;
                div += dist;
            }
            return retVal / div * amp;
        }

        private static uint ShiftVal(uint value)
        {
            int shift = (int)( value % 124 );
            int neg = shift / 31;
            shift = ( shift % 31 ) + 1;
            return ( value ^ ( ( ( ( neg & 1 ) == 1 ? value : ~value ) << shift ) | ( ( neg > 1 ? value : ~value ) >> ( 32 - shift ) ) ) );
        }
    }
}
