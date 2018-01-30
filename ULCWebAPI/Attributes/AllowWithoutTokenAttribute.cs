using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ULCWebAPI.Attributes
{
    /// <summary>
    /// Disables Token Validation
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AllowWithoutTokenAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// List of allowed http methods to bypass token based security
        /// </summary>
        public List<string> AllowedMethods = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        public AllowWithoutTokenAttribute() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowedMethods">List of allowed http methods to bypass token based security</param>
        public AllowWithoutTokenAttribute(string[] allowedMethods)
        {
            AllowedMethods.AddRange(allowedMethods);
        }
    }
}
