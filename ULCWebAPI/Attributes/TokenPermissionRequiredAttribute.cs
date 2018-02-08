using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ULCWebAPI.Extensions;
using ULCWebAPI.Helper;
using ULCWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ULCWebAPI.Attributes
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TokenPermissionRequiredAttribute : ActionFilterAttribute
    {
        private bool oneRequirement = false;
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
        /// <param name="or"></param>
        public TokenPermissionRequiredAttribute(string[] usernames, string requiredRole = "", bool or = false)
        {
            this.usernames.AddRange(usernames);
            this.requiredRole = requiredRole;
            this.oneRequirement = or;
        }

        /// <summary>
        /// Validates the permitted token and grants or denies the access to an action
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                var inRole = false;
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

                if (!string.IsNullOrWhiteSpace(requiredRole))
                {
                    if (!identity.HasClaim(ClaimTypes.Role, requiredRole))
                    {
                        if (!oneRequirement)
                        {
                            context.Result = new ContentResult { Content = "User is not in a permitted role to perform this action", StatusCode = (int)HttpStatusCode.Forbidden };
                            return;
                        }
                    }
                    else
                        inRole = true;
                }

                if (usernames.Count > 0)
                {
                    if (!usernames.Contains(user.UserName) && oneRequirement && !inRole)
                    {
                        context.Result = new ContentResult { Content = "User has no permission to do this action", StatusCode = (int)HttpStatusCode.Forbidden };
                        return;
                    }
                }

                context.HttpContext.User = new ClaimsPrincipal(identity);
                base.OnActionExecuting(context);
            }
            catch(Exception e)
            {
                Tracer.TraceMessage(e.Message);
            }
        }
    }
}
