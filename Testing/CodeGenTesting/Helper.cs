using System.Security.Cryptography;

namespace CodeGenTesting
{
    internal static class Helper
    {
        private static readonly char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        internal static string RandomString()
            => RandomNumberGenerator.GetString(chars, 50);
    }
}
