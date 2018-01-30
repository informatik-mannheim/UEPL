using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ULCWebAPI.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class RequestResponseExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public static string GetToken(this HttpRequest Request)
        {
            string token = string.Empty;

            if (Request.Headers.ContainsKey("Token"))
            {
                if (Request.Headers["Token"].Count > 0)
                {
                    token = Request.Headers["Token"][0];
                }
            }

            return token;
        }
    }
}
