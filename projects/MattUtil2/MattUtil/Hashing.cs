using System;
using System.Collections.Generic;
using System.Text;

namespace MattUtil
{
	public static class Hashing
	{
		public static int Hash(string value)
		{
			int extra = value.Length - value.TrimStart('~').Length;
			value = value.PadLeft(30 + extra, '~');

			uint result = 0x5C6C6A9C;

			int origHash = value.GetHashCode();
			int maxShift = 26 + origHash % 6,
				otherShift = origHash % 27,
				counter = origHash % maxShift;

			if (maxShift < 0) maxShift *= -1;
			if (otherShift < 0) otherShift *= -1;
			if (counter < 0) counter *= -1;

			foreach (char c in value)
			{
				uint val = Convert.ToUInt32(c) ^ (uint)c.GetHashCode();
				result += val << counter;
				result += val >> (32 - counter);
				if (++counter == maxShift)
					counter = (int)((result >> otherShift) % maxShift);
			}
			return (int)result;
		}
	}
}
