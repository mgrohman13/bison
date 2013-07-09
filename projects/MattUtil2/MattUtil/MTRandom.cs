using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

namespace MattUtil
{
    public class MTRandom : IDisposable
    {

        #region constructors

        /// <summary>
        /// Instantiates with a default, time-based, random seed.
        /// </summary>
        public MTRandom()
        {
            SetSeed();
        }

        /// <summary>
        /// Instantiates with a default, time-based, random seed.
        /// </summary>
        public MTRandom(bool storeSeed)
        {
            SetSeed(storeSeed);
        }

        /// <summary>
        /// Instantiates with a default, time-based, random seed with the specified size of unsigned integers.
        /// </summary>
        public MTRandom(ushort seedSize)
        {
            SetSeed(seedSize);
        }

        /// <summary>
        /// Instantiates with a default, time-based, random seed with the specified size of unsigned integers.
        /// 'storeSeed' of true will store the initial seed for future retreival.
        /// </summary>
        public MTRandom(bool storeSeed, ushort seedSize)
        {
            SetSeed(storeSeed, seedSize);
        }

        /// <summary>
        /// Instantiates with a specified seed.
        /// </summary>
        public MTRandom(uint[] seed)
        {
            SetSeed(seed);
        }

        /// <summary>
        /// Instantiates with a specified seed.  'storeSeed' of true will store the initial seed for future retreival.
        /// </summary>
        public MTRandom(bool storeSeed, uint[] seed)
        {
            SetSeed(storeSeed, seed);
        }

        #endregion

        #region constants

        static MTRandom()
        {
            MTRandom.watch = new Stopwatch();
            watch.Start();

            //find 32-bit signed integer limits for the Geometric distribution with current float/double conversion implementation
            MTRandom.OE_INT_LIMIT = (int)Math.Floor(int.MaxValue / GetOEMax());
            MTRandom.OE_INT_FLOAT_LIMIT = (int)Math.Floor(int.MaxValue / GetOEFlaotMax()) - 4; //fudge factor

            //find the maximum value for the Gaussian distribution with current float/double conversion implementation
            MTRandom.GAUSSIAN_MAX = GetGaussianMax(DOUBLE_DIV);
            MTRandom.GAUSSIAN_FLOAT_MAX = (float)GetGaussianMax(FLOAT_DIV);
        }
        private static double GetGaussianMax(double div)
        {
            double a = ( div - 1 ) / 2 / div;
            double b = a;
            double c = GetC(ref a, ref b);
            return Math.Abs(a * DoGaussian(c));
        }

        //the maximum number of seed values that can be incorporated into the generator's initial state
        public const ushort MAX_SEED_SIZE = LENGTH + 3;

        //constants for float generation and conversion
        public const byte FLOAT_BITS = 24;
        private const float FLOAT_DIV = 0x0FFFFFF;
        private const float FLOAT_DIV_1 = 0x1000000;

        //constants for double generation and conversion
        public const byte DOUBLE_BITS = 53;
        private const double DOUBLE_DIV = 0x1FFFFFFFFFFFFF;
        private const double DOUBLE_DIV_1 = 0x20000000000000;

        private static readonly int OE_INT_LIMIT;           // 58,455,924 (36.7368005696771)
        private static readonly int OE_INT_FLOAT_LIMIT;     //129,090,164 (16.6355324      )
        public static readonly double GAUSSIAN_MAX;         //8.531146110505567
        public static readonly float GAUSSIAN_FLOAT_MAX;    //5.707707

        private static readonly double LN_2 = Math.Log(2);

        //constants for the Mersenne Twister
        private const int LENGTH = 624;
        private const int STEP = 397;

        private const uint UPPER_MASK = 0x80000000;     //10000000000000000000000000000000
        private const uint LOWER_MASK = 0x7fffffff;     //01111111111111111111111111111111

        private static readonly uint[]
                ODD_FACTOR = { 0, 0x9908B0DF };         //10011001000010001011000011011111

        private const int TEMPER_1 = 11;
        private const int TEMPER_2 = 7;
        private const uint TEMPER_MASK_2 = 0x9D2C5680;  //10011101001011000101011010000000
        private const int TEMPER_3 = 15;
        private const uint TEMPER_MASK_3 = 0xEFC60000;  //11101111110001100000000000000000
        private const int TEMPER_4 = 18;

        //constants for Marsaglia's KISS
        private const uint LCG_MULTIPLIER = 0x00010DCD; //00000000000000010000110111001101
        private const uint LCG_INCREMENT = 0x4F1BBCDD;  //01001111000110111011110011011101

        private const int LFSR_1 = 13;
        private const int LFSR_2 = 17;
        private const int LFSR_3 = 5;

        private const uint MWC_1_MULT = 0x4650;         //00000000000000000100011001010000
        private const uint MWC_2_MULT = 0x78B7;         //00000000000000000111100010110111
        private const uint MWC_MASK = 0xFFFF;           //00000000000000001111111111111111
        private const int MWC_SHIFT = 16;

        //seeding constants
        private const uint INIT_SEED = 0x012BD6AA;      //00000001001010111101011010101010
        private const uint SEED_FACTOR_1 = 0x6C078965;  //01101100000001111000100101100101
        private const uint SEED_FACTOR_2 = 0x0019660D;  //00000000000110010110011000001101
        private const uint SEED_FACTOR_3 = 0x5D588B65;  //01011101010110001000101101100101

        private const uint LCGN_SEED = 0x075BCD15;       //00000111010110111100110100010101
        private const uint LFSR_SEED = 0x159A55E5;      //00010101100110100101010111100101
        private const uint MWC1_SEED = 0x1F123BB5;     //00011111000100100011101110110101
        private const uint MWC2_SEED = 0x369BF75D;     //00110110100110111111011101011101

        private const uint SHIFT_FACTOR = 0x816B8DF8;   //10000001011010111000110111111000

        #endregion

        #region fields

        //used for shifting time values into seeds
        private static uint counter = 0xF2154EE4;       //11110010000101010100111011100100

        //optional ticker thread to independently permutate the algorithms
        private Thread thread = null;
        private static Stopwatch watch;

        //stuff for storing the initial seed
        private uint[] seedVals = null;
        //default storeSeed to false to save memory
        private bool storeSeed = false;

        // MT state
        private uint[] m;
        private ushort t;

        // KISS state
        private uint lcgn;
        private uint lfsr;
        private uint mwc1;
        private uint mwc2;

        //for storing extra bits when not all are used
        private byte bitCount;
        private uint bits;

        //store the extra gaussian generated by the Marsaglia polar method
        private double gaussian;
        private float gaussianFloat;

        #endregion

        #region properties

        /// <summary>
        /// Gets or set whether or not the object will store the initial seed for future retreival.
        /// Setting to false will immediately dispose the seed (if already stored) and free up memory.
        /// Defaults to false if not specified in the constructor.
        /// </summary>
        public bool StoreSeed
        {
            get
            {
                return storeSeed;
            }
            set
            {
                storeSeed = value;
                if (!value)
                    seedVals = null;
            }
        }

        /// <summary>
        /// Gets or sets the seed used to generate the random sequence.
        /// StoreSeed must be true to retreive the seed.
        /// </summary>
        public uint[] Seed
        {
            get
            {
                if (!storeSeed || seedVals == null)
                    throw new InvalidOperationException("The instance did not store the initial seed value.  "
                           + "This is because the object was initialized with StoreSeed set to false (default), "
                           + "the last call to SetSeed() was made while StoreSeed was false, "
                           + "or StoreSeed was set to false at any time since the last call to SetSeed().");

                return seedVals;
            }
            set
            {
                SetSeed(value);
            }
        }

