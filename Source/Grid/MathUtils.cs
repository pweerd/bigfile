using System;

namespace Bitmanager.Grid {
	internal static class MathUtils
	{
		public static long Clipped (long v, long max) {
			return v < 0 ? 0 : (v <= max || max < 0 ? v : max);
		}
	}
}
