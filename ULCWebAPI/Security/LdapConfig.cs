using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ULCWebAPI.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class LdapConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool UseSSL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BindDN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BindCredentials { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SearchBase { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SearchFilter { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AdminEmployeeType { get; set; }
    }
}
