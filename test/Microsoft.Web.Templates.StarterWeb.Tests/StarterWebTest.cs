using System;
using System.IO;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.TestHost;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Framework.DependencyInjection;
using StarterWeb;
using Xunit;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.Logging;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Net.Http.Headers;

namespace Microsoft.Web.Templates.Tests
{
    public class StarterWebTests : TemplateTestBase
    {
        private static readonly string _templateName = "StarterWeb";

        protected override string TemplateName
        {
            get
            {
                return _templateName;
            }
        }

        [Fact]
        public async void Verify_Home_Index_Get()
        {
            var server = CreateServer();
            var client = server.CreateClient();

            // Act
            var getReponse = await client.GetAsync("http://localhost");
            var reponseContent = await getReponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, getReponse.StatusCode);
            Assert.Contains("This application consists of:", reponseContent);
        }

        [Fact]
        public async void Verify_Home_About_Get()
        {
            var server = CreateServer();
            var client = server.CreateClient();

            // Act
            var getReponse = await client.GetAsync("http://localhost/Home/About");
            var reponseContent = await getReponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, getReponse.StatusCode);
            Assert.Contains("Use this area to provide additional information.", reponseContent);
        }

        [Fact]
        public async void Verify_Home_Contact_Get()
        {
            var server = CreateServer();
            var client = server.CreateClient();

            // Act
            var getReponse = await client.GetAsync("http://localhost/Home/Contact");
            var reponseContent = await getReponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, getReponse.StatusCode);
            Assert.Contains("mailto:Support@example.com", reponseContent);
        }
    }
}
