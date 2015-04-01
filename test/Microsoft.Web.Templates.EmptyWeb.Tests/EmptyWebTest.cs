using System;
using System.Net;
using EmptyWeb;
using Xunit;

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
