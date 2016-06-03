using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MattUtil
{
    public class RandBooleans
    {
        private static MTRandom random = null;
        public static MTRandom Random
        {
            get
            {
                if (random == null)
                {
                    random = new MTRandom();
                    random.StartTick();
                }
                return random;
            }
            set
            {
                random = value;
            }
        }

        private static double gaussianDeviation = .45;// >=0
        private static double geometricPercent = .2;  // [0,1]
        public static double GaussianDeviation
        {
            get
            {
                return gaussianDeviation;
            }
            set
            {
                if (value < 0)
                    throw new Exception();
                gaussianDeviation = value;
            }
        }
        public static double GeometricPercent
        {
            get
            {
                return geometricPercent;
            }
            set
            {
                if (value < 0 || value > 1)
                    throw new Exception();
                geometricPercent = value;
            }
        }

        private double chance;//0-1
        public double Chance
        {
            get
            {
                return chance;
            }
            set
            {
                double min = 1.0 / ( 1L << ( MTRandom.DOUBLE_BITS + 3 ) );
                if (value < min || value > ( 1 - min ))
                    throw new Exception();

                if (value == 0 || value == 1)
                {
                    chance = value;
                }
                else if (chance == 0 || chance == 1)
                {
                    chance = value;
                    Reset();
                }
                else
                {
                    Console.WriteLine();

                    double mult = ( time + 1 ) / ( GetStartTime(chance) + 1 );
                    Console.WriteLine(mult);
                    if (Direction() != Direction(value))
                        mult = 1 / mult;
                    mult = Math.Sqrt(mult);
                    mult = Math.Max(.5, Math.Min(mult, 2));
                    Console.WriteLine(mult);
                    DoReset(GetStartTime(value) * mult);
                    chance = value;

                    Console.WriteLine();
                }
            }
        }

        private long time;

        public RandBooleans(double chance)
            : this(random, chance)
        {
        }
        public RandBooleans(MTRandom random, double chance)
        {
            Random = random;
            this.chance = chance;
            Reset();
        }

        public bool GetResult()
        {
            if (chance == 0)
                return false;
            if (chance == 1)
                return true;

            bool result = ( --time < 0 );
            if (result)
                Reset(1);
            return ( result != Direction() );
        }

        private bool Direction()
        {
            return Direction(chance);
        }
        private static bool Direction(double chance)
        {
            return ( chance > 0.5 );
        }

        private void Reset()
        {
            Reset(0.5);
        }
        private void Reset(double mult)
        {
            DoReset(GetTime(chance, mult));
        }
        private void DoReset(double time)
        {
            this.time = Random.GaussianOEInt(time, gaussianDeviation, geometricPercent);
        }

        private static double GetStartTime(double chance)
        {
            return GetTime(chance, 0.5);
        }
        private static double GetTime(double chance, double mult)
        {
            if (Direction(chance))
                chance = 1 - chance;
            return mult * ( 1 / chance - 1 );
        }
    }
}
