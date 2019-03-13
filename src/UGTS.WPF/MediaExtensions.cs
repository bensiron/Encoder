using System;
using System.Windows.Media;

namespace UGTS.WPF
{
    public static class MediaExtensions
	{
		/// <summary>
		/// returns a new color which is the fraction of the way from the first to the second color
		/// </summary>
		public static Color XBlend(this Color c1, Color c2, double fraction)
		{
			var c = default(Color);
			c.A = Convert.ToByte(Blend(c1.A, c2.A, fraction).XLimit(0, 255));
			c.R = Convert.ToByte(Blend(c1.R, c2.R, fraction).XLimit(0, 255));
			c.G = Convert.ToByte(Blend(c1.G, c2.G, fraction).XLimit(0, 255));
			c.B = Convert.ToByte(Blend(c1.B, c2.B, fraction).XLimit(0, 255));
			return c;
		}

		private static double Blend(double a, double b, double fraction)
		{
			return a + (b - a) * fraction;
		}

        /// <summary>
        /// Returns value limited by the min and max values (min if value is less than min, and max if value if greater than max).
        /// </summary>
        public static TComparable XLimit<TComparable>(this TComparable val, TComparable minVal, TComparable maxVal)
            where TComparable : IComparable
        {
            if (val.CompareTo(minVal) < 0) return minVal;
            return val.CompareTo(maxVal) > 0 ? maxVal : val;
        }
    }
}
