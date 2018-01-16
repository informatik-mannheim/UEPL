using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProjectAPI.Extensions;
using ProjectAPI.Helper;
using ProjectAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectAPI.Attributes
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TokenPermissionRequiredAttribute : ActionFilterAttribute
    {
        private string requiredRole = string.Empty;
        private List<string> usernames = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        public TokenPermissionRequiredAttribute(string requiredRole = "")
        {
            this.requiredRole = requiredRole;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usernames"></param>
        /// <param name="requiredRole"></param>
        public TokenPermissionRequiredAttribute(string[] usernames, string requiredRole = "")
        {
            this.usernames.AddRange(usernames);
            this.requiredRole = requiredRole;
        }

        /// <summary>
        /// Validates the permitted token and grants or denies the access to an action
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                var allowAttributes = context.Filters.OfType<AllowWithoutTokenAttribute>();

                foreach(var allowAttribute in allowAttributes)
                {
                    if (allowAttribute != null)
                    {
                        if (allowAttribute.AllowedMethods.Count > 0)
                        {
                            if (allowAttribute.AllowedMethods.Contains(context.HttpContext.Request.Method))
                                return;
                        }
                        else
                            return;
                    }
                }

                var database = context.HttpContext.RequestServices.GetService(typeof(APIDatabaseContext)) as APIDatabaseContext;
                var userTokens = database.GetFullTable<LoginToken>();
                var token = context.HttpContext.Request.GetToken();

                if(!database.IsTokenValid(token))
                {
                    context.Result = new ContentResult { Content = "Token is not valid", StatusCode = (int)HttpStatusCode.Unauthorized };
                    return;
                }

                var user = database.GetLoginToken(token).User;

                ClaimsIdentity identity = new ClaimsIdentity("Token");
                identity.AddClaim(new Claim(ClaimTypes.Role, user.EmployeeType == "staff" ? "Admin" : "User"));
                identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

                context.HttpContext.User = new ClaimsPrincipal(identity);
                
                if(!string.IsNullOrWhiteSpace(requiredRole))
                {
                    if(requiredRole.ToLower() == "admin" && user.EmployeeType != "staff")
                    {
                        context.Result = new ContentResult { Content = "User is not in a permitted role to perform this action", StatusCode = (int)HttpStatusCode.Forbidden };
                        return;
                    }
                }

                if (usernames.Count > 0)
                {
                    if(!usernames.Contains(user.UserName))
                    {
                        context.Result = new ContentResult { Content = "User has no permission to do this action", StatusCode = (int)HttpStatusCode.Forbidden };
                        return;
                    }
                }

                base.OnActionExecuting(context);
            }
            catch(Exception e)
            {
                Tracer.TraceMessage(e.Message);
            }
        }
    }
}