        #endregion

        #region seeding

        #region static stuff for entropic seeds

        public static uint[] TimeSeed()
        {
            uint b = ShiftVal((uint)Environment.TickCount);

            Thread.Sleep(1);

            long ticks = DateTime.Now.Ticks;
            uint c = ShiftVal((uint)ticks);
            uint f = ShiftVal((uint)( ticks >> 32 ));

            Thread.Sleep(0);

            ticks = Environment.WorkingSet;
            uint d = ShiftVal((uint)ticks);
            uint g = ShiftVal((uint)( ticks >> 32 ));

            ticks = watch.ElapsedTicks;
            uint a = ShiftVal((uint)ticks);
            uint e = ShiftVal((uint)( ticks >> 32 ));

            return new uint[] { a, b, c, d, e, f, g };
        }

        /// <summary>
        /// Takes an unsigned integer assumed to have predictable high-order bits and unpredictable low-order bits
        /// (such as a time signature), and shifts it so that all the bits are more unpredictable.
        /// </summary>
        public static uint ShiftVal(uint value)
        {
            lock (typeof(MTRandom))
                unchecked
                {
                    return ( counter = ShiftVal(value, counter + SHIFT_FACTOR) );
                }
        }
        public static uint ShiftVal(uint value, uint seed)
        {
            unchecked
            {
                value += seed;
                //determine a shift and negation based on the less-predictable low-order bits
                int shift = (int)( value % 124 );
                int neg = shift / 31;
                shift = ( shift % 31 ) + 1;
                //shift to both sides to retain a full 32 bits in the shifted value
                return ( value ^ ( ( ( ( neg & 1 ) == 1 ? value : ~value ) << shift ) | ( ( neg > 1 ? value : ~value ) >> ( 32 - shift ) ) ) );
            }
        }

        #endregion

        #region public overloads to reinitialize with a new seed

        /// <summary>
        /// Resets the random sequence with a default, time-based, random seed.
        /// </summary>
        public void SetSeed()
        {
            //use a large seed size as it will limit the possible states attainable immediately after initialization
            SetSeed(MAX_SEED_SIZE);
        }

        /// <summary>
        /// Resets the random sequence with a default, time-based, random seed.  'storeSeed' of true will store the initial seed for future retreival.
        /// </summary>
        public void SetSeed(bool storeSeed)
        {
            this.storeSeed = storeSeed;
            SetSeed();
        }

        /// <summary>
        /// Resets the random sequence with a default, time-based, random seed with the specified number of unsigned integers.
        /// </summary>
        public void SetSeed(ushort seedSize)
        {
            SetSeed(GenerateSeed(seedSize));
        }

        /// <summary>
        /// Resets the random sequence with a default, time-based, random seed with the specified number of unsigned integers.
        /// 'storeSeed' of true will store the initial seed for future retreival.
        /// </summary>
        public void SetSeed(bool storeSeed, ushort seedSize)
        {
            this.storeSeed = storeSeed;
            SetSeed(seedSize);
        }

        /// <summary>
        /// Resets the random sequence with a specified seed.
        /// 'storeSeed' of true will store the initial seed for future retreival.
        /// </summary>
        public void SetSeed(bool storeSeed, params uint[] seed)
        {
            this.storeSeed = storeSeed;
            SetSeed(seed);
        }

        #endregion

        #region Seeding Algorithm

        public static uint[] GenerateSeed(ushort seedSize)
        {
            //we get the first time seed before doing anything else for 2 reasons:
            //1 - the total time taken to execute this method provides minor entropy
            //2 - the ShiftVal counter is immediately modified based on the current time
            uint[] timeSeed = TimeSeed();

            if (seedSize <= 0 || seedSize > MAX_SEED_SIZE)
                throw new ArgumentOutOfRangeException("seedSize", seedSize,
                        "seedSize must be greater than 0 and less than " + ( MAX_SEED_SIZE + 1 ).ToString());

            int timeSeedLength = timeSeed.Length;
            uint[] seed = new uint[seedSize];

            //pick up some initial system-based entropy, both in the seed and in the ShiftVal counter
            int a = 0;
            Process process = Process.GetCurrentProcess();
            AddShiftedSeed(seed, ref a, process.PagedMemorySize64);
            AddShiftedSeed(seed, ref a, new object());
            AddShiftedSeed(seed, ref a, Guid.NewGuid());
            AddShiftedSeed(seed, ref a, Environment.CommandLine);
            AddShiftedSeed(seed, ref a, process.TotalProcessorTime.Ticks);
            AddShiftedSeed(seed, ref a, Environment.StackTrace);
            AddShiftedSeed(seed, ref a, process.StartTime.Ticks);
            AddShiftedSeed(seed, ref a, Environment.MachineName);
            AddShiftedSeed(seed, ref a, process.Handle);
            AddShiftedSeed(seed, ref a, process.ProcessorAffinity);
            AddShiftedSeed(seed, ref a, process.Id);
            AddShiftedSeed(seed, ref a, Environment.UserName);
            AddShiftedSeed(seed, ref a, process.NonpagedSystemMemorySize64);

            //fill in the entirety of the seed length with time-based entropy
            int b = 0;
            for (a = 0 ; a < seedSize ; ++b)
            {
                if (b >= timeSeedLength)
                {
                    timeSeed = TimeSeed();
                    b = 0;
                }
                AddSeed(seed, ref a, timeSeed[b]);
            }

            //use any remaining uints left over in the timeSeed array 
            for ( ; b < timeSeedLength ; ++b)
                AddSeed(seed, ref a, timeSeed[b]);

            //provide a second time seed if we have only used one so far
            //so that the total time taken to execute this method provides minor entropy
            if (seedSize <= timeSeedLength)
            {
                b = 0;
                for (timeSeed = TimeSeed() ; b < timeSeedLength ; ++b)
                    AddSeed(seed, ref a, timeSeed[b]);
            }
            return seed;
        }
        private static void AddShiftedSeed(uint[] seed, ref int a, object value)
        {
            //shift value to help randomize time or memory signatures
            AddSeed(seed, ref a, ShiftVal((uint)value.GetHashCode()));
        }
        private static void AddSeed(uint[] seed, ref int a, uint value)
        {
            unchecked
            {
                if (a >= seed.Length)
                    a = 0;
                seed[a++] += value;
            }
        }

