using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

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
            double a = (div - 1) / 2 / div;
            double b = a;
            double c = GetC(ref a, ref b);
            return Math.Abs(a * DoGaussian(c));
        }

        //the maximum number of seed values that can be incorporated into the generator's initial state
        public const ushort MAX_SEED_SIZE = LENGTH + 4;

        //constants for float generation and conversion
        public const byte FLOAT_BITS = 24;
        private const float FLOAT_DIV = 0x0FFFFFF;
        private const float FLOAT_DIV_1 = 0x1000000;

        //constants for double generation and conversion
        public const byte DOUBLE_BITS = 53;
        private const double DOUBLE_DIV = 0x1FFFFFFFFFFFFF;
        private const double DOUBLE_DIV_1 = 0x20000000000000;

        private static readonly int OE_INT_LIMIT;           //  58,455,924 ( 36.7368005696771 )
        private static readonly int OE_INT_FLOAT_LIMIT;     // 129,090,164 ( 16.6355324       )

        public static readonly double GAUSSIAN_MAX;         // 8.531146110505567
        public static readonly float GAUSSIAN_FLOAT_MAX;    // 5.707707

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

        private const uint LCGN_SEED = 0x075BCD15;      //00000111010110111100110100010101
        private const uint LFSR_SEED = 0x159A55E5;      //00010101100110100101010111100101
        private const uint MWC1_SEED = 0x1F123BB5;      //00011111000100100011101110110101
        private const uint MWC2_SEED = 0x369BF75D;      //00110110100110111111011101011101

        private const uint SHIFT_FACTOR = 0x816B8DF8;   //10000001011010111000110111111000

        #endregion

        #region fields

        //used for shifting time values into seeds
        private static uint counter = 0xF2154EE4;       //11110010000101010100111011100100

        //optional ticker thread to independently permutate the algorithms
        private Thread thread = null;
        private static readonly Stopwatch watch;

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
            uint f = ShiftVal((uint)(ticks >> 32));

            Thread.Sleep(0);

            ticks = Environment.WorkingSet;
            uint d = ShiftVal((uint)ticks);
            uint g = ShiftVal((uint)(ticks >> 32));

            ticks = watch.ElapsedTicks;
            uint a = ShiftVal((uint)ticks);
            uint e = ShiftVal((uint)(ticks >> 32));

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
                    counter = ShiftVal(value, counter + SHIFT_FACTOR);
                    return counter;
                }
        }
        public static uint ShiftVal(uint value, uint seed)
        {
            unchecked
            {
                value += seed;
                //determine a shift and negation based on the less-predictable low-order bits
                int shift = (int)(value % (31 * 4));
                int neg = shift / 31;
                shift = (shift % 31) + 1;
                //shift to both sides to retain a full 32 bits in the shifted value
                uint v1 = ((neg & 1) == 1 ? value : ~value) << (shift);
                uint v2 = ((neg & 2) == 2 ? value : ~value) >> (32 - shift);
                value ^= v1 | v2;
                return value;
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

        public static uint[] GenerateSeed(IEnumerable<object> seedData)
        {
            byte[] bytes = seedData
                .SelectMany(obj => obj is System.Collections.IEnumerable arr ? arr.OfType<object>() : new object[] { obj })
                .SelectMany(obj => obj is string str ? str.ToCharArray().Cast<object>() : new object[] { obj })
                .SelectMany(GetBytes)
                .ToArray();
            uint[] seed = new uint[(bytes.Length + 3) / 4];
            for (int a = 0; a < bytes.Length; a++)
                seed[a / 4] ^= (uint)bytes[a] << ((a % 4) * 8);
            if (seed.Length > MTRandom.MAX_SEED_SIZE)
            {
                uint[] copy = new uint[MTRandom.MAX_SEED_SIZE];
                for (uint b = 0; b < seed.Length; b++)
                {
                    uint c = b % MTRandom.MAX_SEED_SIZE;
                    ulong d = 31ul * copy[c] + seed[b] + b;
                    copy[c] = (uint)(d >> 32) + (uint)d;
                }
                seed = copy;
            }
            return seed;

            static byte[] GetBytes(object obj)
            {
                if (obj is null)
                    return new byte[] { 188, 74, 110 };

                Type type = obj.GetType();
                if (type.IsEnum)
                {
                    type = type.GetEnumUnderlyingType();
                    obj = Convert.ChangeType(obj, type);
                }

                if (!type.IsValueType)
                    throw new ArgumentException($"{obj?.GetType()} must be a value type; {obj}");

                int size = Marshal.SizeOf(obj);
                //?
                if (size > 8)
                    throw new ArgumentException($"{obj?.GetType()} must be less than or equal to 8 bytes; {obj}");

                byte[] arr = new byte[size];
                IntPtr ptr = IntPtr.Zero;
                try
                {
                    ptr = Marshal.AllocHGlobal(size);
                    Marshal.StructureToPtr(obj, ptr, true);
                    Marshal.Copy(ptr, arr, 0, size);
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
                return arr;
            }
        }

        public static uint[] GenerateSeed(ushort seedSize)
        {
            //we get the first time seed before doing anything else for 2 reasons:
            //1 - the total time taken to execute this method provides minor entropy
            //2 - the ShiftVal counter is immediately modified based on the current time
            uint[] timeSeed = TimeSeed();

            if (seedSize <= 0 || seedSize > MAX_SEED_SIZE)
                throw new ArgumentOutOfRangeException("seedSize", seedSize,
                        "seedSize must be greater than 0 and less than " + (MAX_SEED_SIZE + 1).ToString());

            int timeSeedLength = timeSeed.Length;
            uint[] seed = new uint[seedSize];

            int a = 0, b;
            bool c = true;

            //pick up some initial system-based entropy, both in the seed and in the ShiftVal counter
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
            for (b = 0; a < seedSize; ++b)
            {
                if (b == timeSeedLength)
                {
                    c = false;
                    timeSeed = TimeSeed();
                    b = 0;
                }
                AddSeed(seed, ref a, timeSeed[b]);
            }

            //use any remaining uints left over in the timeSeed array 
            for (; b < timeSeedLength; ++b)
                AddSeed(seed, ref a, timeSeed[b]);

            //provide a second time seed if we have only used one so far
            //so that the total time taken to execute this method provides minor entropy
            if (c)
            {
                timeSeed = TimeSeed();
                for (b = 0; b < timeSeedLength; ++b)
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
                if (a == seed.Length)
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
                            "seed length must be greater than 0 and less than " + (MAX_SEED_SIZE + 1));

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
                uint a = 0, b = (seedSize + 1) % 2; //"b" offset avoids using the same seed value twice in m[0] when seedSize == 2 || 4
                lcgn = SeedKISS(LCGN_SEED, b++, seed, ref a);
                mwc2 = SeedKISS(MWC2_SEED, b++, seed, ref a);
                mwc1 = SeedKISS(MWC1_SEED, b++, seed, ref a);
                lfsr = SeedKISS(LFSR_SEED, b++, seed, ref a);

                //initialize MT with a constant PRNG
                SeedMT(SEED_FACTOR_1, INIT_SEED, a);
                //use all seed values in combination with the results of another (different) PRNG pass
                SeedMT(SEED_FACTOR_2, seedSize, seed, ref a);
                //run a third and final pass to ensure all seed values are represented in all MT state values
                SeedMT(SEED_FACTOR_3, m[LENGTH - 1], a);

                a += MAX_SEED_SIZE;
                b = m[LENGTH - 1];

                //ensure non-zero MT
                // technically as long as any "m" is non-zero the generator works
                // but you want to avoid bit-sparsity in general for LFSR-type generators
                // so we go ahead and ensure all "m" are non-zero
                for (uint c = 0; c < LENGTH; ++c)
                    m[c] = EnsureNonZero(m[c], seed, ref a);

                //ensure all seed values are represented in KISS as well
                lfsr = b = SeedAlg(lfsr, b, SEED_FACTOR_2, ++a);
                lcgn = b = SeedAlg(lcgn, b, SEED_FACTOR_2, ++a);
                mwc2 = b = SeedAlg(mwc2, b, SEED_FACTOR_2, ++a);
                mwc1 = b = SeedAlg(mwc1, b, SEED_FACTOR_2, ++a);

                //ensure non-zero LFSR, and MWCs (LCG can be zero)
                lfsr = EnsureNonZero(lfsr, seed, ref a);
                mwc1 = EnsureNonZero(mwc1, seed, ref a);
                mwc2 = EnsureNonZero(mwc2, seed, ref a);

                NextUInt();
            }
        }
        private uint SeedKISS(uint initSeed, uint b, uint[] seed, ref uint a)
        {
            unchecked
            {
                return (m[b] = GetSeed(seed, ref a) - 1) + initSeed;
            }
        }
        private void SeedMT(uint seedFactor, uint initSeed, uint a)
        {
            uint c = a;
            SeedMT(seedFactor, initSeed, null, ref c);
        }
        private void SeedMT(uint seedFactor, uint initSeed, uint[] seed, ref uint a)
        {
            unchecked
            {
                m[0] = SeedAlg(m[0], initSeed, seedFactor, a);
                if (seed != null)
                    m[0] += GetSeed(seed, ref a);
                for (uint b = 1; b < LENGTH; ++b)
                {
                    m[b] = SeedAlg(m[b], m[b - 1], seedFactor, 4 + b);
                    if (seed != null)
                        m[b] += GetSeed(seed, ref a) + ((4 + b - a) << 1); //additional salt increases when "a" wraps around
                }
            }
        }
        private uint SeedAlg(uint cur, uint prev, uint seedFactor, uint add)
        {
            unchecked
            {
                return ((((prev >> 30) ^ prev) * seedFactor) ^ cur) + add;
            }
        }
        private uint EnsureNonZero(uint value, uint[] seed, ref uint a)
        {
            while (value == 0)
            {
                a += 631; //lowest prime that is > MAX_SEED_SIZE
                value = ShiftVal(SHIFT_FACTOR, seed[a % seed.Length] + a);
            }
            return value;
        }
        private uint GetSeed(uint[] seed, ref uint a)
        {
            if (a == seed.Length)
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
                uint value = (MersenneTwister() + MarsagliaKISS());

                uint timeVal;
                lock (typeof(MTRandom))
                {
                    //combine in a value based off of the timing of calls
                    timeVal = GetShiftedTicks();
                    counter += value;
                }

                //Console.Write(formatBits(value + timeVal, 32));

                return (value + timeVal);
            }
        }

        private uint GetShiftedTicks()
        {
            unchecked
            {
                long ticks = watch.ElapsedTicks;
                uint retVal = ShiftVal((uint)ticks + (uint)(ticks >> 32));
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
                return (LCG() + LFSR() + MWC());
            }
        }

        //The congruential generator x(n)=69069*x(n-1)+1327217885, period 2^32.
        private uint LCG()
        {
            lock (this)
                return (lcgn = LCG(lcgn));
        }
        public static uint LCG(uint lcgn)
        {
            unchecked
            {
                return LCG_MULTIPLIER * lcgn + LCG_INCREMENT;
            }
        }

        //A 3-shift shift-register generator, period 2^32-1,
        private uint LFSR()
        {
            lock (this)
                return (lfsr = LFSR(lfsr));
        }
        public static uint LFSR(uint lfsr)
        {
            return XorLShift(XorRShift(XorLShift(lfsr, LFSR_1), LFSR_2), LFSR_3);
        }
        private static uint XorLShift(uint value, int shift)
        {
            unchecked
            {
                return (value ^ (value << shift));
            }
        }
        private static uint XorRShift(uint value, int shift)
        {
            unchecked
            {
                return (value ^ (value >> shift));
            }
        }

        //Two 16-bit multiply-with-carry generators, period 597273182964842497(>2^59)
        private uint MWC()
        {
            lock (this)
                return MWCs(ref mwc1, ref mwc2);
        }
        public static uint MWCs(ref uint mwc1, ref uint mwc2)
        {
            unchecked
            {
                return (MWC1(ref mwc1) << MWC_SHIFT) + MWC2(ref mwc2);
            }
        }
        private static uint MWC1(ref uint mwc1)
        {
            return (mwc1 = MWC(MWC_1_MULT, mwc1));
        }
        private static uint MWC2(ref uint mwc2)
        {
            return (mwc2 = MWC(MWC_2_MULT, mwc2));
        }
        private static uint MWC(uint mult, uint value)
        {
            unchecked
            {
                return (mult * (value & MWC_MASK) + (value >> MWC_SHIFT));
            }
        }

        //int mersenneCount = 0;
        //Mersenne Twister pseudorandom number generator, period 2^19937-1.
        private uint MersenneTwister()
        {
            unchecked
            {
                uint a;

                lock (this)
                {
                    if (t >= LENGTH)
                    {
                        //Console.WriteLine("MersenneTwister " + mersenneCount++);
                        //generate the next state of N 32-bit uints
                        uint b;
                        for (b = 0; b < LENGTH - STEP; ++b)
                        {
                            a = (m[b] & UPPER_MASK) | (m[b + 1] & LOWER_MASK);
                            m[b] = m[b + STEP] ^ (a >> 1) ^ ODD_FACTOR[a & 1];
                        }
                        for (; b < LENGTH - 1; ++b)
                        {
                            a = (m[b] & UPPER_MASK) | (m[b + 1] & LOWER_MASK);
                            m[b] = m[b + STEP - LENGTH] ^ (a >> 1) ^ ODD_FACTOR[a & 1];
                        }
                        a = (m[LENGTH - 1] & UPPER_MASK) | (m[0] & LOWER_MASK);
                        m[LENGTH - 1] = m[STEP - 1] ^ (a >> 1) ^ ODD_FACTOR[a & 1];
                        t = 0;
                    }

                    a = m[t++];
                }

                //tempering
                a ^= (a >> TEMPER_1);
                a ^= (a << TEMPER_2) & TEMPER_MASK_2;
                a ^= (a << TEMPER_3) & TEMPER_MASK_3;
                a ^= (a >> TEMPER_4);

                return a;
            }
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
            for (int a = buffer.Length; --a > -1;)
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

        //ulong totalBits = 0;
        /// <summary>
        /// Returns a uniform random integer from 0 through 2^numBits-1.
        /// </summary>
        public ulong NextBits(byte numBits)
        {
            int orig = numBits;
            if (numBits <= 0 || numBits >= 65)
                throw new ArgumentOutOfRangeException("numBits", numBits, "numBits must be greater than 0 and less than 65");

            //Console.WriteLine(totalBits += numBits);

            //take initial coherent 32 bit uints when we can
            ulong retVal;
            if (numBits > 32)
                retVal = (((ulong)NextUInt()) << (numBits -= 32));
            else
                retVal = 0;
            if (numBits == 32)
                return (retVal | NextUInt());

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
                    retVal |= (((ulong)this.bits) << moreBits);
                    //reset the bits field with a new set of 32
                    this.bits = NextUInt();
                    this.bitCount = 32;
                    //fill in the result low-order bits
                    if (moreBits > 0)
                        CopyBits(ref retVal, (byte)moreBits);
                }
            }

            //Console.Write(formatBits(retVal, orig));
            //Console.WriteLine(formatBits(retVal, orig));

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
            return (bits & (uint.MaxValue >> (32 - numBits)));
        }

        private static string FormatBits(ulong value, int numBits)
        {
            ////if (numBits < 13) return "";
            //int val = (int)(65 + 58 * (value / (Math.Pow(2, numBits))));
            //if (val > 90 && val < 97)
            //    val = 32;
            //return ((char)val).ToString();

            //Console.WriteLine(numBits);
            return Convert.ToString((long)value, 2).PadLeft(numBits, '0');
        }

        //    public static String toAlphanumeric(BigInteger value)
        //    {
        //        if (value.signum() == -1)
        //        {
        //            value = value.negate().shiftLeft(1);
        //            if (value.testBit(value.bitLength() - 2))
        //            {
        //                value = value.setBit(0);
        //            }
        //        }

        //        StringBuilder builder = new StringBuilder();
        //        do
        //        {
        //            builder.append(getChar(value.intValue() & 0x3F));
        //        } while (( value = value.shiftRight(6) ).signum() == 1);
        //        return builder.toString();
        //    }
        //    private static char getChar(int value)
        //    {
        //        if (value < 1)
        //        {
        //            // '-'
        //            value += 45;
        //        }
        //        else if (value < 11)
        //        {
        //            // 0-9
        //            value += 47;
        //        }
        //        else if (value < 37)
        //        {
        //            // A-Z
        //            value += 54;
        //        }
        //        else if (value < 38)
        //        {
        //            // '_'
        //            value += 58;
        //        }
        //        else
        //        {
        //            // a-z
        //            value += 59;
        //        }
        //        return (char)value;
        //    }

        //}

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
                (value2, value1) = (value1, value2);
            }

            //determine the number of bits we need
            ulong range = (uint)(value2 - value1);
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
                (value2, value1) = (value1, value2);
            }

            return DoubleHalf() * (value2 - value1) + value1;
        }

        /// <summary>
        /// Returns a random number uniformly distributed between two inclusive values.
        /// </summary>
        public float Range(float value1, float value2)
        {
            if (value1 > value2)
            {
                (value2, value1) = (value1, value2);
            }

            return FloatHalf() * (value2 - value1) + value1;
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
            if (average == (float)average)
                return OEInt((float)average);

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
            int limit = (isFloat ? OE_INT_FLOAT_LIMIT : OE_INT_LIMIT);
            if (average > limit || average < -limit)
                throw new ArgumentOutOfRangeException("average", average, "average must be from -" + limit + " through " + limit);

            bool neg = (average < 0);
            if (neg)
                average = -average;

            int retVal = (int)(-oe / Math.Log(average / (average + 1)));

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
            if (average == (float)average)
                return GetOEIntMax((float)average);

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
            return GaussianCappedInt(average, devPct, lowerCap, (average == (float)average && devPct == (float)devPct));
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
            //prevent boundary errors
            if (lowerCap == int.MinValue)
                ++lowerCap;

            //we will need to retry in some cases (worst-case expected retries <1)
            while (true)
            {
                //use lowerCap-1 to allow for the full probability of rounding to exactly lowerCap or upperCap
                double rand = GaussianCapped(average, devPct, lowerCap - 1.0, isFloat);

                if (rand > average)
                {
                    double upperDbl = average * 2.0 - lowerCap;
                    double upperCap = Math.Ceiling(upperDbl);

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
                result = average - ((average - result) % (average - lowerCap));
            else if (result > (average * 2.0 - lowerCap))
                result = average + ((result - average) % (average - lowerCap));
            return result;
        }

        /// <summary>
        /// Returns a normally distributed integer with a mean of 'average' and standard deviation of average*devPct.
        /// </summary>
        public int GaussianInt(double average, double devPct)
        {
            if (average == (float)average && devPct == (float)devPct)
                return GaussianInt((float)average, (float)devPct);

            CheckGaussianInt(average, average * devPct, false);
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
            if (stdDev == (float)stdDev)
                return GaussianInt((float)stdDev);

            CheckGaussianInt(0, stdDev, false);
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
            double gaussianMax = (isFloat ? GAUSSIAN_FLOAT_MAX : GAUSSIAN_MAX);
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
                        this.gaussianFloat = (float)(a * c);
                    else
                        this.gaussian = a * c;
                    return b * c;
                }
                else if (isFloat)
                {
                    float retVal = this.gaussianFloat;
                    //float.NaN signifies lack of a buffered value
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
            return Math.Sqrt((-2 * Math.Log(c)) / c);
        }

        /// <summary>
        /// Returns a random boolean with equal probability.
        /// </summary>
        public bool Bool()
        {
            return (NextBits(1) != 0);
        }

        /// <summary>
        /// Returns a random boolean with a chance/1 probability of being true.
        /// </summary>
        public bool Bool(double chance)
        {
            if (chance == (float)chance)
                return Bool((float)chance);

            CheckBool(chance);
            return (NextDouble() < chance);
        }

        /// <summary>
        /// Returns a random boolean with a chance/1 probability of being true.
        /// </summary>
        public bool Bool(float chance)
        {
            CheckBool(chance);
            return (NextFloat() < chance);
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
            return Round(number, (number == (float)number));
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
            if (number > int.MaxValue || number < int.MinValue)
                throw new ArgumentOutOfRangeException("number", number, "number must be within the range of possible int values");

            int result = (int)Math.Floor(number);
            number -= result;

            CheckBool(number);
            if (rand < number)
                ++result;

            return result;
        }

        public float GaussianOE(float average, float devPct, float oePct)
        {
            SplitAvg(ref average, ref devPct, ref oePct);
            return GaussianCapped(average, devPct) + OE(oePct);
        }
        public double GaussianOE(double average, double devPct, double oePct)
        {
            SplitAvg(ref average, ref devPct, ref oePct);
            return GaussianCapped(average, devPct) + OE(oePct);
        }
        public int GaussianOEInt(float average, float devPct, float oePct)
        {
            SplitAvg(ref average, ref devPct, ref oePct);
            return GaussianCappedInt(average, devPct) + OEInt(oePct);
        }
        public int GaussianOEInt(double average, double devPct, double oePct)
        {
            SplitAvg(ref average, ref devPct, ref oePct);
            return GaussianCappedInt(average, devPct) + OEInt(oePct);
        }
        public float GaussianOE(float average, float devPct, float oePct, float lowerCap)
        {
            SplitAvg(ref average, ref devPct, ref oePct, lowerCap);
            return GaussianCapped(average, devPct, lowerCap) + OE(oePct);
        }
        public double GaussianOE(double average, double devPct, double oePct, double lowerCap)
        {
            SplitAvg(ref average, ref devPct, ref oePct, lowerCap);
            return GaussianCapped(average, devPct, lowerCap) + OE(oePct);
        }
        public int GaussianOEInt(float average, float devPct, float oePct, int lowerCap)
        {
            SplitAvg(ref average, ref devPct, ref oePct, lowerCap);
            return GaussianCappedInt(average, devPct, lowerCap) + OEInt(oePct);
        }
        public int GaussianOEInt(double average, double devPct, double oePct, int lowerCap)
        {
            SplitAvg(ref average, ref devPct, ref oePct, lowerCap);
            return GaussianCappedInt(average, devPct, lowerCap) + OEInt(oePct);
        }
        private static void SplitAvg(ref float average, ref float devPct, ref float oePct)
        {
            SplitAvg(ref average, ref devPct, ref oePct, 0);
        }
        private static void SplitAvg(ref float average, ref float devPct, ref float oePct, double lowerCap)
        {
            double averageDouble = average, devPctDouble = devPct, oePctDouble = oePct;
            SplitAvg(ref averageDouble, ref devPctDouble, ref oePctDouble, lowerCap);
            average = (float)averageDouble;
            oePct = (float)oePctDouble;
        }
        //private static void SplitAvg(ref double average, ref double oePct)
        //{
        //    SplitAvg(ref average, ref oePct, 0);
        //}
        private static void SplitAvg(ref double average, ref double oePct, double lowerCap)
        {
            if (average == lowerCap)
            {
                oePct = 0;
            }
            else
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
        }
        private static void SplitAvg(ref double average, ref double devPct, ref double oePct)
        {
            SplitAvg(ref average, ref devPct, ref oePct, 0);
        }
        private static void SplitAvg(ref double average, ref double devPct, ref double oePct, double lowerCap)
        {
            SplitAvg(ref average, ref oePct, lowerCap);

            // TODO: devPct>1

            //if (lowerCap > average)
            //    throw new ArgumentOutOfRangeException("lowerCap", lowerCap, "lowerCap must be less than or equal to average");

            //devPct = Math.Abs(devPct);
            //oePct = Math.Abs(oePct);
            //double needed = devPct + oePct;
            //double avail = Math.Min(( average - lowerCap ) / average, needed);
            //oePct *= avail / needed;

            //oePct *= average;
            //average -= oePct;

            //if (lowerCap > average)
            //    throw new Exception();
        }

        /// <summary>
        /// Iterates in random order over an enumeration of objects.
        /// Modifications to the parameter enumerable will not affect iteration.
        /// The same return enumerable can be re-used for different orderings of the same objects.
        /// </summary>
        public IEnumerable<T> Iterate<T>(IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");

            return Iterate<T>(enumerable.ToList());
        }

        /// <summary>
        /// Iterates in random order over each point on an integral grid in euclidean space.
        /// </summary>
        public IEnumerable<Point> Iterate(int width, int height)
        {
            if (width < 1)
                throw new ArgumentOutOfRangeException("width", width, "width must be greater than 0");
            if (height < 1)
                throw new ArgumentOutOfRangeException("height", height, "height must be greater than 0");

            return Iterate(0, width - 1, 0, height - 1);
        }
        /// <summary>
        /// Iterates in random order over each point on an integral grid in euclidean space.
        /// </summary>
        public IEnumerable<Point> Iterate(int startX, int endX, int startY, int endY)
        {
            if (startX > endX)
                throw new ArgumentOutOfRangeException("endX", endX, "endX must be greater than or equal to startX");
            if (startY > endY)
                throw new ArgumentOutOfRangeException("endY", endY, "endY must be greater than or equal to startY");

            int height = endY - startY + 1;
            return Iterate((endX - startX + 1) * height)
                    .Select(coord => new Point(startX + coord / height, startY + coord % height));
        }
        /// <summary>
        /// Iterates in random order over the integers 0 through count-1.
        /// </summary>
        public IEnumerable<int> Iterate(int count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException("count", count, "count must be greater than 0");

            return Iterate(new int[count], (list, idx) =>
            {
                int item = list[idx];
                //we can avoid having to initialize the array by assuming the index if the value is 0
                if (item == 0)
                    return idx;
                return item;
            });
        }

        /// <summary>
        /// Modifies the order of a list to random order.
        /// </summary>
        public void Shuffle<T>(IList<T> list)
        {
            IEnumerator<T> enumerator = Iterate(list, shuffle: true).GetEnumerator();
            while (enumerator.MoveNext())
                ;
        }

        private IEnumerable<T> Iterate<T>(IList<T> list, Func<IList<T>, int, T> GetItem = null, bool shuffle = false)
        {
            int min = 0, max = list.Count - 1;
            while (min <= max)
            {
                //select a random remaining object
                int idx = RangeInt(min, max);
                T next = (GetItem == null ? list[idx] : GetItem(list, idx));

                //yield return so that we don't fully shuffle the list unless the entire enumerable is walked
                yield return next;

                //ensure multiple calls to the same returned enumerable will work correctly
                //even if broken out of prematurely, reset, excercised again, etc.
                if (idx > min || shuffle)
                {
                    if (idx < max)
                    {
                        //maintain remaining objects
                        list[idx] = (GetItem == null ? list[max] : GetItem(list, max));
                        list[max] = next;
                    }
                    --max;
                }
                else
                {
                    ++min;
                }
            }
        }

        /// <summary>
        /// Returns a random object from 'choices' with equal probability.
        /// </summary>
        public T SelectValue<T>(IEnumerable<T> choices)
        {
            if (choices == null)
                throw new ArgumentNullException("choices");

            int count = -1;
            //if we have both a count and random access, we do not have to walk the enumeration at all
            IList<T> list = (choices as IList<T>);
            if (list == null)
                if (choices is not ICollection<T> collection)
                    //we must completely walk the enumeration to get the count, so put the results in a data structure with random access
                    list = choices.ToList();
                else
                    //with a count, we only have to walk the enumeration until the selected index
                    count = collection.Count;
            if (list != null)
                count = list.Count;

            if (count < 1)
                throw new ArgumentException("choices cannot be empty", "choices");

            int idx = Next(count);
            if (list == null)
                return choices.ElementAt(idx);
            else
                return list[idx];
        }

        /// <summary>
        /// Returns a random object from 'choices', where 'GetChance' returns the probability of selecting each object.
        /// </summary>
        public T SelectValue<T>(IEnumerable<T> choices, Func<T, int> GetChance)
        {
            if (choices == null)
                throw new ArgumentNullException("choices");
            if (GetChance == null)
                throw new ArgumentNullException("GetChance");

            //we have to loop through all elements to get the total, so we build a dictionary while doing so
            Dictionary<T, int> dictionary = new();
            int total = 0;
            foreach (T obj in choices)
            {
                //ensure we only call GetChance once for each object, in case it doesnt always return a constant value
                if (!dictionary.TryGetValue(obj, out int chance))
                {
                    chance = GetChance(obj);
                    CheckChance(chance);
                    dictionary.Add(obj, chance);
                }
                //when the same object appears in 'choices' multiple times, its probability of being selected increases appropriately
                //so we must also increase the total
                total = checked(total + chance);
            }
            return SelectValue<T>(choices, dictionary, total);
        }
        /// <summary>
        /// Returns a random key from 'choices', where the value is the probability of selecting that key.
        /// </summary>
        public T SelectValue<T>(IDictionary<T, int> choices)
        {
            if (choices == null)
                throw new ArgumentNullException("choices");

            int total = 0;
            foreach (int chance in choices.Values)
            {
                CheckChance(chance);
                total = checked(total + chance);
            }
            return SelectValue<T>(choices.Keys, choices, total);
        }
        private void CheckChance(int chance)
        {
            if (chance < 0)
                throw new ArgumentOutOfRangeException("choices", chance, "each individual chance must be greater than or equal to 0");
        }
        private T SelectValue<T>(IEnumerable<T> choices, IDictionary<T, int> chances, int total)
        {
            if (total <= 0)
                throw new ArgumentOutOfRangeException("choices", total, "the sum total of the chances must be greater than 0");

            //select a random number within the total
            total = Next(total);
            //find the object within whose probability range the selection resides
            foreach (T key in choices)
            {
                int curChance = chances[key];
                if (total < curChance)
                    return key;
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
            return WeightedInt(max, weight, (weight == (float)weight));
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
            int retVal = RangeInt(0, Round(Weighted(max, ModWeight(weight, isFloat), out bool neg, isFloat), isFloat));
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
            double retVal = Weighted(max, weight, out bool neg, isFloat);

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
            neg = (weight > 1);
            if (neg)
                weight = 2 - weight;

            double mid = max / 2;
            double avg = mid * weight;

            double key;
            if (isFloat)
                key = DoubleHalf((float)avg);
            else
                key = DoubleHalf(avg);

            avg = (mid - avg) / (mid - key);

            bool rand;
            if (isFloat)
            {
                //check for rounding errors
                float avgFloat = (float)avg;
                if (avgFloat > 1f)
                    avgFloat = 1f;
                else if (avgFloat < 0f)
                    avgFloat = 0f;

                rand = Bool(avgFloat);
            }
            else
            {
                rand = Bool(avg);
            }

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
                thread = new Thread(RunTick)
                {
                    IsBackground = true
                };
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

            int inc = 0;
            while (true)
            {
                //Console.WriteLine(inc++);
                //Console.WriteLine(watch.Elapsed.TotalSeconds.ToString("0.0"));

                //amount of sleep time is random, averaging at frequency
                int sleep = OEInt(frequency);
                //Console.WriteLine(sleep);

                object throwAway;

                ulong choose = NextBits(4);
                //Console.WriteLine(choose);

                //randomly choose an algorithm or number of bits to permutate
                switch (choose)
                {
                    case 0:
                        throwAway = Gaussian(false);
                        break;
                    case 1:
                        throwAway = Gaussian(true);
                        break;
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        throwAway = NextBits((byte)RangeInt(1, 31));
                        break;
                    case 7:
                        throwAway = NextUInt();
                        break;
                    case 8:
                    case 9:
                    case 10:
                        throwAway = MersenneTwister();
                        //Console.WriteLine(formatBits((uint)throwAway, 32) + " - MersenneTwister");
                        break;
                    case 11:
                        throwAway = MarsagliaKISS();
                        //Console.WriteLine(formatBits((uint)throwAway, 32) + " - MarsagliaKISS");
                        break;
                    case 12:
                        throwAway = LCG();
                        //Console.WriteLine(formatBits((uint)throwAway, 32) + " - LCG");
                        break;
                    case 13:
                        throwAway = LFSR();
                        //Console.WriteLine(formatBits((uint)throwAway, 32) + " - LFSR");
                        break;
                    case 14:
                        throwAway = MWC1(ref mwc1);
                        //Console.WriteLine((formatBits((uint)throwAway & 65535, 16)).PadLeft(32) + " - MWC1");
                        break;
                    case 15:
                        throwAway = MWC2(ref mwc2);
                        //Console.WriteLine((formatBits((uint)throwAway & 65535, 16)).PadLeft(32) + " - MWC2");
                        break;
                    default:
                        throw new Exception("internal error");
                }

                //Console.WriteLine(throwAway);

                //permutate counter
                GetShiftedTicks();
                //Console.WriteLine(counter.ToString("X8"));

                //Console.WriteLine();
                Thread.Sleep(sleep);
            }
        }

        ~MTRandom()
        {
            Dispose();
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
