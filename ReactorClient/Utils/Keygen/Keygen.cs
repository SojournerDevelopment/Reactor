using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ReactorClient.Utils.Keygen
{
    /// <summary>
    /// Generator for random data
    /// </summary>
    public static class Keygen
    {

        /// <summary>
        /// Generates a random string of data
        /// </summary>
        /// <param name="size">Amount of characters in the string</param>
        /// <returns>random string</returns>
        public static string GetUniqueKey(int size)
        {
            char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[size];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Generates a random array of bytes
        /// </summary>
        /// <param name="size">size to use as base. This is doubles</param>
        /// <returns>random byte array</returns>
        public static byte[] GetUniqueBytes(int size)
        {
            char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[size];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return Encoding.Unicode.GetBytes(result.ToString());
        }
    }
}
