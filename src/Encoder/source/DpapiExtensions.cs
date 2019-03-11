using System;
using System.Security.Cryptography;
using Text = System.Text;

namespace UGTS.Encoder
{
    /// <summary>
    /// Note that these methods are not particularly fast, on my machine they take about 250us per call.
    /// </summary>
	public static class DpapiExtensions
    {
        /// <summary>
        /// Uses the DPAPI with a per-user key to return the encrypted value of the text.  
        /// Throws a CryptographicException if you attempt to use this (regardless of DataProtectionScope) while impersonating another user (which often happens in the context of IIS)
        /// </summary>
        public static string XEncrypt(this string text, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            var b = Text.Encoding.UTF8.GetBytes("" + text);
            b = ProtectedData.Protect(b, null, scope);
            return b.XToHexadecimalString();
        }

        /// <summary>
        /// Uses the DPAPI with a either the per-user or per-machine key to return the decrypted value of the text.
        /// Throws a CryptographicException if you attempt to use this (regardless of DataProtectionScope) while impersonating another user (which often happens in the context of IIS)
        /// </summary>
        public static string XDecrypt(this string text)
        {
            var b = text.XDecryptToBytes();
            return new string(Text.Encoding.UTF8.GetChars(b));
        }

        private static byte[] XDecryptToBytes(this string text)
        {
            var b = text.XHexadecimalToBytes();
            // scope comes from the encrypted blob, not the parameter to this method
            // therefore, we can pass in whatever we want here, and it still works
            return ProtectedData.Unprotect(b, null, DataProtectionScope.CurrentUser);
        }
    }

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
