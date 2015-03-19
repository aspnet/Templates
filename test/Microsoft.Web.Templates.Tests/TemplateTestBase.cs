using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.ServiceLookup;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Infrastructure;

namespace Microsoft.Web.Templates.Tests
{
    public abstract class TemplateTestBase : IDisposable
    {
        // path from Templates\test\Microsoft.Web.Templates.Tests
        protected static readonly string TestProjectsPath = Path.Combine("..", "..", "artifacts", "build", "Test");

        protected IServiceProvider CreateServices(string applicationWebSiteName)
        {
            return CreateServices(applicationWebSiteName, TestProjectsPath);
        }

        public void Dispose()
        {
          Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        protected abstract Type StartupType { get; }
        protected abstract string TemplateName { get; }

        public HostingInformation InitializeHosting()
        {
          return InitializeHosting(StartupType, TemplateName);
        }

        protected HostingInformation InitializeHosting(Type startupType, string templateName)
        {
            return new HostingInformation(startupType)
            {
                Provider = CreateServices(templateName)
            };
        }

        private IServiceProvider CreateServices(string applicationWebSiteName, string applicationPath)
        {
            var originalProvider = CallContextServiceLocator.Locator.ServiceProvider;
            var appEnvironment = originalProvider.GetRequiredService<IApplicationEnvironment>();

            // When an application executes in a regular context, the application base path points to the root 
            // directory where the application is located, for example MvcSample.Web. However, when executing 
            // an aplication as part of a test, the ApplicationBasePath of the IApplicationEnvironment points 
            // to the root folder of the test project. 
            // To compensate for this, we need to calculate the original path and override the application 
            // environment value so that components like the view engine work properly in the context of the 
            // test. 
            var appBasePath = CalculateApplicationBasePath(appEnvironment, applicationWebSiteName, applicationPath);

            var services = new ServiceCollection();
            services.AddInstance(
                typeof(IApplicationEnvironment),
                new TestApplicationEnvironment(appEnvironment, appBasePath));

            // Injecting a custom assembly provider via configuration setting. It's not good enough to just 
            // add the service directly since it's registered by MVC. 
            var providerType = CreateAssemblyProviderType(applicationWebSiteName);

            var configuration = new TestConfigurationProvider();
            configuration.Configuration.Set(
                typeof(IAssemblyProvider).FullName,
                providerType.AssemblyQualifiedName);

            services.AddInstance(
                typeof(ITestConfigurationProvider),
                configuration);

            services.AddInstance(
                typeof(ILoggerFactory),
                new LoggerFactory());

            return BuildFallbackServiceProvider(services, originalProvider);
        }

        // Calculate the path relative to the application base path.
        public static string CalculateApplicationBasePath(IApplicationEnvironment appEnvironment,
                                                          string applicationWebSiteName, string websitePath)
        {
            // Mvc/test/WebSites/applicationWebSiteName
            return Path.GetFullPath(
                Path.Combine(appEnvironment.ApplicationBasePath, websitePath, applicationWebSiteName));
        }

        // TODO: Kill this code
        private static IServiceProvider BuildFallbackServiceProvider(IEnumerable<ServiceDescriptor> services, IServiceProvider fallback)
        {
            var sc = HostingServices.Create(fallback);
            sc.Add(services);

            // Build the manifest
            var manifestTypes = services.Where(t => t.ServiceType.GetTypeInfo().GenericTypeParameters.Length == 0
                    && t.ServiceType != typeof(IServiceManifest)
                    && t.ServiceType != typeof(IServiceProvider))
                    .Select(t => t.ServiceType).Distinct();
            sc.AddInstance<IServiceManifest>(new ServiceManifest(manifestTypes, fallback.GetRequiredService<IServiceManifest>()));
            return sc.BuildServiceProvider();
        }

        private static Type CreateAssemblyProviderType(string siteName)
        {
            // Creates a service type that will limit MVC to only the controllers in the test site.
            // We only want this to happen when running in proc.
            var assembly = Assembly.Load(new AssemblyName(siteName));

            var providerType = typeof(TestAssemblyProvider<>).MakeGenericType(assembly.GetExportedTypes()[0]);
            return providerType;
        }

        private class ServiceManifest : IServiceManifest
        {
            public ServiceManifest(IEnumerable<Type> services, IServiceManifest fallback = null)
            {
                Services = services;
                if (fallback != null)
                {
                    Services = Services.Concat(fallback.Services).Distinct();
                }
            }

            public IEnumerable<Type> Services { get; private set; }
        }
    }
}