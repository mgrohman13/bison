using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MattUtil
{
    [Serializable]
    public struct PointD
    {
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
        public bool EqualsWithFudge(PointD obj, double fudgeFactor)
        {
            PointD p2 = (PointD)obj;
            return ( EqualsWithFudge(x, p2.x, fudgeFactor) && EqualsWithFudge(y, p2.y, fudgeFactor) );
        }
        private static bool EqualsWithFudge(double a, double b, double fudgeFactor)
        {
            return ( Math.Abs(a - b) < fudgeFactor );
        }
        public override bool Equals(object obj)
        {
            if (obj is PointD)
            {
                PointD p2 = (PointD)obj;
                return ( x == p2.x && y == p2.y );
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Point.GetHashCode(GetInt(x), GetInt(y));
        }
        private static int GetInt(double val)
        {
            ulong a = (ulong)BitConverter.DoubleToInt64Bits(val);
            return (int)( a ^ ( a >> 32 ) );
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
