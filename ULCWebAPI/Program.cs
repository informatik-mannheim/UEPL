using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                .UseKestrel() //.UseKestrel(cfg => cfg.UseHttps(new X509Certificate2("Cert.pfx", "")))
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseUrls("http://*:10000")
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
