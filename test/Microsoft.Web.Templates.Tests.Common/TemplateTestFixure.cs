using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Versioning;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
                .ConfigureServices(serviceCollection => InitializeServices(serviceCollection, relativePath))
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

        protected virtual void InitializeServices(IServiceCollection services, string relativePath)
        {
            // When an application executes in a regular context, the application base path points to the root
            // directory where the application is located, for example .../samples/MvcSample.Web. However, when
            // executing an application as part of a test, the ApplicationBasePath of the IApplicationEnvironment
            // points to the root folder of the test project.
            // To compensate, we need to calculate the correct project path and override the application
            // environment value so that components like the view engine work properly in the context of the test.
            var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;
            var applicationName = startupAssembly.GetName().Name;

            var applicationEnvironment = PlatformServices.Default.Application;

            var applicationRoot = Path.GetFullPath(Path.Combine(
               applicationEnvironment.ApplicationBasePath,
               relativePath,
               applicationName
            ));

            services.AddSingleton<IApplicationEnvironment>(
                new TestApplicationEnvironment(applicationEnvironment, applicationName, applicationRoot));

            // Inject a custom assembly provider. Overrides AddMvc() because that uses TryAdd().
            var assemblyProvider = new StaticAssemblyProvider();
            assemblyProvider.CandidateAssemblies.Add(startupAssembly);
            services.AddSingleton<IAssemblyProvider>(assemblyProvider);
        }
    }

    // An application environment that overrides the base path of the original
    // application environment in order to make it point to the folder of the original web
    // aaplication so that components like ViewEngines can find views as if they were executing
    // in a regular context.
    public class TestApplicationEnvironment : IApplicationEnvironment
    {
        private readonly IApplicationEnvironment _original;

        public TestApplicationEnvironment(IApplicationEnvironment original, string name, string basePath)
        {
            _original = original;
            ApplicationName = name;
            ApplicationBasePath = basePath;
        }

        public string ApplicationName { get; }

        public string ApplicationVersion
        {
            get
            {
                return _original.ApplicationVersion;
            }
        }

        public string ApplicationBasePath { get; }

        public FrameworkName RuntimeFramework
        {
            get
            {
                return _original.RuntimeFramework;
            }
        }
    }
}