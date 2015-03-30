using System;
using System.IO;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.TestHost;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Framework.DependencyInjection;
using EmptyWeb;
using Xunit;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.Logging;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Net.Http.Headers;

namespace Microsoft.Web.Templates.Tests
{
    public class EmptyWebTests : TemplateTestBase
    {
        private static readonly string _templateName = "EmptyWeb";

        public EmptyWebTests()
        {
        }

        protected override Type StartupType
        {
            get
            {
                return typeof(Startup);
            }
        }

        protected override string TemplateName
        {
            get
            {
                return _templateName;
            }
        }

        protected override HostingInformation GetHostingInformation()
        {
            return new EmptyWebHostingInformation(StartupType);
        }

        [Fact]
        public async void Verify_Get()
        {
            var server = CreateServer();
            var client = server.CreateClient();

            // Act
            var getReponse = await client.GetAsync("http://localhost");
            var reponseContent = await getReponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, getReponse.StatusCode);
            Assert.Contains("Hello World!", reponseContent);
        }
    }
}
