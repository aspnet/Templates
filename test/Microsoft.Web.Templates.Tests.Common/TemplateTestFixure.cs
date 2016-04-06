using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Versioning;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;

namespace Microsoft.Web.Templates.Tests
{
    public class TemplateTestFixure<TStartup> : IDisposable
    {
        private TestServer _server;
        
        // path from Templates\test\Microsoft.Web.Templates.Tests
        protected static readonly string TestProjectsPath = Path.Combine("..", "..", "..", "..", "..", "..", "intermediate", "Test");

        public TemplateTestFixure()
            : this(TestProjectsPath)
        {
        }

        protected TemplateTestFixure(string relativePath)
        {
            var builder = new WebHostBuilder()
                .UseContentRoot(GetApplicationPath(relativePath))
                .ConfigureServices(InitializeServices)
                .UseStartup(typeof(TStartup));

            _server = new TestServer(builder);

            CreateClient();
        }

        public HttpClient Client { get; private set; }

        public void Dispose()
        {
            Client.Dispose();
            _server.Dispose();
        }

        public HttpClient CreateClient()
        {
            if (Client != null)
            {
                Client.Dispose();
            }

            Client = _server.CreateClient();
            Client.BaseAddress = new Uri("http://localhost");

            return Client;
        }

        private static string GetApplicationPath(string relativePath)
        {
            var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;
            var applicationName = startupAssembly.GetName().Name;
            var applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;
            return Path.GetFullPath(Path.Combine(applicationBasePath, relativePath, applicationName));
        }

        protected virtual void InitializeServices(IServiceCollection services)
        {
            var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;

            // Inject a custom application part manager. Overrides AddMvcCore() because that uses TryAdd().
            var manager = new ApplicationPartManager();
            manager.ApplicationParts.Add(new AssemblyPart(startupAssembly));

            manager.FeatureProviders.Add(new ControllerFeatureProvider());
            manager.FeatureProviders.Add(new ViewComponentFeatureProvider());

            services.AddSingleton(manager);
        }
    }
}