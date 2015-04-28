using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.TestHost;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Infrastructure;

namespace Microsoft.Web.Templates.Tests
{
    public abstract class TemplateTestBase
    {
        // path from Templates\test\Microsoft.Web.Templates.Tests
        protected static readonly string TestProjectsPath = Path.Combine("..", "..", "artifacts", "build", "Test");

        protected abstract string TemplateName { get; }

        protected TestServer CreateServer()
        {
            EnsurePath(Path.Combine(TestProjectsPath, TemplateName, "wwwroot"));

            // Get current IApplicationEnvironment; likely added by the host.
            var provider = CallContextServiceLocator.Locator.ServiceProvider;
            var originalEnvironment = provider.GetRequiredService<IApplicationEnvironment>();

            // When an application executes in a regular context, the application base path points to the root
            // directory where the application is located, for example MvcSample.Web. However, when executing
            // an application as part of a test, the ApplicationBasePath of the IApplicationEnvironment points
            // to the root folder of the test project.
            // To compensate for this, we need to calculate the original path and override the application
            // environment value so that components like the view engine work properly in the context of the
            // test.
            var applicationBasePath = CalculateApplicationBasePath(
                originalEnvironment,
                TemplateName,
                TestProjectsPath);
            var environment = new TestApplicationEnvironment(
                originalEnvironment,
                applicationBasePath,
                TemplateName);

            var hostingEnvironment = new HostingEnvironment();
            hostingEnvironment.Initialize(applicationBasePath, environmentName: null);
            try
            {
                CallContextServiceLocator.Locator.ServiceProvider = new WrappingServiceProvider(provider, environment, hostingEnvironment);
                var assemblyProvider = CreateAssemblyProvider(TemplateName);
                var builder = TestServer.CreateBuilder()
                    .UseStartup(TemplateName)
                    .UseServices(services => 
                    {
                        services.AddInstance<IHostingEnvironment>(hostingEnvironment);
                        services.AddInstance(assemblyProvider);
                    });
                return new TestServer(builder);
            }
            finally
            {
                CallContextServiceLocator.Locator.ServiceProvider = provider;
            }
        }

        // Calculate the path relative to the application base path.
        public static string CalculateApplicationBasePath(IApplicationEnvironment appEnvironment,
                                                          string applicationWebSiteName, string websitePath)
        {
            return Path.GetFullPath(
                Path.Combine(appEnvironment.ApplicationBasePath, websitePath, applicationWebSiteName));
        }

        private static IAssemblyProvider CreateAssemblyProvider(string siteName)
        {
            // Creates a service type that will limit MVC to only the controllers in the test site.
            // We only want this to happen when running in-process.
            var assembly = Assembly.Load(new AssemblyName(siteName));
            var provider = new FixedSetAssemblyProvider
            {
                CandidateAssemblies =
                {
                    assembly,
                },
            };

            return provider;
        }

        private static void EnsurePath(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private class WrappingServiceProvider : IServiceProvider
        {
            private readonly IApplicationEnvironment _appEnv;
            private readonly IHostingEnvironment _hostingEnv;
            private readonly IServiceProvider _fallback;

            public WrappingServiceProvider(IServiceProvider fallback, IApplicationEnvironment appEnv, IHostingEnvironment hostingEnv)
            {
                _appEnv = appEnv;
                _hostingEnv = hostingEnv;
                _fallback = fallback;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IApplicationEnvironment))
                {
                    return _appEnv;
                }
                else if (serviceType == typeof(IHostingEnvironment))
                {
                    return _hostingEnv;
                }
                return _fallback.GetService(serviceType);
            }
        }

    }
}