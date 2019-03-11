using System;

namespace UGTS.WPF
{
    public static class MathExtensions
    {
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
