using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ULCWebAPI.Security
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        ApplicationUser Login(string username, string password);
    }
}
