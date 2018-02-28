using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using ULCWebAPI.Models;
using Newtonsoft.Json;

namespace ULCWebAPI.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationUser : ModelBase<int>
    {
        internal static string adminType = "staff";

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsAdmin()
        {
            return EmployeeType == adminType;
        }

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
        public string PasswordHash { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public ICollection<UserLecture> UserLectures { get; internal set; } = new List<UserLecture>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"User {DisplayName} with username {UserName} and email {Email} as {EmployeeType}";
        }
    }
}