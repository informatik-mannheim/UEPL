using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ULCWebAPI.Models
{
    /// <summary>
    /// User-Model to bind to a json response
    /// </summary>
    public class User
    {
        /// <summary>
        /// Users name (mostly ldap-username or mail)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Users password
        /// </summary>
        public string Password { get; set; }
    }
}
