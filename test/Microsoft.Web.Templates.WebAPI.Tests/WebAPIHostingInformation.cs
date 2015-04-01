//using System;
//using System.Reflection;
//using Microsoft.AspNet.Builder;

//namespace Microsoft.Web.Templates.Tests
//{
//    public class WebAPIHostingInformation : HostingInformation
//    {
//        public WebAPIHostingInformation(Type startupType) : base(startupType)
//        {
//        }

//        protected override void ConfigureApp(IApplicationBuilder app)
//        {
//            var configure = StartupType.GetMethod("Configure", BindingFlags.Public | BindingFlags.Instance);
//            configure.Invoke(Startup, new object[] { app, HostingEnvironment });
//        }
//    }
//}