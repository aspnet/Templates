using System;
using WebAPI;
using Xunit;
using System.Net;
using System.Net.Http;

namespace Microsoft.Web.Templates.Tests
{
    public class WebAPITests : IClassFixture<TemplateTestFixure<WebAPI.Startup>>
    {
        public HttpClient Client { get; private set; }
        private TemplateTestFixure<WebAPI.Startup> _fixture;

        public WebAPITests(TemplateTestFixure<WebAPI.Startup> fixture) : base()
        {
            _fixture = fixture;
            this.Client = _fixture.Client;
        }

        [Fact]
        public async void Verify_Api_Get_Values()
        {
            // Act
            var getReponse = await Client.GetAsync("http://localhost/api/values");
            var responseContent = await getReponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, getReponse.StatusCode);
            Assert.Equal("[\"value1\",\"value2\"]", responseContent);
        }

        [Fact]
        public async void Verify_Api_Get_Value()
        {
            // Act
            var getReponse = await Client.GetAsync("http://localhost/api/values/5");
            var responseContent = await getReponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, getReponse.StatusCode);
            Assert.Equal("value", responseContent);
        }
    }
}
