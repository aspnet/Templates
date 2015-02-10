using System;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Hosting;

namespace Microsoft.Web.Templates.FunctionalTests
{
    public class TestHostingEnvironment : IHostingEnvironment
    {
        public string EnvironmentName
        {
            get; set;
        }

        public string WebRoot
        {
            get; internal set;
        }

        public IFileProvider WebRootFileProvider
        {
            get; set;
        }
    }
}