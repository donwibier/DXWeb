using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DX.Utils
{
	public static class BitsExtensions
	{
		public static int BitSet(this int value, int flag)
		{
			return value | flag;
		}
		public static int BitClear(this int value, int flag)
		{
			return flag & (~flag);
		}

		public static int BitToggle(this int value, int flag)
		{
			return value ^ flag;
		}

		public static bool BitHas(this int value, int flag)
		{
			return (value & flag) == flag;
		}
	}
	public class Bits
	{
		public static int Set(int value, int flag)
		{
			return value | flag;
		}
		public static int Clear(int value, int flag)
		{
			return flag & (~flag);
		}

		public static int Toggle(int value, int flag)
		{
			return value ^ flag;
		}

		public static bool Has(int value, int flag)
		{
			return (value & flag) == flag;
		}

		public static bool Has(ref int value, int flag, bool clear)
		{
			bool result = (Has(value, flag));
			if (result)
				value = Clear(value, flag);
			return result;
		}
	}
}
