using System.Security.Cryptography;
using System.Text;

namespace UnlayerCache.API.Util
{
    public class Hash
    {
        private const string Key = "1UHjPgXZzXCGkhxV2QCnooyJexUzvJr9";

        private static readonly Encoding Encoding = Encoding.UTF8;

        public static string HashString(string data)
        {
            return System.Convert.ToBase64String(HmacSha256(data, Key));
        }
        
        private static byte[] HmacSha256(string data, string key)
        {
            using (var hmac = new HMACSHA256(Encoding.GetBytes(key)))
            {
                return hmac.ComputeHash(Encoding.GetBytes(data));
            }
        }
    }
}