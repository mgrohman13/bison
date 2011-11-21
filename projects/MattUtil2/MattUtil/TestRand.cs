using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MattUtil
{
    public class RootRandom2
    {
        const double MULT = 1.00000000000000013;

        double sqrt;
        double max;

        public RootRandom2(double seed)
        {
            sqrt = Math.Sqrt(seed) % 1;
            while (sqrt == 0)
                sqrt = Math.Sqrt(seed * MULT) % 1;

            this.sqrt = sqrt % 1;
            this.max = 1;

            bool val = n();
            while (val == n())
                ;
        }

        public string Next()
        {
            return n() ? "1" : "0";
        }

        private bool n()
        {
            double half = max / 2;
            bool r = ( sqrt > half );
            if (r)
            {
                sqrt -= half;
                max -= half;
            }
            else
            {
                max = half;
            }
            sqrt *= MULT * 2;
            max *= MULT * 2;
            if (sqrt == max)
                max *= MULT;
            return r;
        }
    }
}
