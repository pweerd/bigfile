using System;

namespace Bitmanager.Grid {
	internal static class MathUtils
	{
		public static int Clip(int min, int value, int max)
		{
			return Math.Max(min, Math.Min(value, max));
		}

		public static double Clip(double min, double value, double max)
		{
			return Math.Max(min, Math.Min(value, max));
		}

		public static long Clipped (long v, long max) {
			return v < 0 ? 0 : (v <= max || max < 0 ? v : max);
		}
	}
}
