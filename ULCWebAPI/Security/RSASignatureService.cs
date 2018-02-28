using ULCWebAPI.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ULCWebAPI.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class RSASignatureService : ISignatureService
    {
        private bool loaded = false;
        private X509Certificate2 _certificate;
        private RSA _rsaPrivateProvider;
        private RSA _rsaPublicProvider;

        /// <summary>
        /// 
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enabled"></param>
        public RSASignatureService(bool enabled = true)
        {
            Enabled = enabled;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="password"></param>
        public bool LoadCertificate(string filename, string password)
        {
            try
            {
                _certificate = new X509Certificate2(filename, password);
                _rsaPrivateProvider = _certificate.GetRSAPrivateKey();
                _rsaPublicProvider = _certificate.GetRSAPublicKey();
                loaded = true;
                return true;
            }
            catch(Exception e)
            {
                Tracer.TraceMessage(e);
                throw e;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="hashAlgorithmName"></param>
        /// <returns></returns>
        public byte[] SignData(byte[] data, HashAlgorithmName hashAlgorithmName)
        {
            try
            {
                if(!loaded)
                {
                    Tracer.TraceMessage("The certificate is not loaded, does not exist or contains no private key!");
                    return null;
                }
                else
                {
                    return _rsaPrivateProvider.SignData(data, hashAlgorithmName, RSASignaturePadding.Pkcs1);
                }
            }
            catch(Exception e)
            {
                Tracer.TraceMessage(e);
                throw e;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="signature"></param>
        /// <param name="hashAlgorithmName"></param>
        /// <returns></returns>
        public bool VerifyData(byte[] data, byte[] signature, HashAlgorithmName hashAlgorithmName)
        {
            try
            {
                if (!loaded)
                {
                    Tracer.TraceMessage("The certificate is not loaded, does not exist or contains no private key!");
                    return false;
                }
                else
                {
                    return _rsaPublicProvider.VerifyData(data, signature, hashAlgorithmName, RSASignaturePadding.Pkcs1);
                }
            }
            catch(Exception e)
            {
                Tracer.TraceMessage(e);
                throw e;
            }
        }
    }
}
