using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using ProjectAPI.Models;
using Newtonsoft.Json;

namespace ProjectAPI.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationUser : ModelBase<int>
    {
        /// <summary>
        /// 
        /// </summary>
        public string DisplayName { get; internal set; }

        /// <summary>
        /// Could have one of the following values:
        /// - (string.Empty)
        /// - staff
        /// - IB
        /// - UIB
        /// - IMB
        /// </summary>
        public string EmployeeType { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public string Email { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public string LdapID { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; internal set; }
    }
}