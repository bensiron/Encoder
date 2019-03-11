using System.Security;
using System.Security.Cryptography;
using UGTS.Encoding;
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
        public static string XEncrypt(this string text, DataProtectionScope scope = DataProtectionScope.CurrentUser, IByteConverter converter = null)
        {
            converter = converter ?? new Base16ByteConverter();
            var b = Text.Encoding.UTF8.GetBytes("" + text);
            b = ProtectedData.Protect(b, null, scope);
            return converter.ToString(b);
        }

        /// <summary>
        /// Uses the DPAPI with a either the per-user or per-machine key to return the decrypted value of the text.
        /// Throws a CryptographicException if you attempt to use this (regardless of DataProtectionScope) while impersonating another user (which often happens in the context of IIS)
        /// </summary>
        public static string XDecrypt(this string text, IByteConverter converter = null)
        {
            var b = text.XDecryptToBytes();
            return new string(Text.Encoding.UTF8.GetChars(b));
        }

        /// <summary>
        /// Uses the DPAPI with a either the per-user or per-machine key to return the decrypted value of the text as a secure string
        /// Throws a CryptographicException if you attempt to use this (regardless of DataProtectionScope) while impersonating another user (which often happens in the context of IIS)
        /// </summary>
        public static SecureString XDecryptAsSecureString(this string text, IByteConverter converter = null)
        {
            var s = new SecureString();
            var b = text.XDecryptToBytes(converter);
            var chars = Text.Encoding.UTF8.GetChars(b);
            chars.XForEach(c => s.AppendChar(c));
            b.XClear();
            chars.XClear();
            return s;
        }

        private static byte[] XDecryptToBytes(this string text, IByteConverter converter = null)
        {
            converter = converter ?? new Base16ByteConverter();
            var b = converter.ToBytes(text);
            // scope comes from the encrypted blob, not the parameter to this method
            // therefore, we can pass in whatever we want here, and it still works
            return ProtectedData.Unprotect(b, null, DataProtectionScope.CurrentUser);
        }
    }
}