        /// <summary>
        /// Resets the random sequence with a specified seed.
        /// </summary>
        public void SetSeed(params uint[] seed)
        {
            lock (this)
            {
                uint seedSize = (uint)seed.Length;
                if (seedSize <= 0 || seedSize > MAX_SEED_SIZE)
                    throw new ArgumentOutOfRangeException("seed", seed,
                        "seedSize must be greater than 0 and less than " + ( MAX_SEED_SIZE + 1 ));

                //reset fields
                if (storeSeed)
                    seedVals = (uint[])seed.Clone();
                else
                    seedVals = null;
                m = new uint[LENGTH];
                t = ushort.MaxValue;
                bitCount = 0;
                bits = 0;
                gaussian = double.NaN;
                gaussianFloat = float.NaN;

                //seed KISS and re-use same seed values within MT
                uint a = 0, b = ( seedSize & 1 );
                lcgn = SeedKISS(LCGN_SEED, b++, seed, ref a);
                mwc2 = SeedKISS(MWC2_SEED, b++, seed, ref a);
                mwc1 = SeedKISS(MWC1_SEED, b++, seed, ref a);
                lfsr = SeedKISS(LFSR_SEED, b++, seed, ref a);

                //initialize MT with a constant PRNG
                SeedMT(SEED_FACTOR_1, INIT_SEED);
                //use all seed values in combination with the results of another (different) PRNG pass
                SeedMT(SEED_FACTOR_2, ( 5 - seedSize ) << 1, seed, ref a);
                //run a third and final pass to ensure all seed values are represented in all MT state values
                SeedMT(SEED_FACTOR_3, m[LENGTH - 1]);

                //ensure all seed values are represented in KISS as well
                a = ( a << 1 ) + LENGTH - 3;
                b = m[LENGTH - 1];
                lfsr = b = SeedAlg(lfsr, b, SEED_FACTOR_2, ++a);
                lcgn = b = SeedAlg(lcgn, b, SEED_FACTOR_2, ++a);
                mwc2 = b = SeedAlg(mwc2, b, SEED_FACTOR_2, ++a);
                mwc1 = b = SeedAlg(mwc1, b, SEED_FACTOR_2, ++a);

                //ensure non-zero MT, LFSR, and MWCs (LCG can be zero)
                m[1] = EnsureNonZero(m[1]);
                lfsr = EnsureNonZero(lfsr);
                mwc1 = EnsureNonZero(mwc1);
                mwc2 = EnsureNonZero(mwc2);

                NextUInt();
            }
        }
        private uint SeedKISS(uint initSeed, uint b, uint[] seed, ref uint a)
        {
            unchecked
            {
                return ( m[b] = GetSeed(seed, ref a) ) + initSeed;
            }
        }
        private void SeedMT(uint seedFactor, uint initSeed)
        {
            uint a = 0;
            SeedMT(seedFactor, initSeed, null, ref a);
        }
        private void SeedMT(uint seedFactor, uint initSeed, uint[] seed, ref uint a)
        {
            unchecked
            {
                m[0] += initSeed;
                for (uint b = 1 ; b < LENGTH ; ++b)
                {
                    m[b] = SeedAlg(m[b], m[b - 1], seedFactor, b);
                    if (seed != null)
                        m[b] += GetSeed(seed, ref a) + ( ( 4 + b - a ) << 1 );
                }
            }
        }
        private uint SeedAlg(uint cur, uint prev, uint seedFactor, uint add)
        {
            unchecked
            {
                return ( ( ( ( prev >> 30 ) ^ prev ) * seedFactor ) ^ cur ) + add;
            }
        }
        private uint EnsureNonZero(uint value)
        {
            if (value == 0)
                value = SEED_FACTOR_3;
            return value;
        }
        private uint GetSeed(uint[] seed, ref uint a)
        {
            if (a >= seed.Length)
                a = 0;
            return seed[a++];
        }

        #endregion

        #endregion

        #region PRNG Algorithm

        /// <summary>
        /// Returns a random integer from 0 through 4294967295, inclusive.
        /// </summary>
        public uint NextUInt()
        {
            unchecked
            {
                //combining Marsaglia's KISS with the Mersenne Twister provdes higher quality random numbers with an obscene period>2^20060
                uint value = ( MersenneTwister() + MarsagliaKISS() );

                uint timeVal;
                lock (typeof(MTRandom))
                {
                    //combine in a value based off of the timing of calls
                    timeVal = GetShiftedTicks();
                    counter += value;
                }

                return ( value + timeVal );
            }
        }

        private uint GetShiftedTicks()
        {
            unchecked
            {
                long ticks = watch.ElapsedTicks;
                uint retVal = ShiftVal((uint)ticks + (uint)( ticks >> 32 ));
                if (this.thread == null)
                    retVal = 0;
                return retVal;
            }
        }

        //Marsaglia's KISS (Keep It Simple Stupid) pseudorandom number generator, overall period>2^123.
        private uint MarsagliaKISS()
        {
            unchecked
            {
                return ( LCG() + LFSR() + MWC() );
            }
        }

        //The congruential generator x(n)=69069*x(n-1)+1327217885, period 2^32.
        private uint LCG()
        {
            lock (this)
                unchecked
                {
                    return ( lcgn = LCG(lcgn) );
                }
        }
        public static uint LCG(uint lcgn)
        {
            return LCG_MULTIPLIER * lcgn + LCG_INCREMENT;
        }

        //A 3-shift shift-register generator, period 2^32-1,
        private uint LFSR()
        {
            lock (this)
                return ( lfsr = LFSR(lfsr) );
        }
        public static uint LFSR(uint lfsr)
        {
            return XorLShift(XorRShift(XorLShift(lfsr, LFSR_1), LFSR_2), LFSR_3);
        }
        private static uint XorLShift(uint value, int shift)
        {
            return ( value ^ ( value << shift ) );
        }
        private static uint XorRShift(uint value, int shift)
        {
            return ( value ^ ( value >> shift ) );
        }

        //Two 16-bit multiply-with-carry generators, period 597273182964842497(>2^59)
        private uint MWC()
        {
            lock (this)
                return MWCs(ref mwc1, ref mwc2);
        }
        public static uint MWCs(ref uint mwc1, ref uint mwc2)
        {
            return ( MWC1(ref mwc1) << MWC_SHIFT ) + MWC2(ref mwc2);
        }
        private static uint MWC1(ref uint mwc1)
        {
            return ( mwc1 = MWC(MWC_1_MULT, mwc1) );
        }
        private static uint MWC2(ref uint mwc2)
        {
            return ( mwc2 = MWC(MWC_2_MULT, mwc2) );
        }
        private static uint MWC(uint mult, uint value)
        {
            unchecked
            {
                return ( mult * ( value & MWC_MASK ) + ( value >> MWC_SHIFT ) );
            }
        }

        //int asdf = 0;
        //Mersenne Twister pseudorandom number generator, period 2^19937-1.
        private uint MersenneTwister()
        {
            uint a;

            lock (this)
            {
                if (t >= LENGTH)
                {
                    //Console.WriteLine(++asdf);
                    //generate the next state of N 32-bit uints
                    uint b;
                    for (b = 0 ; b < LENGTH - STEP ; ++b)
                    {
                        a = ( m[b] & UPPER_MASK ) | ( m[b + 1] & LOWER_MASK );
                        m[b] = m[b + STEP] ^ ( a >> 1 ) ^ ODD_FACTOR[a & 1];
                    }
                    for ( ; b < LENGTH - 1 ; ++b)
                    {
                        a = ( m[b] & UPPER_MASK ) | ( m[b + 1] & LOWER_MASK );
                        m[b] = m[b + STEP - LENGTH] ^ ( a >> 1 ) ^ ODD_FACTOR[a & 1];
                    }
                    a = ( m[LENGTH - 1] & UPPER_MASK ) | ( m[0] & LOWER_MASK );
                    m[LENGTH - 1] = m[STEP - 1] ^ ( a >> 1 ) ^ ODD_FACTOR[a & 1];
                    t = 0;
                }

                a = m[t++];
            }

            //tempering
            a ^= ( a >> TEMPER_1 );
            a ^= ( a << TEMPER_2 ) & TEMPER_MASK_2;
            a ^= ( a << TEMPER_3 ) & TEMPER_MASK_3;
            a ^= ( a >> TEMPER_4 );

            return a;
        }

        #endregion

        #region public random methods

        #region methods functionally equivalent to System.Random

        /// <summary>
        /// Returns a uniform random integer from 0 through 2147483646.
        /// This method is functionally equivalent to System.Random.Next().
        /// </summary>
        public int Next()
        {
            return RangeInt(0, int.MaxValue - 1);
        }

        /// <summary>
        /// Returns a uniform random integer from 0 through maxValue-1.
        /// This method is functionally equivalent to System.Random.Next(int).
        /// </summary>
        public int Next(int maxValue)
        {
            return Next(0, maxValue);
        }

