using System.Security.Cryptography;
using Text = System.Text;

namespace UGTS.Encoder
{
	public static class DpapiExtensions
    {
        public static string EncryptWithDpapi(this string text, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            var b = Text.Encoding.UTF8.GetBytes("" + text);
            b = ProtectedData.Protect(b, null, scope);
            return b.ToHexadecimalString();
        }

        public static string DecryptWithDpapi(this string text)
        {
            var b = text.DecryptToBytes();
            return new string(Text.Encoding.UTF8.GetChars(b));
        }

        private static byte[] DecryptToBytes(this string text)
        {
            var b = text.HexadecimalToBytes();
            return ProtectedData.Unprotect(b, null, DataProtectionScope.CurrentUser);
        }
    }
}
