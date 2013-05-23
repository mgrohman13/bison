using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MattUtil
{
    [Serializable]
    public struct PointS
    {
        public short X, Y;

        public PointS(short x, short y)
        {
            this.X = x;
            this.Y = y;
        }

        public override bool Equals(object obj)
        {
            if (obj is PointS)
            {
                PointS p2 = (PointS)obj;
                return ( X == p2.X && Y == p2.Y );
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Point.GetHashCode(X, Y);
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", X, Y);
        }

        public static bool operator ==(PointS a, PointS b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(PointS a, PointS b)
        {
            return !( a.Equals(b) );
        }
    }
}
