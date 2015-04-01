using System;
using System.Reflection;
using System.Threading;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.ServiceLookup;
using Microsoft.Framework.Logging;

namespace Microsoft.Web.Templates.Tests
{
    public class HostingInformation
    {
        private Type _startupType;
        private HostingEnvironment _hostingEnvironment;
        private object _startup;

        public HostingInformation(Type startupType)
        {
            _startupType = startupType;
            _hostingEnvironment = new HostingEnvironment();
        }
        public HostingEnvironment HostingEnvironment
        {
            get { return _hostingEnvironment; }
        }
        public Action<IApplicationBuilder> ApplicationBuilder
        {
            get { return ConfigureApp; }
        }

        public Action<IServiceCollection> ApplicationConfigureServices
        {
            get { return ConfigureServices; }
        }

        protected virtual object Startup
        {
            get
            {
                return LazyInitializer.EnsureInitialized<object>(ref _startup, () => 
                {
                    return System.Activator.CreateInstance(_startupType, _hostingEnvironment);
                });
            }
        }

        protected Type StartupType
        {
            get { return _startupType; }
        }

        protected virtual void ConfigureApp(IApplicationBuilder app)
        {
            var configure = StartupType.GetMethod("Configure", BindingFlags.Public | BindingFlags.Instance);
            configure.Invoke(Startup, new object[] { app, HostingEnvironment, app.ApplicationServices.GetService<ILoggerFactory>() });
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            var configureServices = StartupType.GetMethod("ConfigureServices", BindingFlags.Public | BindingFlags.Instance);
            configureServices.Invoke(Startup, new object[] { services });
        }
    }
}