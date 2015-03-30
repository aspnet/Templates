using System;
using System.Reflection;
using System.Threading;
using Microsoft.AspNet.Builder;

namespace Microsoft.Web.Templates.Tests
{
    public class EmptyWebHostingInformation : HostingInformation
    {
        private object _startup;

        public EmptyWebHostingInformation(Type startupType) : base(startupType)
        {
        }

        protected override object Startup
        {
            get
            {
                return LazyInitializer.EnsureInitialized<object>(ref _startup, () =>
                {
                    return System.Activator.CreateInstance(StartupType);
                });
            }
        }

        protected override void ConfigureApp(IApplicationBuilder app)
        {
            var configure = StartupType.GetMethod("Configure", BindingFlags.Public | BindingFlags.Instance);
            configure.Invoke(Startup, new object[] { app });
        }
    }
}