        /// <summary>
        /// Returns a uniform random integer from 'minValue' through maxValue-1.
        /// This method is functionally equivalent to System.Random.Next(int,int).
        /// </summary>
        public int Next(int minValue, int maxValue)
        {
            //special case to maintain functional equivalence to System.Random.Next(int,int)
            if (minValue == maxValue)
                return minValue;
            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException("maxValue", maxValue, "maxValue must be greater than or equal to minValue");
            return RangeInt(minValue, maxValue - 1);
        }

        /// <summary>
        /// Returns a uniform random number from 0.0 inclusive to 1.0 exculsive.
        /// This method is functionally equivalent to System.Random.NextDouble().
        /// </summary>
        public double NextDouble()
        {
            //use DOUBLE_DIV_1 to exclude 1.0
            return NextBits(DOUBLE_BITS) / DOUBLE_DIV_1;
        }

        /// <summary>
        /// Entirely overwrites 'buffer' with uniform random bytes from 0 through 255.
        /// This method is functionally equivalent to System.Random.NextBytes(byte[]).
        /// </summary>
        public void NextBytes(byte[] buffer)
        {
            for (int a = buffer.Length ; --a > -1 ; )
                buffer[a] = (byte)NextBits(8);
        }

        ///// <summary>
        ///// Fills the provided byte array with random bytes.
        ///// This method is functionally equivalent to System.Random.NextBytes(). 
        ///// </summary>
        //public unsafe void NextBytes(byte[] buffer)
        //{
        //    int length = ( buffer.Length >> 2 ) << 2;

        //    if (length > 0)
        //        fixed (byte* pByte0 = buffer)
        //        {
        //            uint* pDWord = (uint*)pByte0;
        //            for (int i = 0, len = length >> 2 ; i < len ; i++)
        //                pDWord[i] = NextUInt();
        //        }

        //    if (length != buffer.Length)
        //        InternNextBytes(buffer, length);
        //}
        ///// <summary>
        ///// Fills the provided byte array with random bytes, starting at the given index
        ///// </summary>
        //private void InternNextBytes(byte[] buffer, int i)
        //{
        //    while (i < buffer.Length)
        //    {
        //        //generate 4 bytes
        //        uint w = NextUInt();

        //        //fill the buffer up
        //        for (int c = 0 ; i < buffer.Length && c < 32 ; c += 8)
        //            buffer[i++] = (byte)( w >> c );
        //    }
        //}

        #endregion

        #region other random methods

        /// <summary>
        /// Returns a uniform random integer from 0 through 2^numBits-1.
        /// </summary>
        public ulong NextBits(byte numBits)
        {
            if (numBits <= 0 || numBits >= 65)
                throw new ArgumentOutOfRangeException("numBits", numBits, "numBits must be greater than 0 and less than 65");

            //take initial coherent 32 bit uints when we can
            ulong retVal;
            if (numBits > 32)
                retVal = ( ( (ulong)NextUInt() ) << ( numBits -= 32 ) );
            else
                retVal = 0;
            if (numBits == 32)
                return ( retVal | ( (ulong)NextUInt() ) );

            lock (this)
            {
                //any bits left over need to include an updated time value
                if (this.bitCount > 0)
                    this.bits ^= MaskBits(GetShiftedTicks(), this.bitCount);

                int moreBits = numBits - this.bitCount;
                if (moreBits < 0)
                {
                    //we have enough bits left over, so just copy the amount needed
                    CopyBits(ref retVal, numBits);
                }
                else
                {
                    //use all the remaining bits as high-roder bits
                    retVal |= ( ( (ulong)this.bits ) << moreBits );
                    //reset the bits field with a new set of 32
                    this.bits = NextUInt();
                    this.bitCount = 32;
                    //fill in the result low-order bits
                    if (moreBits > 0)
                        CopyBits(ref retVal, (byte)moreBits);
                }
            }

            return retVal;
        }
        private void CopyBits(ref ulong retVal, byte numBits)
        {
            //consume the bits
            retVal |= MaskBits(this.bits, numBits);
            //remove the used bits and bring the next bits to the bottom
            this.bits >>= numBits;
            //keep track of how many bits we have
            this.bitCount -= numBits;
        }
        private static uint MaskBits(uint bits, byte numBits)
        {
            return ( bits & ( uint.MaxValue >> ( 32 - numBits ) ) );
        }

        /// <summary>
        /// Returns a uniform random number from 0.0 through 2*average.
        /// </summary>
        public double DoubleFull(double average)
        {
            return DoubleFull() * average;
        }

        /// <summary>
        /// Returns a uniform random number from 0.0 through 2*average.
        /// </summary>
        public float DoubleFull(float average)
        {
            return FloatFull() * average;
        }

        /// <summary>
        /// Returns a uniform random number from 0.0 through 2.0.
        /// </summary>
        public double DoubleFull()
        {
            return DoubleHalf() * 2.0;
        }

        /// <summary>
        /// Returns a uniform random number from 0.0 through 2.0.
        /// This method is the float equivalent to DoubleFull().
        /// </summary>
        public float FloatFull()
        {
            return FloatHalf() * 2f;
        }

        /// <summary>
        /// Returns a uniform random number from 0.0 through 'maximum'.
        /// </summary>
        public double DoubleHalf(double maximum)
        {
            return DoubleHalf() * maximum;
        }

        /// <summary>
        /// Returns a uniform random number from 0.0 through 'maximum'.
        /// </summary>
        public float DoubleHalf(float maximum)
        {
            return FloatHalf() * maximum;
        }

        /// <summary>
        /// Returns a uniform random number from 0.0 through 1.0.
        /// </summary>
        public double DoubleHalf()
        {
            //use DOUBLE_DIV to include 1.0
            return NextBits(DOUBLE_BITS) / DOUBLE_DIV;
        }

        /// <summary>
        /// Returns a uniform random number from 0.0 through 1.0.
        /// This method is the float equivalent to DoubleHalf().
        /// </summary>
        public float FloatHalf()
        {
            //use DOUBLE_DIV to include 1.0
            return NextBits(FLOAT_BITS) / FLOAT_DIV;
        }

        /// <summary>
        /// Returns a uniform random number from 0.0 inclusive to 1.0 exculsive.
        /// This method is the float equivalent to NextDouble().
        /// </summary>
        public float NextFloat()
        {
            return NextBits(FLOAT_BITS) / FLOAT_DIV_1;
        }

        /// <summary>
        /// Returns a random integer uniformly distributed between two inclusive values.
        /// </summary>
        public int RangeInt(int value1, int value2)
        {
            //no constraints on the parameters, so check for the simple case or swap them if necessary
            if (value1 == value2)
                return value1;
            if (value1 > value2)
            {
                int temp = value1;
                value1 = value2;
                value2 = temp;
            }

            //determine the number of bits we need
            ulong range = (uint)( value2 - value1 );
            byte numBits = (byte)Math.Ceiling(Math.Log(range + 1) / LN_2);

            //throw out any values outside of the range in order to ensure a uniform distribution (worst-case expected retries <1)
            ulong bits;
            do
            {
                bits = NextBits(numBits);
            } while (bits > range);

            return value1 + (int)bits;
        }

        /// <summary>
        /// Returns a random number uniformly distributed between two inclusive values.
        /// </summary>
        public double Range(double value1, double value2)
        {
            //no constraints on the parameters, so swap them if necessary
            if (value1 > value2)
            {
                double temp = value1;
                value1 = value2;
                value2 = temp;
            }

            return DoubleHalf() * ( value2 - value1 ) + value1;
        }

        /// <summary>
        /// Returns a random number uniformly distributed between two inclusive values.
        /// </summary>
        public float Range(float value1, float value2)
        {
            if (value1 > value2)
            {
                float temp = value1;
                value1 = value2;
                value2 = temp;
            }

            return FloatHalf() * ( value2 - value1 ) + value1;
        }

