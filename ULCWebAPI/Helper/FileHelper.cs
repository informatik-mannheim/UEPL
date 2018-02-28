using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ULCWebAPI.Helper
{
    /// <summary>
    /// 
    /// </summary>
    public class FileHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hashAlogrithm"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string ComputeHash(HashAlgorithm hashAlogrithm, string filename)
        {
            string hashValue;

            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                var byteHash = hashAlogrithm.ComputeHash(fs);
                var stringHash = Convert.ToBase64String(byteHash);
                hashValue = stringHash;
            }

            return hashValue;
        }
    }
}
