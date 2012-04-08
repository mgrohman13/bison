using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MattUtil
{
    [Serializable]
    public struct PointD
    {
        public static int EQUALITY_PRECISION = 10;

        private double x, y;
        public double X
        {
            get
            {
                return x;
            }
        }
        public double Y
        {
            get
            {
                return y;
            }
        }
        public PointD(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public override bool Equals(object obj)
        {
            if (obj is PointD)
            {
                PointD p2 = (PointD)obj;
                return ( Math.Round(x, EQUALITY_PRECISION) == Math.Round(p2.x, EQUALITY_PRECISION)
                        && Math.Round(y, EQUALITY_PRECISION) == Math.Round(p2.y, EQUALITY_PRECISION) );
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Point.GetHashCode(GetInt(Math.Round(x, EQUALITY_PRECISION)), GetInt(Math.Round(y, EQUALITY_PRECISION)));
        }
        private static int GetInt(double val)
        {
            ulong a = (ulong)BitConverter.DoubleToInt64Bits(val);
            return (int)( a + ( a >> 32 ) );
        }
        public override string ToString()
        {
            return string.Format("({0:G3},{1:G3})", x, y);
        }
        public static bool operator ==(PointD a, PointD b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(PointD a, PointD b)
        {
            return !( a.Equals(b) );
        }
    }
}
