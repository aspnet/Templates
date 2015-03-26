using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.TestHost;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.ServiceLookup;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Infrastructure;

namespace Microsoft.Web.Templates.Tests
{
    public abstract class TemplateTestBase
    {
        // path from Templates\test\Microsoft.Web.Templates.Tests
        protected static readonly string TestProjectsPath = Path.Combine("..", "..", "artifacts", "build", "Test");

        protected TestServer CreateServer()
        {
            return CreateServer(GetHostingInformation());
        }

        protected virtual HostingInformation GetHostingInformation()
        {
            return new HostingInformation(StartupType);
        }

        protected abstract Type StartupType { get; }
        protected abstract string TemplateName { get; }

        private TestServer CreateServer(HostingInformation hostingInfo)
        {
            return TestServer.Create(
                hostingInfo.ApplicationBuilder,
                services => AddTestServices(services, TemplateName, TestProjectsPath, hostingInfo.ApplicationConfigureServices));
        }

        private static void AddTestServices(
            IServiceCollection services,
            string applicationWebSiteName,
            string applicationPath,
            Action<IServiceCollection> configureServices)
        {
            applicationPath = applicationPath ?? TestProjectsPath;

            // Get current IApplicationEnvironment; likely added by the host.
            var provider = services.BuildServiceProvider();
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
                applicationWebSiteName,
                applicationPath);
            var environment = new TestApplicationEnvironment(
                originalEnvironment,
                applicationBasePath);
            services.AddInstance<IApplicationEnvironment>(environment);
            services.AddInstance<IHostingEnvironment>(new HostingEnvironment(environment));
            var providerType = CreateAssemblyProviderType(applicationWebSiteName);
            var configuration = new TestConfigurationProvider();
            configuration.Configuration.Set(
                typeof(IAssemblyProvider).FullName,
                providerType.AssemblyQualifiedName);
            services.AddInstance<ITestConfigurationProvider>(configuration);
            services.AddInstance<ILoggerFactory>(new LoggerFactory());


            if (configureServices != null)
            {
                configureServices(services);
            }
        }


        // Calculate the path relative to the application base path.
        public static string CalculateApplicationBasePath(IApplicationEnvironment appEnvironment,
                                                          string applicationWebSiteName, string websitePath)
        {
            return Path.GetFullPath(
                Path.Combine(appEnvironment.ApplicationBasePath, websitePath, applicationWebSiteName));
        }

        private static Type CreateAssemblyProviderType(string siteName)
        {
            // Creates a service type that will limit MVC to only the controllers in the test site.
            // We only want this to happen when running in-process.
            var assembly = Assembly.Load(new AssemblyName(siteName));
            var providerType = typeof(TestAssemblyProvider<>).MakeGenericType(assembly.GetExportedTypes()[0]);
            return providerType;
        }
    }
}