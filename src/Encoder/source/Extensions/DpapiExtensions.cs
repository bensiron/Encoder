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
        public static string EncryptWithDpapi(this string text, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            var b = Text.Encoding.UTF8.GetBytes("" + text);
            b = ProtectedData.Protect(b, null, scope);
            return b.ToHexadecimalString();
        }

        /// <summary>
        /// Uses the DPAPI with a either the per-user or per-machine key to return the decrypted value of the text.
        /// Throws a CryptographicException if you attempt to use this (regardless of DataProtectionScope) while impersonating another user (which often happens in the context of IIS)
        /// </summary>
        public static string DecryptWithDpapi(this string text)
        {
            var b = text.DecryptToBytes();
            return new string(Text.Encoding.UTF8.GetChars(b));
        }

        private static byte[] DecryptToBytes(this string text)
        {
            var b = text.HexadecimalToBytes();
            // scope comes from the encrypted blob, not the parameter to this method
            // therefore, we can pass in whatever we want here, and it still works
            return ProtectedData.Unprotect(b, null, DataProtectionScope.CurrentUser);
        }
    }
}