        /// <summary>
        /// Returns an open-ended random integer starting at 0 with a mean of 1.  Geometric distribution.
        /// </summary>
        public int OEInt()
        {
            return OEInt(1);
        }
        /// <summary>
        /// Returns an open-ended random integer starting at 0 with a mean of 'average'.  Geometric distribution.
        /// </summary>
        public int OEInt(float average)
        {
            return DoOEInt(average, OEFloat(), true);
        }
        /// <summary>
        /// Returns an open-ended random integer starting at 0 with a mean of 'average'.  Geometric distribution.
        /// </summary>
        public int OEInt(double average)
        {
            return DoOEInt(average, OE(), false);
        }

        /// <summary>
        /// Returns an open-ended random number starting at 0.0 with a mean of 1.0.  Exponential distribution.
        /// </summary>
        public float OEFloat()
        {
            return (float)DoOE(NextFloat());
        }
        /// <summary>
        /// Returns an open-ended random number starting at 0.0 with a mean of 1.0.  Exponential distribution.
        /// </summary>
        public double OE()
        {
            return DoOE(NextDouble());
        }
        /// <summary>
        /// Returns an open-ended random number starting at 0.0 with a mean of 'average'.  Exponential distribution.
        /// </summary>
        public float OE(float average)
        {
            return OEFloat() * average;
        }
        /// <summary>
        /// Returns an open-ended random number starting at 0.0 with a mean of 'average'.  Exponential distribution.
        /// </summary>
        public double OE(double average)
        {
            return OE() * average;
        }

        private static int DoOEInt(double average, double oe, bool isFloat)
        {
            if (average == 0)
                return 0;
            //ensure that our result will be able to fit within the range of int
            int limit = ( isFloat ? OE_INT_FLOAT_LIMIT : OE_INT_LIMIT );
            if (average > limit || average < -limit)
                throw new ArgumentOutOfRangeException("average", average, "average must be from -" + limit + " through " + limit);

            bool neg = ( average < 0 );
            if (neg)
                average = -average;

            int retVal = (int)( -oe / Math.Log(average / ( average + 1 )) );

            if (neg)
                retVal = -retVal;
            return retVal;
        }
        private static double DoOE(double nextDouble)
        {
            //we cannot take the log of 0.0, so use 1-NextDouble() to exclude it
            return -Math.Log(1 - nextDouble);
        }

        public static int GetOEIntMax()
        {
            return GetOEIntMax(1);
        }
        public static int GetOEIntMax(float average)
        {
            return DoOEInt(average, GetOEFlaotMax(), true);
        }
        public static int GetOEIntMax(double average)
        {
            return DoOEInt(average, GetOEMax(), false);
        }
        public static float GetOEFlaotMax()
        {
            return (float)DoOE(FLOAT_DIV / FLOAT_DIV_1);
        }
        public static double GetOEMax()
        {
            return DoOE(DOUBLE_DIV / DOUBLE_DIV_1);
        }
        public static float GetOEMax(float average)
        {
            return GetOEFlaotMax() * average;
        }
        public static double GetOEMax(double average)
        {
            return GetOEMax() * average;
        }

        //Old quasi-Geometric-Exponential hybrid code:
        //public int OEInt(double average)
        //{
        //    //ensure with almost certain probability that our result will be able to fit within the range of int
        //    const int max = int.MaxValue / 210;
        //    if (average > max || average < -max)
        //        throw new ArgumentOutOfRangeException("average", average, "average must be between -" + max + " and " + max + ", inclusive");
        //    //we need a certain probability of returning an immediate 0 to account for the rounding bias
        //    //that would normally cause 0's to appear only half as often as one would expect
        //    //x~4.75 is approximately the value at which the probability of Round(OE(x))=0 is 1/8
        //    bool zero = average <= 4.75;
        //    if (!zero)
        //        zero = Bool(4.75 / average);
        //    return Round(OE(zero, average));
        //}
        //private double OE(bool zero, double average)
        //{
        //    //randomly modify the starting termination bit sequence
        //    ulong add = NextBits(3);
        //    //zero of true causes at least 1/8th of all results to be 0, regardless of the average
        //    ulong count = add + ( zero ? 0u : 1u );
        //    //look for a changing termination bit sequence, in order to eliminate any pseudorandom bias
        //    while (NextBits(3) != count % 8)
        //        ++count;
        //    //multiply the result with a random double and account for all variables to maintain the average
        //    return DoubleHalf() * ( ( count - add ) / ( zero ? 3.5 : 4.0 ) ) * average;
        //
        //    //TODO: citywar OE values
        //}

        /// <summary>
        /// Returns a normally distributed integer with a mean of 'average' and standard deviation of average*devPct,
        /// capped at an inclusive minimum value of 0.  The inclusive maximum value will be ceil(2*average).
        /// </summary>
        public int GaussianCappedInt(double average, double devPct)
        {
            return GaussianCappedInt(average, devPct, 0);
        }

        /// <summary>
        /// Returns a normally distributed integer with a mean of 'average' and standard deviation of average*devPct,
        /// capped at an inclusive minimum value of 0.  The inclusive maximum value will be ceil(2*average).
        /// </summary>
        public int GaussianCappedInt(float average, float devPct)
        {
            return GaussianCappedInt(average, devPct, 0);
        }

        /// <summary>
        /// Returns a normally distributed integer with a mean of 'average' and standard deviation of average*devPct,
        /// capped at an inclusive minimum value of 'lowerCap'.  The inclusive maximum value will be ceil(2*average-lowerCap).
        /// </summary>
        public int GaussianCappedInt(double average, double devPct, int lowerCap)
        {
            return GaussianCappedInt(average, devPct, lowerCap, ( average == (float)average && devPct == (float)devPct ));
        }

        /// <summary>
        /// Returns a normally distributed integer with a mean of 'average' and standard deviation of average*devPct,
        /// capped at an inclusive minimum value of 'lowerCap'.  The inclusive maximum value will be ceil(2*average-lowerCap).
        /// </summary>
        public int GaussianCappedInt(float average, float devPct, int lowerCap)
        {
            return GaussianCappedInt(average, devPct, lowerCap, true);
        }

        private int GaussianCappedInt(double average, double devPct, int lowerCap, bool isFloat)
        {
            if (average == lowerCap)
                return lowerCap;
            if (average < lowerCap)
                throw new ArgumentOutOfRangeException("lowerCap", lowerCap, "lowerCap must be less than or equal to average");
            if (2 * average - lowerCap > int.MaxValue)
                CheckGaussianInt(average, average * devPct, isFloat);

            if (average - lowerCap <= .5)
            {
                //not manually handling this case where only 2 posible values should be returned
                //can cause the lowerCap limit to be violated by the upperCap logic
                return lowerCap + Round(average - lowerCap, isFloat);
            }

            //we will need to retry in some cases (worst-case expected retries <1)
            while (true)
            {
                //use lowerCap-1 to allow for the full probability of rounding to exactly lowerCap or upperCap
                double rand = GaussianCapped(average, devPct, lowerCap - 1, isFloat);

                if (rand > average)
                {
                    double upperDbl = average * 2.0 - lowerCap;
                    int upperCap = (int)Math.Ceiling(upperDbl);

                    //in case of a non-integer upperCap, duplicate the probability of discarding low values on the high end
                    //this way we maintain the correct mean
                    double diff = upperCap - upperDbl;
                    int result = Round(rand + diff, isFloat);
                    if (result <= upperCap)
                        return Round(result - diff, isFloat);
                    //retry if result>upperCap
                }
                else
                {
                    int result = Round(rand, isFloat);
                    if (result >= lowerCap)
                        return result;
                    //retry if result<lowerCap
                }
            }
        }

