namespace UGTS.Encoder
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns True if the given string is Nothing or blank.
        /// </summary>
        public static bool XIsBlank(this string s)
        {
            if (string.IsNullOrEmpty(s)) return true;
            if (!char.IsWhiteSpace(s[0])) return false;
            return s.Trim().Length == 0;
        }
    }
}
