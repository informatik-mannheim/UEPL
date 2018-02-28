using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace ULCWebAPI
{
    /// <summary>
    /// Class containing the Web-App Entrypoint
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Program-Entrypoint calling the ASP.Net Core WebHost
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, 10000);
                    options.Listen(IPAddress.Any, 10001, listenOptions =>
                    {
                        listenOptions.UseHttps("server.pfx", "");
                    });
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
