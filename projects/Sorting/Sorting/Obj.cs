using System;
using System.Collections.Generic;
using System.Text;

namespace Sorting
{
	//this class wraps a uint and is used for sorting
	class Obj
	{
		//Value should never be referenced from a sort
		public uint Value;
		public Obj(uint value, RefreshDelegate Compared)
		{
			this.Value = value;
			this.Compared = Compared;
		}

		private RefreshDelegate Compared;

		//these should be the only operations used in sorts
		public static bool operator >(Obj a, Obj b)
		{
			a.Compared();
			return a.Value > b.Value;
		}
		public static bool operator <(Obj a, Obj b)
		{
			a.Compared();
			return a.Value < b.Value;
		}
		public static int Compare(Obj a, Obj b)
		{
			a.Compared();
			if (a.Value > b.Value)
				return 1;
			if (a.Value < b.Value)
				return -1;
			return 0;
		}

		//at the moment Im not allowing any standard equality comparisons
		//why? I dont know
		//but if you want to test equality from a sort, use Obj.Compare
		//to test equality outside of a sort, you can also use Obj.Value
		public static bool operator ==(Obj a, Obj b)
		{
			throw new InvalidOperationException();
		}
		public static bool operator !=(Obj a, Obj b)
		{
			throw new InvalidOperationException();
		}
		public override bool Equals(object obj)
		{
			throw new InvalidOperationException();
		}
		public override int GetHashCode()
		{
			throw new InvalidOperationException();
		}

		//for debug purposes mostly
		public override string ToString()
		{
			return Value.ToString();
		}
	}
}
