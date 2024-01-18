using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt;
using System.Security.Cryptography;

namespace Iotapass.Services
{
    internal static class IotaCryptService
    {
        /// <summary>
        /// Returns true if input string s and hashed pass are a match, returns false otherwise.
        /// </summary>
        internal static bool CompareHash(string s, string p)
        {
            if (BCrypt.Net.BCrypt.EnhancedVerify(s, p))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a hash of the input string.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        internal static string GetHash(string s)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(s, 13);
        }
        
        // EN/DE cryptography
        internal static string E(string s, string email)
        {
            string hash = email;
            byte[] vs = UTF8Encoding.UTF8.GetBytes("&"+s);
            using (MD5 mD5 = MD5.Create())
            {
                byte[] keys = mD5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash));
                using (TripleDES triple = TripleDES.Create())
                {
                    triple.Key = keys;
                    triple.Mode = CipherMode.ECB;
                    triple.Padding = PaddingMode.PKCS7;
                    // { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 }
                    ICryptoTransform crypto = triple.CreateEncryptor();
                    byte[] results = crypto.TransformFinalBlock(vs, 0, vs.Length);
                    return Convert.ToBase64String(results, 0, results.Length);
                }
            }
        }
        internal static string D(string s, string email)
        {
            string hash = email;
            byte[] vs = Convert.FromBase64String(s);
            using (MD5 mD5 = MD5.Create())
            {
                byte[] keys = mD5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash));
                using (TripleDES triple = TripleDES.Create())
                {
                    triple.Key = keys;
                    triple.Mode = CipherMode.ECB;
                    triple.Padding = PaddingMode.PKCS7;
                    // { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 }
                    ICryptoTransform crypto = triple.CreateDecryptor();
                    byte[] results = crypto.TransformFinalBlock(vs, 0, vs.Length);
                    return UTF8Encoding.UTF8.GetString(results).Substring(1);
                }
            }
        }
        internal static string GetSHA1Hash(string s)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                return Convert.ToHexString(sha1.ComputeHash(Encoding.UTF8.GetBytes(s)));
            }
        }
    }
}
