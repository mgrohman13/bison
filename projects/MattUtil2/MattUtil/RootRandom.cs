using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MattUtil
{
    public class RootRandom
    {
        private static TrueInt zero = new TrueInt(0), one = new TrueInt(1), two = new TrueInt(2), four = new TrueInt(4);

        private TrueInt y;
        private TrueInt r;

        public RootRandom(TrueInt seed)
        {
            if (seed < two)
                seed = two;
            init(seed);
            if (this.r == zero)
                init(seed + one);

            bool val = Next(zero);
            while (val == Next(zero))
                ;
        }

        private void init(TrueInt seed)
        {
            this.r = zero;
            this.y = zero;

            bool first = false;
            for (int a = seed.Count ; --a > -1 ; )
                for (int b = 30 ; b > -1 ; b -= 2)
                {
                    uint c = ( seed[a] >> b ) & 3;
                    if (first || c != 0)
                    {
                        first = true;
                        Next(new TrueInt(c));
                    }
                }
        }

        public string Next()
        {
            string v = Next(zero) ? "1" : "0";
            //if (y > r)
            //    y -= r;
            //else if (y < r)
            //    r -= y;
            return v;
        }

        private bool Next(TrueInt a)
        {
            checked
            {
                TrueInt _2y = two * y;
                TrueInt _2y_sqr = Sqr(_2y);
                TrueInt _2y_1 = _2y + one;
                TrueInt _2y_1_sqr = Sqr(_2y_1);
                TrueInt _4r_a = ( four * r + a );

                bool b = _2y_1_sqr - _2y_sqr <= _4r_a;

                y = ( b ? _2y_1 : _2y );
                r = _4r_a - ( ( b ? _2y_1_sqr : _2y_sqr ) - _2y_sqr );

                return b;
            }
        }

        private TrueInt Sqr(TrueInt value)
        {
            return value * value;
        }

        public struct TrueInt
        {
            private const int SHIFT = 32;
            const long BORROW = 0x100000000;

            private readonly List<uint> vals;

            public TrueInt(TrueInt value)
            {
                this.vals = value.vals;
            }

            public TrueInt(List<uint> vals)
            {
                this.vals = vals;

                Trim();
            }

            public TrueInt(ulong value)
            {
                this.vals = new List<uint>();

                this.vals.Add((uint)value);
                this.vals.Add((uint)( value >> SHIFT ));

                Trim();
            }

            public TrueInt(int value)
            {
                this.vals = new List<uint>();

                this.vals.Add((uint)value);

                Trim();
            }

            private void Trim()
            {
                int count = Count;
                while (--count > -1 && vals[count] == 0)
                    vals.RemoveAt(count);
            }

            public int Count
            {
                get
                {
                    return vals.Count;
                }
            }

            public uint this[int i]
            {
                get
                {
                    return vals[i];
                }
                set
                {
                    bool trim = true;
                    while (i >= Count)
                    {
                        trim = false;
                        vals.Add(0);
                    }
                    vals[i] = value;
                    if (trim && i == Count - 1)
                        Trim();
                }
            }

            public override bool Equals(object obj)
            {
                if (obj is TrueInt)
                    return Compare(this, (TrueInt)obj) == CompVal.Equal;
                return false;
            }

            public static bool operator !=(TrueInt a, TrueInt b)
            {
                return Compare(a, b) != CompVal.Equal;
            }

            public static bool operator ==(TrueInt a, TrueInt b)
            {
                return Compare(a, b) == CompVal.Equal;
            }

            public static bool operator <(TrueInt a, TrueInt b)
            {
                return Compare(a, b) == CompVal.Less;
            }

            public static bool operator >(TrueInt a, TrueInt b)
            {
                return Compare(a, b) == CompVal.Greater;
            }

            public static bool operator <=(TrueInt a, TrueInt b)
            {
                return Compare(a, b) != CompVal.Greater;
            }

            public static bool operator >=(TrueInt a, TrueInt b)
            {
                return Compare(a, b) != CompVal.Less;
            }

            private enum CompVal : byte
            {
                Less,
                Equal,
                Greater,
            }

            private static CompVal Compare(TrueInt a, TrueInt b)
            {
                int count = a.Count;
                if (count < b.Count)
                    return CompVal.Less;
                else if (count > b.Count)
                    return CompVal.Greater;
                while (--count > -1)
                    if (a.vals[count] < b.vals[count])
                        return CompVal.Less;
                    else if (a.vals[count] > b.vals[count])
                        return CompVal.Greater;
                return CompVal.Equal;
            }

            public static TrueInt operator ++(TrueInt a)
            {
                return new TrueInt(a + one);
            }

            public static TrueInt operator --(TrueInt a)
            {
                return new TrueInt(a - one);
            }

            public static TrueInt operator +(TrueInt a, TrueInt b)
            {
                int count = Math.Max(a.Count, b.Count) + 1;
                List<uint> vals = new List<uint>(count);

                ulong carry = 0;
                for (int i = 0 ; i < count ; ++i)
                {
                    carry += GetUlong(a, i) + GetUlong(b, i);
                    vals.Add((uint)carry);
                    carry >>= SHIFT;
                }

                return new TrueInt(vals);
            }

            public static TrueInt operator -(TrueInt a, TrueInt b)
            {
                if (a < b)
                    throw new Exception();

                int count = a.Count;
                List<uint> vals = new List<uint>(count);

                bool borrow = false;
                for (int i = 0 ; i < count ; ++i)
                {
                    long value = GetLong(a, i) - GetLong(b, i);
                    if (borrow)
                        --value;
                    borrow = ( value < 0 );
                    if (borrow)
                        value += BORROW;
                    vals.Add((uint)value);
                }

                return new TrueInt(vals);
            }

            public static TrueInt operator *(TrueInt a, TrueInt b)
            {
                TrueInt vals = zero;

                for (int x = 0 ; x < a.Count ; ++x)
                {
                    ulong ax = GetUlong(a, x);
                    for (int y = 0 ; y < b.Count ; ++y)
                    {
                        TrueInt value = new TrueInt(ax * GetUlong(b, y));
                        for (int z = x + y ; --z > -1 ; )
                            value.vals.Insert(0, 0);
                        vals += value;

                        //ulong value = ax * GetUlong(b, y);
                        //ulong place = GetUlong(vals, x + y) + (ulong)( (uint)value );
                        //ulong next = ( value >> SHIFT ) + ( place >> SHIFT );
                        //place = (uint)place;
                        //for (int z = x + y ; true ; ++z)
                        //{
                        //    place += GetUlong(vals, z);
                        //    next += ( place >> SHIFT );
                        //    vals[z] = (uint)place;
                        //}
                    }
                }

                return new TrueInt(vals);
            }

            //public static TrueInt operator >>(TrueInt a, int b)
            //{
            //    if (b < 1 || b > 31)
            //        throw new Exception();

            //    int count = a.Count;
            //    List<uint> vals = new List<uint>(count);

            //    uint remain = 0;
            //    while (--count > -1)
            //    {
            //        uint value = GetUint(a, count);
            //        uint temp = ( value & ( uint.MaxValue >> ( 32 - b ) ) ) << ( 32 - b );
            //        value >>= b;
            //        value += remain;
            //        vals.Insert(0, (uint)value);
            //        remain = temp;
            //    }

            //    return new TrueInt(vals);
            //}

            private static ulong GetUlong(TrueInt trueInt, int index)
            {
                return GetUint(trueInt, index);
            }

            private static long GetLong(TrueInt trueInt, int index)
            {
                return GetUint(trueInt, index);
            }

            private static uint GetUint(TrueInt trueInt, int index)
            {
                if (trueInt.Count > index)
                    return trueInt.vals[index];
                return 0;
            }

            public override int GetHashCode()
            {
                int code = 0;
                for (int i = 0 ; i < Count ; ++i)
                    code ^= (int)vals[i];
                return code;
            }

            public override string ToString()
            {
                string res = "";
                for (int i = Count ; --i > -1 ; )
                    res += vals[i].ToString("X").PadLeft(8, '0');
                return res.TrimStart('0').PadLeft(1, '0');
            }
        }
    }
}
