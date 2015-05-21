using System.Net;
using Xunit;

namespace Microsoft.Web.Templates.Tests
{
    public class StarterWebAITests : TemplateTestBase
    {
        private static readonly string _templateName = "StarterWeb.AI";

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
            Assert.Contains("Home Page - " + _templateName, reponseContent);
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
