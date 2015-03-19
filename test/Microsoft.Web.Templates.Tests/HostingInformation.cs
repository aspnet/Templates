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

		public HostingInformation(Type startupType)
		{
			_startupType = startupType;
			_hostingEnvironment = new TestHostingEnvironment();
		}
		public TestHostingEnvironment HostingEnvironment
		{
			get { return _hostingEnvironment; }
		}
		public Action<IApplicationBuilder> ApplicationBuilder
		{
			get { return ConfigureApp; }
		}

		public IServiceProvider Provider { get; set; }

		private void ConfigureApp(IApplicationBuilder app)
		{
			var startup = System.Activator.CreateInstance(_startupType, _hostingEnvironment);
			var configureServices = _startupType.GetMethod("ConfigureServices", BindingFlags.Public | BindingFlags.Instance);
			app.UseServices(services => configureServices.Invoke(startup, new object[] { services }));

			var configure = _startupType.GetMethod("Configure", BindingFlags.Public | BindingFlags.Instance);
			configure.Invoke(startup, new object[] { app, HostingEnvironment, app.ApplicationServices.GetService<ILoggerFactory>() });
		}
	}
}