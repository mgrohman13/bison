using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ideas
{
    //http://www.cs.ucl.ac.uk/staff/d.jones/GoodPracticeRNG.pdf
    //https://groups.google.com/group/sci.math.num-analysis/msg/eb4ddde782b17051?hl=en

    class _1
    {
        //seed
        //y must never be set to zero!
        // Also avoid setting z=c=0!
        static uint x = 123456789, y = 362436000, z = 521288629, c = 7654321;

        uint KISS()
        {
            const ulong a = 698769069;
            x = 69069 * x + 12345;
            y ^= ( y << 13 );
            y ^= ( y >> 17 );
            y ^= ( y << 5 );
            ulong t = a * z + c;
            c = (uint)( t >> 32 );
            return x + y + ( z = (uint)t );
        }
    }

    /* Public domain code for JKISS RNG */
    class _2
    {
        //seed
        static uint x = 123456789, y = 987654321, z = 43219876, c = 6543217;

        uint JKISS()
        {
            x = 314527869 * x + 1234567;
            y ^= y << 5;
            y ^= y >> 7;
            y ^= y << 22;
            ulong t = 4294584393 * z + c;
            c = (uint)( t >> 32 );
            z = (uint)t;
            return x + y + z;
        }
    }

    /* Implementation of a 32-bit KISS generator which uses no multiply instructions */
    class _3
    {
        static uint x = 123456789, y = 234567891, z = 345678912, w = 456789123;
        bool c = false;

        uint JKISS32()
        {
            int t;
            y ^= ( y << 5 );
            y ^= ( y >> 7 );
            y ^= ( y << 22 );
            t = (int)( z + w + ( c ? 1 : 0 ) );
            z = w;
            c = ( t < 0 );
            w = (uint)t & 2147483647;
            x += 1411392427;
            return x + y + w;
        }
    }

    /* Public domain code for JLKISS RNG - long period KISS RNG with 64-bit operations */
    class _4
    {
        //seed
        //Do not set y=0!
        //Avoid z=c=0!
        ulong x = 123456789123, y = 987654321987;
        uint z = 43219876, c = 6543217;

        uint JLKISS()
        {
            ulong t;

            x = 1490024343005336237 * x + 123456789;
            y ^= y << 21;
            y ^= y >> 17;
            y ^= y << 30;
            t = 4294584393 * z + c;
            c = (uint)( t >> 32 );
            z = (uint)t;
            return (uint)( x >> 32 ) + (uint)y + z; /* Return 32-bit result */
        }
    }

    /* Public domain code for JLKISS64 RNG - long period KISS RNG producing 64-bit results */
    class _5
    {
        //seed
        //Do not set y=0!
        ulong x = 123456789123, y = 987654321987;
        uint z1 = 43219876, c1 = 6543217, z2 = 21987643, c2 = 1732654;

        ulong JLKISS64()
        {
            ulong t;

            x = 1490024343005336237 * x + 123456789;
            y ^= y << 21;
            y ^= y >> 17;
            y ^= y << 30;
            t = 4294584393 * z1 + c1;
            c1 = (uint)( t >> 32 );
            z1 = (uint)t;
            t = 4246477509 * z2 + c2;
            c2 = (uint)( t >> 32 );
            z2 = (uint)t;
            //Return 64-bit result
            return x + y + z1 + ( (ulong)z2 << 32 );
        }
    }

    /* MWC256 from Usenet posting by G. Marsaglia - Period 2^8222 */
    class _6
    {
        static uint[] Q = new uint[256];
        static uint c = 362436;
        uint MWC256()
        {
            ulong t;
            byte i = 255;
            t = 809430660 * Q[++i] + c;
            c = (uint)( t >> 32 );
            return ( Q[i] = (uint)t );
        }
    }

    /* CMWC4096 from Usenet posting by G. Marsaglia - Period 2^131086 */
    class _7
    {
        static uint[] Q = new uint[4096];
        static uint c = 362436;
        uint CMWC4096()
        {
            ulong t;
            uint x;
            uint i = 4095;
            i = ( i + 1 ) & 4095;
            t = 18782 * Q[i] + c;
            c = (uint)( t >> 32 );
            x = (uint)( t + c );
            if (x < c)
            {
                x++;
                c++;
            }
            return ( Q[i] = 0xFFFFFFFEU - x );
        }
    }

    /* Super KISS based on Usenet posting by G. Marsaglia - Period 54767 * 2^1337279 */
    class _8
    {
        const uint len = 41790;

        static uint[] Q = new uint[len];
        static uint indx = len, carry = 362436, xcng = 1236789, xs = 521288629;
        /* Fill Q array with random u32-bit ints and return first element */
        uint refill()
        {
            int i;
            ulong t;
            for (i = 0 ; i < len ; i++)
            {
                t = 7010176 * Q[i] + carry;
                carry = (uint)( t >> 32 );
                Q[i] = (uint)( ~t );
            }
            indx = 1;
            return ( Q[0] );
        }
        /* Return 32-bit random integer – calls refill() when needed */
        uint SuperKISS()
        {
            xcng = 69069 * xcng + 123;
            xs ^= xs << 13;
            xs ^= xs >> 17;
            xs ^= xs >> 5;
            return ( indx < len ? Q[indx++] : refill() ) + xcng + xs;
        }
    }

    class _9
    {
        //seed
        static ulong z = 362436069, w = 521288629, jsr = 123456789, jcong = 380116160;
        static ulong a = 224466889, b = 7584631;
        static ulong[] t = new ulong[256];

        //???non-seed???
        static ulong x = 0, y = 0;
        bool bro;
        static byte c = 0;

        ulong znew()
        {
            return ( z = 36969 * ( z & 65535 ) + ( z >> 16 ) );
        }
        ulong wnew()
        {
            return ( w = 18000 * ( w & 65535 ) + ( w >> 16 ) );
        }
        ulong MWC()
        {
            return ( ( znew() << 16 ) + wnew() );
        }
        ulong SHR3()
        {
            jsr ^= ( jsr << 17 );
            jsr ^= ( jsr >> 13 );
            return ( jsr ^= ( jsr << 5 ) );
        }
        ulong CONG()
        {
            return ( jcong = 69069 * jcong + 1234567 );
        }
        ulong FIB()
        {
            b = a + b;
            return ( a = ( b - a ) );
        }
        ulong KISS()
        {
            return ( ( MWC() ^ CONG() ) + SHR3() );
        }
        ulong LFIB4()
        {
            c++;
            return ( t[c] = ( t[c] + t[(byte)( c + 58 )] + t[(byte)( c + 119 )] + t[(byte)( c + 178 )] ) );
        }
        ulong SWB()
        {
            c++;
            bro = ( x < y );
            return ( t[c] = ( ( x = t[(byte)( c + 34 )] ) - ( y = ( t[(byte)( c + 19 )] + (ulong)( bro ? 1 : 0 ) ) ) ) );
        }

        /* Example procedure to set the table, using KISS: */
        void settable(ulong i1, ulong i2, ulong i3, ulong i4, ulong i5, ulong i6)
        {
            int i;
            z = i1;
            w = i2;
            jsr = i3;
            jcong = i4;
            a = i5;
            b = i6;
            for (i = 0 ; i < 256 ; i = i + 1)
                t[i] = KISS();
        }
    }

    class _10
    {
        ulong x = 123456789, y = 362436000, z = 521288629, c = 7654321;
        ulong KISS()
        {
            ulong t, a = 698769069;
            x = 69069 * x + 12345;
            y ^= ( y << 13 );
            y ^= ( y >> 17 );
            y ^= ( y << 5 );
            t = a * z + c;
            c = ( t >> 32 );
            return x + y + ( z = t );
        }
    }

    class _11
    {
        static ulong[] Q = new ulong[256];
        static ulong c = 362436;
        ulong MWC256()
        {
            ulong t, a = 1540315826;
            ulong x;
            byte i = 255;
            t = a * Q[++i] + c;
            c = ( t >> 32 );
            x = t + c;
            if (x < c)
            {
                x++;
                c++;
            }
            return ( Q[i] = x );
        }
    }

    class _12
    {
        static ulong[] Q = new ulong[4096];
        static ulong c = 362436;
        ulong CMWC4096()
        {
            ulong t, a = 18782;
            ulong i = 4095;
            ulong x, r = 0xfffffffe;
            i = ( i + 1 ) & 4095;
            t = a * Q[i] + c;
            c = ( t >> 32 );
            x = t + c;
            if (x < c)
            {
                x++;
                c++;
            }
            return ( Q[i] = r - x );
        }
    }

    class _13
    {
        //y can be any 32-bit integer not 0, 
        //z and w any 31-bit integers not multiples of 7559 
        //c can be 0 or 1. 
        static uint x = 123456789, y = 362436069, z = 21288629, w = 14921776;
        bool c = false;
        ulong KISS()
        {
            x += 0x208A28AD;
            y ^= ( y << 13 );
            y ^= ( y >> 17 );
            y ^= ( y << 5 );
            uint t = z + w + (uint)( c ? 1 : 0 );
            z = w;
            c = ( ( t >> 31 ) == 1 );
            w = t & 0x7FFFFFFF;
            return ( x + y + w );
        }
    }

    public class _14
    {
        private const int len = 41790;
        private const ulong mlt = 7010176;

        private uint[] mwc;
        private ushort idx;
        private uint xs, lcg, carry;

        public _14(params uint[] seed)
        {
            Seed(seed);
        }

        //Marsaglia's KISS (Keep It Simple, Stupid)
        public uint Next()
        {
            return MWC() + XS() + LCG();
        }

        public void Seed(params uint[] seed)
        {
            //init fields
            mwc = new uint[len];
            idx = 32940;
            xs = 3366698789;
            lcg = 1025196070;
            carry = 1213447760;

            //null, empty, and 0 are all handled as different seeds
            uint a = 0;
            if (seed == null || seed.Length == 0)
            {
                if (seed == null)
                    carry += Next();
                carry += Next();
                seed = new uint[] { 0 };
            }
            lcg += (uint)seed.Length;

            //seed inital values
            xs += GetSeed(seed, ref a);
            lcg += GetSeed(seed, ref a);
            uint b = GetSeed(seed, ref a);
            carry += b;
            idx += (ushort)( b + ( b >> 16 ) );
            Validate();

            //seed a temporary MWC
            for (b = 0 ; b < len ; b++)
                mwc[b] = XS() + LCG() + GetSeed(seed, ref a) + a;
            //use any remaining seed values
            while (a != seed.Length)
                mwc[Next() % len] += GetSeed(seed, ref a);

            //seed real MWC using the temporary KISS so all seed values are represented in all state values
            ushort c = idx;
            idx = len;
            Refill();
            for (b = 0 ; b < len ; b++)
                mwc[b] = Next();
            idx = c;

            //seed XS and LCG using MWC
            xs += MWC();
            lcg += MWC();
            Validate();
        }
        private uint GetSeed(uint[] seed, ref uint a)
        {
            if (a == seed.Length)
            {
                a = 0;
                XS();
            }
            return seed[a++];
        }
        private void Validate()
        {
            idx = (ushort)( idx % ( len + 1 ) );
            if (xs == 0)
                xs = uint.MaxValue;
            carry = (uint)( carry % mlt );
        }

        //Marsaglia's Multiply-With-Carry
        public uint MWC()
        {
            return ( idx < len ? mwc[idx++] : Refill() );
        }
        private uint Refill()
        {
            for (int i = 0 ; i < len ; i++)
            {
                ulong t = mlt * mwc[i] + carry;
                carry = (uint)( t >> 32 );
                mwc[i] = (uint)( ~t );
            }
            idx = 1;
            return mwc[0];
        }
        //XorShift
        public uint XS()
        {
            xs ^= xs << 13;
            xs ^= xs >> 17;
            return ( xs ^= xs >> 5 );
        }
        //Linear Congruential Generator
        public uint LCG()
        {
            return ( lcg = 69609 * lcg + 123 );
        }
    }

    public class _15
    {
        private const uint len = 0x400000;
        private const uint msk = len - 1;

        private static uint[] Q = new uint[len];
        private static uint carry = 0;
        private static uint j = msk;

        private static uint cng = 123456789, xs = 362436069;

        public static uint b32MWC()
        {
            uint t, x;
            j = ( j + 1 ) & msk;
            x = Q[j];
            t = ( x << 28 ) + carry;
            carry = ( x >> 4 ) - (uint)( t < x ? 1 : 0 );
            return ( Q[j] = t - x );
        }

        public static uint CNG()
        {
            return ( cng = 69069 * cng + 13579 );
        }
        public static uint XS()
        {
            xs ^= ( xs << 13 );
            xs ^= ( xs >> 17 );
            return ( xs ^= ( xs << 5 ) );
        }

        public static uint KISS()
        {
            return ( b32MWC() + CNG() + XS() );
        }

        public static void test()
        {
            /* First seed Q[] with CNG+XS:    */
            for (int i = 0 ; i < len ; i++)
                Q[i] = CNG() + XS();
            /* Then generate 10^9 b32MWC()s */
            uint x = 0;
            for (int i = 0 ; i < 1000000000 ; i++)
                x = b32MWC();
            Console.WriteLine("Does x=2769813733 ?\n     x={0}\n", x);
            /* followed by 10^9 KISSes:   */
            for (int i = 0 ; i < 1000000000 ; i++)
                x = KISS();
            Console.WriteLine("Does x=3545999299 ?\n     x={0}\n", x);
        }
    }

    public class _16
    {
        private const ulong len = 0x200000;
        private const ulong msk = len - 1;

        private static ulong[] Q = new ulong[len];
        private static ulong carry = 0;
        private static ulong j = msk;

        private static ulong cng = 123456789987654321, xs = 362436069362436069;

        public static ulong B64MWC()
        {
            ulong t, x;
            j = ( j + 1 ) & msk;
            x = Q[j];
            t = ( x << 28 ) + carry;
            carry = ( x >> 36 ) - (ulong)( t < x ? 1 : 0 );
            return ( Q[j] = t - x );
        }

        public static ulong CNG()
        {
            return ( cng = 6906969069 * cng + 13579 );
        }
        public static ulong XS()
        {
            xs ^= ( xs << 13 );
            xs ^= ( xs >> 17 );
            return ( xs ^= ( xs << 43 ) );
        }

        public static ulong KISS()
        {
            return ( B64MWC() + CNG() + XS() );
        }

        public static void test()
        {
            /* First seed Q[] with CNG+XS:    */
            for (ulong i = 0 ; i < len ; i++)
                Q[i] = CNG() + XS();
            /* Then generate 10^9 B64MWC()s */
            ulong x = 0;
            for (int i = 0 ; i < 1000000000 ; i++)
                x = B64MWC();
            Console.WriteLine("Does x=13596816608992115578 ?\n     x={0}\n", x);
            /* followed by 10^9 KISSes:   */
            for (int i = 0 ; i < 1000000000 ; i++)
                x = KISS();
            Console.WriteLine("Does x=5033346742750153761 ?\n     x={0}\n", x);
        }
    }
}