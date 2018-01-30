using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ULCWebAPI.Extensions
{
    /// <summary>
    /// Class containing controller extensions
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Generate a full qualified url to an controller action
        /// </summary>
        /// <param name="controller">Controller containing the method to call</param>
        /// <param name="routeName">Name of the route which is the call-target</param>
        /// <param name="values">RouteParameters used to identify the full route</param>
        /// <returns></returns>
        public static string FullURL(this ControllerBase controller, string routeName, object values)
        {
            return controller.Url.Action(routeName, controller.RouteData.Values["controller"] as string, values, controller.Request.Scheme);
        }
    }
}
