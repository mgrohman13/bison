using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MattUtil
{
    [Serializable]
    public struct Point
    {
        private int x, y;
        public int X
        {
            get
            {
                return x;
            }
        }
        public int Y
        {
            get
            {
                return y;
            }
        }
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public override bool Equals(object obj)
        {
            if (obj is Point)
            {
                Point p2 = (Point)obj;
                return ( x == p2.x && y == p2.y );
            }
            return false;
        }
        public override int GetHashCode()
        {
            return GetHashCode(x, y);
        }
        public static int GetHashCode(int x, int y)
        {
            return ( ( x << 16 ) + ( x >> 16 ) + y + ( y < 0 ? 1073750016 : 0 ) + ( x < 0 ? 536887296 : 0 ) );
        }
        public override string ToString()
        {
            return string.Format("({0},{1})", x, y);
        }
        public static bool operator ==(Point a, Point b)
        {
            return ( a == null ? b == null : a.Equals(b) );
        }
        public static bool operator !=(Point a, Point b)
        {
            return !( a == b );
        }
    }
}
