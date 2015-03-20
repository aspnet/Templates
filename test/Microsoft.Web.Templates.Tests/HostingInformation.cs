using System;
using System.Reflection;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.ServiceLookup;
using Microsoft.Framework.Logging;

namespace Microsoft.Web.Templates.Tests
{
    public class HostingInformation
    {
        private Type _startupType;
        private TestHostingEnvironment _hostingEnvironment;
        private object _startup;

        public HostingInformation(Type startupType)
        {
            _startupType = startupType;
            _hostingEnvironment = new TestHostingEnvironment();
            _startup = System.Activator.CreateInstance(_startupType, _hostingEnvironment);
        }
        public TestHostingEnvironment HostingEnvironment
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

        private void ConfigureApp(IApplicationBuilder app)
        {
            var configure = _startupType.GetMethod("Configure", BindingFlags.Public | BindingFlags.Instance);
            configure.Invoke(_startup, new object[] { app, HostingEnvironment, app.ApplicationServices.GetService<ILoggerFactory>() });
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var configureServices = _startupType.GetMethod("ConfigureServices", BindingFlags.Public | BindingFlags.Instance);
            configureServices.Invoke(_startup, new object[] { services });
        }
    }
}