        /// <summary>
        /// Returns a normally distributed number with a mean of 'average' and standard deviation of average*devPct,
        /// capped at an inclusive minimum value of 0.0.  The inclusive maximum value will be 2*average.
        /// </summary>
        public double GaussianCapped(double average, double devPct)
        {
            return GaussianCapped(average, devPct, 0);
        }

        /// <summary>
        /// Returns a normally distributed number with a mean of 'average' and standard deviation of average*devPct,
        /// capped at an inclusive minimum value of 0.0.  The inclusive maximum value will be 2*average.
        /// </summary>
        public float GaussianCapped(float average, float devPct)
        {
            return GaussianCapped(average, devPct, 0);
        }

        /// <summary>
        /// Returns a normally distributed number with a mean of 'average' and standard deviation of average*devPct,
        /// capped at an inclusive minimum value of 'lowerCap'.  The inclusive maximum value will be 2*average-lowerCap.
        /// </summary>
        public double GaussianCapped(double average, double devPct, double lowerCap)
        {
            return GaussianCapped(average, devPct, lowerCap, false);
        }

        /// <summary>
        /// Returns a normally distributed number with a mean of 'average' and standard deviation of average*devPct,
        /// capped at an inclusive minimum value of 'lowerCap'.  The inclusive maximum value will be 2*average-lowerCap.
        /// </summary>
        public float GaussianCapped(float average, float devPct, float lowerCap)
        {
            return (float)GaussianCapped(average, devPct, lowerCap, true);
        }

        private double GaussianCapped(double average, double devPct, double lowerCap, bool isFloat)
        {
            if (average == lowerCap)
                return average;
            if (average < lowerCap)
                throw new ArgumentOutOfRangeException("lowerCap", lowerCap, "lowerCap must be less than or equal to average");

            double result;
            if (isFloat)
                result = Gaussian((float)average, (float)devPct);
            else
                result = Gaussian(average, devPct);
            //We use the mod function as a much faster method than re-trying to remain inside the cap.
            //This does have the side effect (for higher devPct) of causing the probability of returning:
            //1 - the exact lower cap or upper cap to decrease to near-zero
            //2 - the exact average to increase to almost twice what would be expected
            //However, the effect is negligible for a double result, as the probability of returning any exact value is inherintly extrememly low.
            if (result < lowerCap)
                result = average - ( ( average - result ) % ( average - lowerCap ) );
            else if (result > ( average * 2.0 - lowerCap ))
                result = average + ( ( result - average ) % ( average - lowerCap ) );
            return result;
        }

        /// <summary>
        /// Returns a normally distributed integer with a mean of 'average' and standard deviation of average*devPct.
        /// </summary>
        public int GaussianInt(double average, double devPct)
        {
            CheckGaussianInt(average, average * devPct, ( average == (float)average && devPct == (float)devPct ));
            return Round(Gaussian(average, devPct));
        }

        /// <summary>
        /// Returns a normally distributed integer with a mean of 'average' and standard deviation of average*devPct.
        /// </summary>
        public int GaussianInt(float average, float devPct)
        {
            CheckGaussianInt(average, average * devPct, true);
            return Round(Gaussian(average, devPct));
        }

        /// <summary>
        /// Returns a normally distributed number with a mean of 'average' and standard deviation of average*devPct.
        /// </summary>
        public double Gaussian(double average, double devPct)
        {
            return average + Gaussian(average * devPct);
        }

        /// <summary>
        /// Returns a normally distributed number with a mean of 'average' and standard deviation of average*devPct.
        /// </summary>
        public float Gaussian(float average, float devPct)
        {
            return average + Gaussian(average * devPct);
        }

        /// <summary>
        /// Returns a normally distributed integer with a mean of 0 and standard deviation of 'stdDev'.
        /// </summary>
        public int GaussianInt(double stdDev)
        {
            CheckGaussianInt(0, stdDev, ( stdDev == (float)stdDev ));
            return Round(Gaussian(stdDev));
        }

        /// <summary>
        /// Returns a normally distributed integer with a mean of 0 and standard deviation of 'stdDev'.
        /// </summary>
        public int GaussianInt(float stdDev)
        {
            CheckGaussianInt(0, stdDev, true);
            return Round(Gaussian(stdDev));
        }

        private void CheckGaussianInt(double average, double stdDev, bool isFloat)
        {
            double gaussianMax = ( isFloat ? GAUSSIAN_FLOAT_MAX : GAUSSIAN_MAX );
            //ensure that our result will be able to fit within the range of int
            double max = Math.Abs(average) + Math.Abs(gaussianMax * stdDev);
            if (max >= int.MaxValue)
                throw new ArgumentOutOfRangeException("stdDev", max,
                    "|average|+|" + gaussianMax + "*stdDev| (average=" + average + ", stdDev=" + stdDev + ") must be less than " + int.MaxValue);
        }

        /// <summary>
        /// Returns a normally distributed number with a mean of 0.0 and standard deviation of 'stdDev'.
        /// </summary>
        public double Gaussian(double stdDev)
        {
            return stdDev * Gaussian();
        }

        /// <summary>
        /// Returns a normally distributed number with a mean of 0.0 and standard deviation of 'stdDev'.
        /// </summary>
        public float Gaussian(float stdDev)
        {
            return stdDev * GaussianFloat();
        }

        /// <summary>
        /// Returns a normally distributed integer with a mean of 0 and standard deviation of 1.
        /// </summary>
        public int GaussianInt()
        {
            return Round(GaussianFloat());
        }

        /// <summary>
        /// Returns a normally distributed number with a mean of 0.0 and standard deviation of 1.0.
        /// </summary>
        public double Gaussian()
        {
            return Gaussian(false);
        }

        /// <summary>
        /// Returns a normally distributed number with a mean of 0.0 and standard deviation of 1.0.
        /// This method is the float equivalent to Gaussian().
        /// </summary>
        public float GaussianFloat()
        {
            return (float)Gaussian(true);
        }

        private double Gaussian(bool isFloat)
        {
            lock (this)
                //check if we have a buffered result
                if (isFloat ? float.IsNaN(this.gaussianFloat) : double.IsNaN(this.gaussian))
                {
                    //Marsaglia polar method
                    double a, b, c;
                    //(expected retries ~0.273)
                    do
                    {
                        if (isFloat)
                        {
                            a = FloatHalf();
                            b = FloatHalf();
                        }
                        else
                        {
                            a = DoubleHalf();
                            b = DoubleHalf();
                        }
                        c = GetC(ref a, ref b);
                    } while (c > 1 || c == 0);
                    c = DoGaussian(c);

                    //generates two at a time, so store one off and return the other
                    if (isFloat)
                        this.gaussianFloat = (float)( a * c );
                    else
                        this.gaussian = a * c;
                    return b * c;
                }
                else if (isFloat)
                {
                    float retVal = this.gaussianFloat;
                    //double.NaN signifies lack of a buffered value
                    this.gaussianFloat = float.NaN;
                    return retVal;
                }
                else
                {
                    double retVal = this.gaussian;
                    //double.NaN signifies lack of a buffered value
                    this.gaussian = double.NaN;
                    return retVal;
                }
        }
        private static double GetC(ref double a, ref double b)
        {
            a = a * 2 - 1;
            b = b * 2 - 1;
            return a * a + b * b;
        }
        private static double DoGaussian(double c)
        {
            return Math.Sqrt(( -2 * Math.Log(c) ) / c);
        }

