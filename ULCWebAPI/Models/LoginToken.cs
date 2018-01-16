using ProjectAPI.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectAPI.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class LoginToken : ModelBase<int>
    {
        /// <summary>
        /// 
        /// </summary>
        public LoginToken()
        {
            Valid = DateTime.Now.Add(TimeSpan.FromMinutes(30));
        }

        /// <summary>
        /// 
        /// </summary>
        public ApplicationUser User { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime Valid { get; set; }

        public override string ToString()
        {
            return $"Username: {User?.UserName}, Token: {Token}, Valid: {Valid}";
        }
    }
}
