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
        public double X, Y;

        public PointD(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public override bool Equals(object obj)
        {
            if (obj is PointD)
            {
                PointD p2 = (PointD)obj;
                return ( Math.Round(X, EQUALITY_PRECISION) == Math.Round(p2.X, EQUALITY_PRECISION)
                        && Math.Round(Y, EQUALITY_PRECISION) == Math.Round(p2.Y, EQUALITY_PRECISION) );
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Point.GetHashCode(Math.Round(X, EQUALITY_PRECISION).GetHashCode(), Math.Round(Y, EQUALITY_PRECISION).GetHashCode());
        }

        public override string ToString()
        {
            return string.Format("({0:G3},{1:G3})", X, Y);
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