        /// <summary>
        /// Returns a random boolean with equal probability.
        /// </summary>
        public bool Bool()
        {
            return ( NextBits(1) != 0 );
        }

        /// <summary>
        /// Returns a random boolean with a chance/1 probability of being true.
        /// </summary>
        public bool Bool(double chance)
        {
            if (chance == (float)chance)
                return Bool((float)chance);

            CheckBool(chance);
            return ( NextDouble() < chance );
        }

        /// <summary>
        /// Returns a random boolean with a chance/1 probability of being true.
        /// </summary>
        public bool Bool(float chance)
        {
            CheckBool(chance);
            return ( NextFloat() < chance );
        }

        private static void CheckBool(double chance)
        {
            if (chance < 0 || chance > 1)
                throw new ArgumentOutOfRangeException("chance", chance, "chance must be between 0 and 1, inclusive");
        }

        /// <summary>
        /// Randomly rounds a number to one of the two closest integers.  The probability of rounding up is (number-floor(number))/1.
        /// </summary>
        public int Round(double number)
        {
            return Round(number, ( number == (float)number ));
        }

        /// <summary>
        /// Randomly rounds a number to one of the two closest integers.  The probability of rounding up is (number-floor(number))/1.
        /// </summary>
        public int Round(float number)
        {
            return Round(number, true);
        }

        private int Round(double number, bool isFloat)
        {
            //the parameter already being an integer is a common situation in real world use
            int result = (int)number;
            if (result == number)
                return result;

            double rand;
            if (isFloat)
                rand = NextFloat();
            else
                rand = NextDouble();
            return Round(number, rand);
        }
        public static int Round(double number, double rand)
        {
            //the parameter already being an integer is a common situation in real world use
            int result = (int)number;
            if (result == number)
                return result;

            if (number > int.MaxValue || number < int.MinValue)
                throw new ArgumentOutOfRangeException("number", number, "number must be within the range of possible int values");

            result = (int)Math.Floor(number);
            number -= result;

            CheckBool(number);
            if (rand < number)
                ++result;

            return result;
        }

        public float GaussianOE(float average, float devPct, float oePct)
        {
            SplitAvg(ref average, ref oePct);
            return GaussianCapped(average, devPct) + OE(oePct);
        }
        public double GaussianOE(double average, double devPct, double oePct)
        {
            SplitAvg(ref average, ref oePct);
            return GaussianCapped(average, devPct) + OE(oePct);
        }
        public int GaussianOEInt(float average, float devPct, float oePct)
        {
            SplitAvg(ref average, ref oePct);
            return GaussianCappedInt(average, devPct) + OEInt(oePct);
        }
        public int GaussianOEInt(double average, double devPct, double oePct)
        {
            SplitAvg(ref average, ref oePct);
            return GaussianCappedInt(average, devPct) + OEInt(oePct);
        }
        public float GaussianOE(float average, float devPct, float oePct, float lowerCap)
        {
            SplitAvg(ref average, ref oePct, lowerCap);
            return GaussianCapped(average, devPct, lowerCap) + OE(oePct);
        }
        public double GaussianOE(double average, double devPct, double oePct, double lowerCap)
        {
            SplitAvg(ref average, ref oePct, lowerCap);
            return GaussianCapped(average, devPct, lowerCap) + OE(oePct);
        }
        public int GaussianOEInt(float average, float devPct, float oePct, int lowerCap)
        {
            SplitAvg(ref average, ref oePct, lowerCap);
            return GaussianCappedInt(average, devPct, lowerCap) + OEInt(oePct);
        }
        public int GaussianOEInt(double average, double devPct, double oePct, int lowerCap)
        {
            SplitAvg(ref average, ref oePct, lowerCap);
            return GaussianCappedInt(average, devPct, lowerCap) + OEInt(oePct);
        }
        private static void SplitAvg(ref float average, ref float oePct)
        {
            SplitAvg(ref average, ref oePct, 0);
        }
        private static void SplitAvg(ref float average, ref float oePct, double lowerCap)
        {
            double averageDouble = average, oePctDouble = oePct;
            SplitAvg(ref averageDouble, ref oePctDouble, lowerCap);
            average = (float)averageDouble;
            oePct = (float)oePctDouble;
        }
        private static void SplitAvg(ref double average, ref double oePct)
        {
            SplitAvg(ref average, ref oePct, 0);
        }
        private static void SplitAvg(ref double average, ref double oePct, double lowerCap)
        {
            oePct *= average;
            average -= oePct;
            if (lowerCap > average)
            {
                oePct += average - lowerCap;
                average = lowerCap;
                if (oePct < 0)
                    throw new ArgumentOutOfRangeException("lowerCap", lowerCap, "lowerCap must be less than or equal to average");
            }
        }

        /// <summary>
        /// Iterates in random order over an enumeration of objects.
        /// Modifications to the enumerable will not affect iteration.
        /// </summary>
        public IEnumerable<T> Iterate<T>(IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");

            //attempt a cast to ICollection to get the count
            ICollection<T> collection = enumerable as ICollection<T>;
            int count;
            if (collection == null)
            {
                //use a list to get the count
                List<T> list = new List<T>(enumerable);
                count = list.Count;
                if (count > 1)
                    return Iterate<T>(list, count, GetItem);
            }
            else if (( count = collection.Count ) > 1)
            {
                //use an array since we already know the count
                T[] array = new T[count];
                collection.CopyTo(array, 0);
                return Iterate<T>(array, count, GetItem);
            }

            //if the count is less than 2 there is no element order
            return EnumerateOnce(enumerable.GetEnumerator());
        }
        private static T GetItem<T>(IList<T> list, int idx)
        {
            return list[idx];
        }
        private IEnumerable<T> EnumerateOnce<T>(IEnumerator<T> enumerator)
        {
            //avoids InvalidOperationException by not calling MoveNext again after returning an item
            if (enumerator.MoveNext())
                yield return enumerator.Current;
        }

        /// <summary>
        /// Iterates in random order over each point on an integral grid in euclidean space
        /// </summary>
        public IEnumerable<Point> Iterate(int width, int height)
        {
            return Iterate(0, width - 1, 0, height - 1);
        }
        /// <summary>
        /// Iterates in random order over each point on an integral grid in euclidean space
        /// </summary>
        public IEnumerable<Point> Iterate(int startX, int endX, int startY, int endY)
        {
            if (startX > endX)
                throw new ArgumentOutOfRangeException("endX", endX, "endX must be greater than or equal to startX");
            if (startY > endY)
                throw new ArgumentOutOfRangeException("endY", endY, "endY must be greater than or equal to startY");

            int height = endY - startY + 1;
            foreach (int coord in Iterate(( endX - startX + 1 ) * height))
                yield return new Point(startX + coord / height, startY + coord % height);
        }

        /// <summary>
        /// Iterates in random order over the integers 0 through count-1.
        /// </summary>
        public IEnumerable<int> Iterate(int count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException("count", "count must be greater than 0");

            return Iterate(new int[count], count, GetItemIdx);
        }
        private static int GetItemIdx(IList<int> list, int idx)
        {
            int item = list[idx];
            //we can avoid having to initialize the array by assuming the index if the value is 0
            if (item == 0)
                return idx;
            return item;
        }

        private delegate T GetItemDelegate<T>(IList<T> list, int idx);
        private IEnumerable<T> Iterate<T>(IList<T> list, int count, GetItemDelegate<T> GetItem)
        {
            while (--count > -1)
            {
                int idx = RangeInt(0, count);
                //yield return in random order as it is determined
                yield return GetItem(list, idx);
                //maintain remaining elements
                if (idx < count)
                    list[idx] = GetItem(list, count);
            }
        }

