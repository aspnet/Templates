using System;
using System.IO;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.TestHost;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Framework.DependencyInjection;
using StarterWeb.IndividualAuth;
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
    public class StarterWebIndividualAuthTests : TemplateTestBase
    {
        private static readonly string _templateName = "StarterWeb.IndividualAuth";

        public StarterWebIndividualAuthTests()
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

        [Fact]
        public async void Verify_Account_Register_Get()
        {
            var server = CreateServer();
            var client = server.CreateClient();

            // Act
            var getReponse = await client.GetAsync("http://localhost/Account/Register");
            Assert.Equal(HttpStatusCode.OK, getReponse.StatusCode);

            var reponseContent = await getReponse.Content.ReadAsStringAsync();
            Assert.Contains("Create a new account.", reponseContent);
        }

        [Fact]
        public async void Verify_Account_Register_CreateAccount()
        {
            var server = CreateServer();
            var client = server.CreateClient();

            // Act
            var getResponse = await client.GetAsync("http://localhost/Account/Register");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var responseContent = await getResponse.Content.ReadAsStringAsync();
            Assert.Contains("Create a new account.", responseContent);

            var verificationToken = ExtractVerificationToken(responseContent);

            HttpContent requestContent = CreateRegisterPost(verificationToken, "ANewUser@ms.com", "Asd!123$$", "Asd!123$$");
            AddCookiesToRequest(getResponse.Headers, requestContent.Headers);

            var postResponse = await client.PostAsync("http://localhost/Account/Register", requestContent);
            var postResponseContent = await getResponse.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
            Assert.Equal("/", GetHeaderValue(postResponse.Headers, "Location"));
        }

        [Fact]
        public async void Verify_Account_Login_Invalid()
        {
            var server = CreateServer();
            var client = server.CreateClient();

            // Act
            var getResponse = await client.GetAsync("http://localhost/Account/Login");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var responseContent = await getResponse.Content.ReadAsStringAsync();

            var verificationToken = ExtractVerificationToken(responseContent);
            HttpContent requestContent = CreateLoginPost(verificationToken, "NotAUser", "NoPassword");
            AddCookiesToRequest(getResponse.Headers, requestContent.Headers);

            var postResponse = await client.PostAsync("http://localhost/Account/Login", requestContent);
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
            var postResponseContent = await getResponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("Use a local account to log in.", responseContent);
            Assert.Contains(responseContent, postResponseContent);
        }

        private string GetHeaderValue(HttpResponseHeaders headers, string name)
        {
            return headers.GetValues(name).ToList()[0];
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

        private HttpContent CreateRegisterPost(string verificationToken, string userName, string password, string confirmPassword)
        {
            List<KeyValuePair<string, string>> form = new List<KeyValuePair<string, string>>();

            form.Add(new KeyValuePair<string, string>("__RequestVerificationToken", verificationToken));
            form.Add(new KeyValuePair<string, string>("UserName", userName));
            form.Add(new KeyValuePair<string, string>("Password", password));
            form.Add(new KeyValuePair<string, string>("ConfirmPassword", confirmPassword));

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

        private string AddCookiesToRequest(HttpHeaders responseHeaders, HttpHeaders requestHeaders)
        {
            var cookiehHeaders = responseHeaders.GetValues("Set-Cookie");
            foreach (var header in cookiehHeaders)
            {
                var cookieParts = header.Split(';');
                var cookie = cookieParts[0];
                var parts = cookie.Split('=');
                requestHeaders.Add("Cookie", string.Format("{0}={1}", parts[0], parts[1]));
            }

            return String.Empty;
        }
    }
}
