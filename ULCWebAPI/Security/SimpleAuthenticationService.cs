using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ULCWebAPI.Security
{
    /// <summary>
    /// DEMO Authentication service, just serving sample username
    /// </summary>
    public class SimpleAuthenticationService : IAuthenticationService
    {
        /// <summary>
        /// Logs the user in and returns a new instance of ApplicationUser
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns></returns>
        public ApplicationUser Login(string username, string password)
        {
            if(username == "demo" && password == "demo")
            {
                return new ApplicationUser { UserName = "demo", EmployeeType = "staff", DisplayName = "Demo Account", Email = "demo@hs-mannheim.de" };
            }
            else if(username == "student" && password == "student")
            {
                return new ApplicationUser { UserName = "student", EmployeeType = "IB", DisplayName = "Student Account", Email = "student@stud.hs-mannheim.de" };
            }
            else
            {
                return null;
            }
        }
    }
}