        public delegate int GetChance<T>(T obj);
        /// <summary>
        /// Returns a random object from 'choices', where 'GetChance' returns the probability of selecting each object.
        /// </summary>
        public T SelectValue<T>(IEnumerable<T> choices, GetChance<T> GetChance)
        {
            if (choices == null)
                throw new ArgumentNullException("choices");
            if (GetChance == null)
                throw new ArgumentNullException("GetChance");

            //we have to loop through all elements to get the total, so we build a dictionary while doing so
            Dictionary<T, int> dictionary = new Dictionary<T, int>();
            int total = 0;
            foreach (T obj in choices)
            {
                int chance;
                //ensure we only call GetChance once for each object, in case it doesnt always return a constant value
                if (!dictionary.TryGetValue(obj, out chance))
                {
                    chance = GetChance(obj);
                    CheckChance(chance);
                    dictionary.Add(obj, chance);
                }
                //when the same object appears in 'choices' multiple times, its probability of being selected increases appropriately
                //so we must also increase the total
                checked
                {
                    total += chance;
                }
            }
            return SelectValue<T>(dictionary, total);
        }
        /// <summary>
        /// Returns a random key from 'choices', where the value is the probability of selecting that key.
        /// </summary>
        public T SelectValue<T>(Dictionary<T, int> choices)
        {
            if (choices == null)
                throw new ArgumentNullException("choices");

            int total = 0;
            foreach (int chance in choices.Values)
            {
                CheckChance(chance);
                checked
                {
                    total += chance;
                }
            }
            return SelectValue<T>(choices, total);
        }
        private void CheckChance(int chance)
        {
            if (chance < 0)
                throw new ArgumentOutOfRangeException("choices", "each individual chance must be greater than or equal to 0");
        }
        private T SelectValue<T>(Dictionary<T, int> dictionary, int total)
        {
            if (total <= 0)
                throw new ArgumentOutOfRangeException("choices", "the sum total of the chances must be greater than 0");

            //select a random number within the total
            total = Next(total);
            //find the object within whose probability range the selection resides
            foreach (KeyValuePair<T, int> pair in dictionary)
            {
                int curChance = pair.Value;
                if (total < curChance)
                    return pair.Key;
                else
                    total -= curChance;
            }

            throw new Exception("internal error");
        }

        /// <summary>
        /// Returns a random number from 0.0 through 1.0 with a mean of 'weight'.
        /// </summary>
        public double Weighted(double weight)
        {
            return Weighted(1.0, weight);
        }

        /// <summary>
        /// Returns a random number from 0.0 through 1.0 with a mean of 'weight'.
        /// </summary>
        public float Weighted(float weight)
        {
            return Weighted(1f, weight);
        }

        /// <summary>
        /// Returns a random number from 0.0 through 'max' with a mean of max*weight.
        /// </summary>
        public double Weighted(double max, double weight)
        {
            return DoWeight(max, ModWeight(weight, false), false);
        }

        /// <summary>
        /// Returns a random number from 0.0 through 'max' with a mean of max*weight.
        /// </summary>
        public float Weighted(float max, float weight)
        {
            return (float)DoWeight(max, ModWeight(weight, true), true);
        }

        /// <summary>
        /// Returns a random integer from 0 through 'max' with a mean of max*weight.
        /// </summary>
        public int WeightedInt(int max, double weight)
        {
            return WeightedInt(max, weight, ( weight == (float)weight ));
        }

        /// <summary>
        /// Returns a random integer from 0 through 'max' with a mean of max*weight.
        /// </summary>
        public int WeightedInt(int max, float weight)
        {
            return WeightedInt(max, weight, true);
        }

        private int WeightedInt(int max, double weight, bool isFloat)
        {
            bool neg;
            int retVal = RangeInt(0, Round(Weighted(max, ModWeight(weight, isFloat), out neg, isFloat), isFloat));
            if (neg)
                retVal = max - retVal;
            return retVal;
        }
        private double ModWeight(double weight, bool isFloat)
        {
            if (weight < 0 || weight > 1)
                throw new ArgumentOutOfRangeException("weight", weight, "weight must be between 0 and 1, inclusive");
            //before generating the actual result, run the same algorithm on the 'weight' parameter itself
            //this prevents result values higher than max*weight*4 from having a uniform distribution
            weight *= 4;
            if (weight > 2)
                return 1 + DoWeight(1, weight - 2, isFloat);
            if (weight < 2)
                return DoWeight(1, weight, isFloat);
            //when weight=.5, we do actually want a uniform distribution
            return 1;
        }
        private double DoWeight(double max, double weight, bool isFloat)
        {
            bool neg;
            double retVal = Weighted(max, weight, out neg, isFloat);

            if (isFloat)
                retVal = DoubleHalf((float)retVal);
            else
                retVal = DoubleHalf(retVal);

            if (neg)
                retVal = max - retVal;
            return retVal;
        }
        private double Weighted(double max, double weight, out bool neg, bool isFloat)
        {
            neg = ( weight > 1 );
            if (neg)
                weight = 2 - weight;

            double mid = max / 2;
            double avg = mid * weight;

            double key;
            if (isFloat)
                key = DoubleHalf((float)avg);
            else
                key = DoubleHalf(avg);

            avg = ( mid - avg ) / ( mid - key );

            bool rand;
            if (isFloat)
                rand = Bool((float)avg);
            else
                rand = Bool(avg);

            if (rand)
                return key * 2;
            else
                return max;
        }

        #endregion

        #endregion

        #region tick

        /// <summary>
        /// Starts a background thread to periodically permutate the algorithm, on average once per second.
        /// For some applications, this will allow entropy in the time between user actions to be brought into the quality of the random numbers.
        /// </summary>
        public void StartTick()
        {
            //default frequency is 1 second
            StartTick(1000);
        }

        /// <summary>
        /// Starts a background thread to periodically permutate the algorithm, on average once per 'frequency' milliseconds.
        /// For some applications, this will allow entropy in the time between user actions to be brought into the quality of the random numbers.
        /// </summary>
        public void StartTick(ushort frequency)
        {
            if (thread == null)
            {
                thread = new Thread(RunTick);
                thread.IsBackground = true;
                thread.Start(frequency);
            }
            else
            {
                StopTick();
                StartTick(frequency);
            }
        }

        private void RunTick(object obj)
        {
            ushort frequency = (ushort)obj;

            while (true)
            {
                //amount of sleep time is random, averaging at frequency
                int sleep = OEInt(frequency);

                //randomly choose an algorithm or number of bits to permutate
                switch (NextBits(4))
                {
                case 0:
                    Gaussian(false);
                    break;
                case 1:
                    Gaussian(true);
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    NextBits((byte)RangeInt(1, 31));
                    break;
                case 7:
                    NextUInt();
                    break;
                case 8:
                case 9:
                case 10:
                    MersenneTwister();
                    break;
                case 11:
                    MarsagliaKISS();
                    break;
                case 12:
                    LCG();
                    break;
                case 13:
                    LFSR();
                    break;
                case 14:
                    MWC1(ref mwc1);
                    break;
                case 15:
                    MWC2(ref mwc2);
                    break;
                default:
                    throw new Exception("internal error");
                }

                //permutate counter
                GetShiftedTicks();

                Thread.Sleep(sleep);
            }
        }

        public void Dispose()
        {
            StopTick();
        }

        /// <summary>
        /// Abort the background thread, if it is running.
        /// </summary>
        public void StopTick()
        {
            if (thread != null)
            {
                //dont want to abort in the middle of an algorithm permutation
                lock (this)
                    lock (typeof(MTRandom))
                        thread.Abort();
                thread = null;
            }
        }

        #endregion

    }
}
