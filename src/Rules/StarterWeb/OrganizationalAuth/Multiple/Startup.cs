﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Runtime;

namespace $safeprojectname$
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Setup configuration sources.

            var configuration = new Configuration(appEnv.ApplicationBasePath)
                .AddJsonFile("config.json")
                .AddJsonFile($"config.{env.EnvironmentName}.json", optional: true);

            if (env.IsEnvironment("Development"))
            {
                // Add a fwlink here and add some comment.
                configuration.AddUserSecrets();
            }
            configuration.AddEnvironmentVariables();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration.GetSubKey("AppSettings"));

            // Add services for OpenId Connect middleware to login using Azure AD.
            services.AddDataProtection();

            services.Configure<CookieAuthenticationOptions>(options =>
            {
                options.AuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.AutomaticAuthentication = true;
            });

            services.ConfigureOpenIdConnectAuthentication(options =>
            {
                options.ClientId = Configuration["Authentication:AzureAd:ClientId"];
                options.Authority = Configuration["Authentication:AzureAd:AADInstance"] + "Common";
                options.AutomaticAuthentication = true;
            });

            // Add MVC services to the services container.
            services.AddMvc();

            // Uncomment the following line to add Web API servcies which makes it easier to port Web API 2 controllers.
            // You need to add Microsoft.AspNet.Mvc.WebApiCompatShim package to project.json
            // services.AddWebApiConventions();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory)
        {
            // Configure the HTTP request pipeline.
            // Add the console logger.
            loggerfactory.AddConsole();

            // Add the following to the request pipeline only in development environment.
            if (env.IsEnvironment("Development"))
            {
                app.UseBrowserLink();
                app.UseErrorPage(ErrorPageOptions.ShowAll);
            }
            else
            {
                // Add Error handling middleware which catches all application specific errors and
                // send the request to the following path or controller action.
                app.UseErrorHandler("/Home/Error");
            }

            // Add static files to the request pipeline.
            app.UseStaticFiles();

            // Add cookie-based authentication to the request pipeline.
            app.UseCookieAuthentication();

            // Add OpenIdConnect middleware so you can login using Azure AD.
            app.UseOpenIdConnectAuthentication(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // instead of using the default validation (validating against a single issuer value, as we do in line of business apps),
                    // we inject our own multitenant validation logic
                    ValidateIssuer = false,

                    // If the app needs access to the entire organization, then add the logic
                    // of validating the Issuer here.
                    // IssuerValidator
                };
                options.Notifications = new OpenIdConnectAuthenticationNotifications()
                {
                    SecurityTokenValidated = (context) =>
                    {
                        // If your authentication logic is based on users then add your logic here
                        return Task.FromResult(0);
                    },
                    AuthenticationFailed = (context) =>
                    {
                        // Pass in the context back to the app
                        context.Response.Redirect("/Home/Error");
                        context.HandleResponse(); // Suppress the exception
                        return Task.FromResult(0);
                    }
                };
            });

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
