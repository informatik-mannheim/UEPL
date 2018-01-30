using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ULCWebAPI.Security
{
    interface ISignatureService
    {
        bool LoadCertificate(string filename, string password);

        byte[] SignData(byte[] data, HashAlgorithmName hashAlgorithmName);
        bool VerifyData(byte[] data, byte[] signature, HashAlgorithmName hashAlgorithmName);
    }
}
