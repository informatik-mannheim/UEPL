using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ULCWebAPI.Models;
using ULCWebAPI.Security;

namespace ULCWebAPI.Attributes
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SignDataAttribute : ResultFilterAttribute
    {
        private HashAlgorithmName _hashAlgorithm = HashAlgorithmName.SHA256;

        /// <summary>
        /// 
        /// </summary>
        public SignDataAttribute()
        {
            this.Order = int.MinValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hashAlgorithm"></param>
        public SignDataAttribute(HashAlgorithmName hashAlgorithm) : this()
        {
            _hashAlgorithm = hashAlgorithm;
        }


        /// <summary>
        /// Signing outgoing data
        /// </summary>
        /// <param name="context"></param>
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var signatureService = context.HttpContext.RequestServices.GetService(typeof(ISignatureService)) as ISignatureService;

            var result = context.Result as OkObjectResult;

            if (result == null)
                return;

            var content = result.Value as string;

            var data = Encoding.UTF8.GetBytes(content);

            var sig_bytes = signatureService.SignData(data, _hashAlgorithm);
            var signature = Convert.ToBase64String(sig_bytes);

            context.HttpContext.Response.Headers.Add("Signature", new Microsoft.Extensions.Primitives.StringValues(signature));

            base.OnResultExecuting(context);
        }
    }
}
