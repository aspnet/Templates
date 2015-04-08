using System;
using WebAPI;
using Xunit;
using System.Net;

namespace Microsoft.Web.Templates.Tests
{
    public class WebAPITests : TemplateTestBase
    {
        private static readonly string _templateName = "WebAPI";

        protected override string TemplateName
        {
            get
            {
                return _templateName;
            }
        }

        [Fact]
        public async void Verify_Api_Get_Values()
        {
            var server = CreateServer();
            var client = server.CreateClient();

            // Act
            var getReponse = await client.GetAsync("http://localhost/api/values");
            var responseContent = await getReponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, getReponse.StatusCode);
            Assert.Equal("[\"value1\",\"value2\"]", responseContent);
        }

        [Fact]
        public async void Verify_Api_Get_Value()
        {
            var server = CreateServer();
            var client = server.CreateClient();

            // Act
            var getReponse = await client.GetAsync("http://localhost/api/values/5");
            var responseContent = await getReponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, getReponse.StatusCode);
            Assert.Equal("value", responseContent);
        }
    }
}
