﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.Facebook;
using Microsoft.AspNet.Authentication.Google;
using Microsoft.AspNet.Authentication.MicrosoftAccount;
using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Authentication.Twitter;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics.Entity;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using Microsoft.Dnx.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders;
using $safeprojectname$.Models;
using $safeprojectname$.Services;

namespace $safeprojectname$
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Setup configuration sources.

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // This reads the configuration keys from the secret store.
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Entity Framework services to the services container.
            services.AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]));

            // Add Identity services to the services container.
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add MVC services to the services container.
            services.AddMvc();

            // Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
            // You will also need to add the Microsoft.AspNet.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
            // services.AddWebApiConventions();

            // Register application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.MinimumLevel = LogLevel.Information;
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            // Configure the HTTP request pipeline.

            // Add the following to the request pipeline only in development environment.
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                // Add Error handling middleware which catches all application specific errors and
                // sends the request to the following path or controller action.
                app.UseExceptionHandler("/Home/Error");
            }

            // Adds middleware to the request pipeline for forwarding Windows Authentication, request scheme, remote IPs, etc to the IIS HttpPlatformHandler..
            app.UseIISPlatformHandler();

            // Add static files to the request pipeline.
            app.UseStaticFiles();

            // Add cookie-based authentication to the request pipeline.
            app.UseIdentity();

        // Add and configure the options for authentication middleware to the request pipeline.
        // You can add options for middleware as shown below.
        // For more information see http://go.microsoft.com/fwlink/?LinkID=532715
        //app.UseFacebookAuthentication(options =>
        //{
        //    options.AppId = Configuration["Authentication:Facebook:AppId"];
        //    options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
        //});
        //app.UseGoogleAuthentication(options =>
        //{
        //    options.ClientId = Configuration["Authentication:Google:ClientId"];
        //    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
        //    options.Events = new OAuthEvents()
        //    {
        //        OnRemoteError = ctx =>
        //        {
        //            ctx.Response.Redirect("/Account/ExternalLoginCallback?remoteError=" + UrlEncoder.Default.UrlEncode(ctx.Error.Message));
        //            ctx.HandleResponse();
        //            return Task.FromResult(0);
        //        }
        //    };
        //});
        //app.UseMicrosoftAccountAuthentication(options =>
        //{
        //    options.ClientId = Configuration["Authentication:MicrosoftAccount:ClientId"];
        //    options.ClientSecret = Configuration["Authentication:MicrosoftAccount:ClientSecret"];
        //});
        //app.UseTwitterAuthentication(options =>
        //{
        //    options.ConsumerKey = Configuration["Authentication:Twitter:ConsumerKey"];
        //    options.ConsumerSecret = Configuration["Authentication:Twitter:ConsumerSecret"];
        //});

        // Add MVC to the request pipeline.
        app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                // Uncomment the following line to add a route for porting Web API 2 controllers.
                // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
            });
        }
    }
}
