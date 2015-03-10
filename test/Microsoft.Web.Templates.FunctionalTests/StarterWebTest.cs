using System;
using System.IO;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.TestHost;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Framework.DependencyInjection;
using MyApplication;
using Xunit;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.Logging;
using Microsoft.AspNet.Authentication.Cookies;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Net.Http.Headers;

namespace Microsoft.Web.Templates.FunctionalTests
{
    public class StarterWebTests : TemplateTests
    {
        private static readonly string StarterWebTemplateName = "StarterWeb";

        private static TestHostingEnvironment _testHosting = new TestHostingEnvironment();
        private static Action<IApplicationBuilder> _app = ConfigureApp;
        
        private IServiceProvider _provider;

        public StarterWebTests()
        {
            _provider = CreateServices(StarterWebTemplateName);
        }

        public static void ConfigureApp(IApplicationBuilder app)
        {
            var startup = new Startup(_testHosting);
            app.UseServices(services =>
                startup.ConfigureServices(services));

            startup.Configure(app, _testHosting, app.ApplicationServices.GetService<ILoggerFactory>());
        }

        [Fact]
        public async void Verify_Home_Index_Get()
        {
            var server = TestServer.Create(_provider, _app);
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
            var server = TestServer.Create(_provider, _app);
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
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var getReponse = await client.GetAsync("http://localhost/Home/Contact");
            var reponseContent = await getReponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, getReponse.StatusCode);
            Assert.Contains("mailto:Support@example.com", reponseContent);
        }

        //[Fact]
        public async void Verify_Account_Register_Get()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var getReponse = await client.GetAsync("http://localhost/Account/Register");
            var reponseContent = await getReponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, getReponse.StatusCode);
            Assert.Contains("Create a new account.", reponseContent);
        }

        //[Fact]
        public async void Verify_Account_Login_Invalid()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var getResponse = await client.GetAsync("http://localhost/Account/Login");
            var responseContent = await getResponse.Content.ReadAsStringAsync();

            var verificationToken = ExtractVerificationToken(responseContent);
            HttpContent requestContent = CreateLoginPost(verificationToken, "NotAUser", "NoPassword");

            var verificationCookie = ExtractVerificationCookie(getResponse.Headers);
            requestContent.Headers.Add("Cookie", string.Format("__RequestVerificationToken={0}", verificationCookie));

            var postResponse = await client.PostAsync("http://localhost/Account/Login", requestContent);
            var postResponseContent = await getResponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            Assert.Contains("Use a local account to log in.", responseContent);

            // We expect a failed login to just return us to the login page, so the expected content is the same
            Assert.Equal(getResponse.StatusCode, postResponse.StatusCode);
            Assert.Contains(responseContent, postResponseContent);
        }

        private HttpContent CreateLoginPost(string verificationToken, string userName, string password, bool rememberMe = false)
        {
            List<KeyValuePair<string, string>> form = new List<KeyValuePair<string, string>>();

            form.Add(new KeyValuePair<string, string>("__RequestVerificationToken", verificationToken));
            form.Add(new KeyValuePair<string, string>("UserName", userName));
            form.Add(new KeyValuePair<string, string>("Password", password));
            form.Add(new KeyValuePair<string, string>("RememberMe", rememberMe.ToString()));

            var content = new FormUrlEncodedContent(form);

            return content;
        }

        private string ExtractVerificationToken(string response)
        {
            // remove &copy; to make XElement happy.
            var fixedResponse = response.Replace("&copy;", "");
            XElement root = XElement.Parse(fixedResponse);
            var token =
                from el in root.Descendants("input")
                where (string)el.Attribute("name") == "__RequestVerificationToken"
                select (string)el.Attribute("value");

            return token.SingleOrDefault();
        }

        private string ExtractVerificationCookie(HttpHeaders headers)
        {
            var cookiehHeaders = headers.GetValues("Set-Cookie");
            foreach(var header in cookiehHeaders)
            {
                var cookies = header.Split(';');
                foreach(var cookie in cookies)
                {
                    if (cookie.StartsWith("__RequestVerificationToken"))
                    {
                        var parts = cookie.Split('=');
                        return parts[1].Trim();
                    }
                }
            }

            return String.Empty;
        }
    }
}
