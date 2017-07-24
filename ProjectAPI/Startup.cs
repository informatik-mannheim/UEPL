﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using ProjectAPI.Models;
using System.IO;

namespace ProjectAPI
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Global Configuration
        /// </summary>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// Global Environment
        /// </summary>
        public IHostingEnvironment Environment { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            Environment = env;
        }

        /// <summary>
        /// Service-Configuration called by .NetCore Runtime
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.Configure<FormOptions>(config =>
            {
                config.ValueLengthLimit = int.MaxValue;
                config.MultipartBodyLengthLimit = uint.MaxValue;
                config.MultipartBoundaryLengthLimit = int.MaxValue;
            });

            var context = new APIDatabaseContext();

            /*if (Environment.IsDevelopment())
                context.Database.EnsureDeleted();*/
            
            context.Database.EnsureCreated();
            context.SaveChanges();

            services.AddCors(options => options.AddPolicy("Automatic", builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));
            services.AddSingleton(Environment);
            services.AddRouting(/*options => options.LowercaseUrls = true*/);
            services.AddDbContext<APIDatabaseContext>(ServiceLifetime.Scoped);
            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = "Project API", Version = "v1" });
                config.IncludeXmlComments(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "ProjectAPI.xml"));
            });
        }

        /// <summary>
        /// Configures the HTTP Pipeline and is called by runtime
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if(env.IsDevelopment())
            {
                loggerFactory.AddConsole(Configuration.GetSection("Logging"));
                loggerFactory.AddDebug();
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("Automatic");
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
            app.UseSwagger();
            app.UseSwaggerUI(config =>
            {
                config.RoutePrefix = "api";
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "Project API v1");
            });
        }
    }
}
