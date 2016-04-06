using System.Net;
using System.Net.Http;
using Xunit;

namespace Microsoft.Web.Templates.Tests
{
    public class StarterWebTests : IClassFixture<TemplateTestFixure<StarterWeb.Startup>>
    {
        public HttpClient Client { get; private set; }
        private TemplateTestFixure<StarterWeb.Startup> _fixture;

        public StarterWebTests(TemplateTestFixure<StarterWeb.Startup> fixture) : base()
        {
            _fixture = fixture;
            this.Client = _fixture.Client;
        }

        [Fact]
        public async void Verify_Home_Index_Get()
        {
            // Act
            var getReponse = await Client.GetAsync("http://localhost");
            var reponseContent = await getReponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, getReponse.StatusCode);
            Assert.Contains("Home Page - StarterWeb", reponseContent);
        }

        [Fact]
        public async void Verify_Home_About_Get()
        {
            // Act
            var getReponse = await Client.GetAsync("http://localhost/Home/About");
            var reponseContent = await getReponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, getReponse.StatusCode);
            Assert.Contains("Use this area to provide additional information.", reponseContent);
        }

        [Fact]
        public async void Verify_Home_Contact_Get()
        {
            // Act
            var getReponse = await Client.GetAsync("http://localhost/Home/Contact");
            var reponseContent = await getReponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, getReponse.StatusCode);
            Assert.Contains("mailto:Support@example.com", reponseContent);
        }
    }
}
