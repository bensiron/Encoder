using System;

namespace UGTS.Encoder
{
    public static class HexadecimalExtensions
    {
        private const string HexadecimalCharacters = "0123456789ABCDEF";
        /// <summary>
        /// Converts the byte array to an uppercase hexadecimal string, returns a blank string if the byte array is Nothing or zero length.
        /// </summary>
        public static string XToHexadecimalString(this byte[] b)
        {
            if ((b == null) || (b.Length == 0)) return "";
            var r = new char[b.Length * 2];
            var pos = 0;
            for (var i = 0; i <= b.Length - 1; i++)
            {
                var y = b[i];
                r[pos] = HexadecimalCharacters[y / 16];
                r[pos + 1] = HexadecimalCharacters[y & 15];
                pos += 2;
            }
            return new string(r);
        }

        /// <summary>
        /// converts a byte into a two digit hexadecimal string
        /// </summary>
        public static string XToHexadecimalString(this byte b)
        {
            return HexadecimalCharacters[b / 16] + HexadecimalCharacters[b & 15].ToString();
        }

        /// <summary>
        /// Converts a hexadecimal string (case-insensitive) to a byte array.  Throws an exception if the byte array has an odd number of bytes or invalid characters.
        /// </summary>
        public static byte[] XHexadecimalToBytes(this string s)
        {
            s = "" + s;
            if ((s.Length & 1) == 1) throw new Exception("The hexadecimal string had an odd number of characters, cannot convert to Byte array");
            var pos = 0;
            var r = new byte[s.Length >> 1];
            for (var i = 0; i <= s.Length - 2; i += 2)
            {
                var i1 = s[i].XHexadecimalToDigit();
                var i2 = s[i + 1].XHexadecimalToDigit();
                if ((i1 < 0) || (i2 < 0)) throw new Exception("Invalid hexadecimal digit pair at position " + i + ", character codes (" + i1 + ", " + i2 + ")");
                r[pos] = Convert.ToByte(i1 * 16 + i2);
                pos++;
            }
            return r;
        }

        /// <summary>
        /// Converts a hexadecimal character to a digit 0-15.  Returns -1 on failure.
        /// </summary>
        public static int XHexadecimalToDigit(this char c)
        {
            var i = (int)c;
            if ((i >= 0x30) & (i <= 0x39)) return i - 48;
            if (i >= 96) i -= 32;
            if ((i >= 65) & (i <= 70)) return i - 55;
            return -1;
        }

        /// <summary>
        /// True if the character is a valid hexadecimal digit.
        /// </summary>
        public static bool XIsHexadecimal(this char c)
        {
            return c.XHexadecimalToDigit() >= 0;
        }

        /// <summary>
        /// Converts a integer value to a two or four digit hexadecimal string, returns a blank string on error
        /// </summary>
        public static string XToHexadecimalString(this int i)
        {
            if ((i < 0) || (i >= 0x10000)) return "";
            return i >= 0x100 ? Convert.ToByte(i / 0x100).XToHexadecimalString() : "" + Convert.ToByte(i + 0xff).XToHexadecimalString();
        }
    }
}