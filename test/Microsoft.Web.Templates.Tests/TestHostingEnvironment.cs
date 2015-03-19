using System;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Hosting;

namespace Microsoft.Web.Templates.Tests
{
    public class TestHostingEnvironment : IHostingEnvironment
    {
        public string EnvironmentName
        {
            get; set;
        }

        public string WebRootPath
        {
            get; internal set;
        }

        public IFileProvider WebRootFileProvider
        {
            get; set;
        }
    }
}