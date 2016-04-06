using System.Net;
using Xunit;

namespace Microsoft.Web.Templates.Tests
{
    public class EmptyWebTests : TemplateTestFixure<EmptyWeb.Startup>
    {
        [Fact]
        public async void Verify_Get()
        {
            // Act
            var getReponse = await Client.GetAsync("http://localhost");
            var reponseContent = await getReponse.Content.ReadAsStringAsync();
            // Assert
            Assert.Equal(HttpStatusCode.OK, getReponse.StatusCode);
            Assert.Contains("Hello World!", reponseContent);
        }
    }
}
