using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml.Linq;
using StarterWeb.IndividualAuth;
using Xunit;

namespace Microsoft.Web.Templates.Tests
{
    public class StarterWebIndividualAuthTests : TemplateTestBase
    {
        private static readonly string _templateName = "StarterWeb.IndividualAuth";
        private static readonly string IdentityCookieName = ".AspNet.Microsoft.AspNet.Identity.Application";

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

            HttpContent requestContent = CreateRegisterPost(verificationToken, "testUser@ms.com", "Asd!123$$", "Asd!123$$");
            AddCookiesToRequest(getResponse.Headers, requestContent.Headers);

            var postResponse = await client.PostAsync("http://localhost/Account/Register", requestContent);
            var postResponseContent = await getResponse.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
            Assert.Equal("/", GetHeaderValue(postResponse.Headers, "Location"));

            // Grab the auth cookie
            string authCookie = GetAuthCookie(postResponse.Headers);
            Assert.NotEqual(String.Empty, authCookie);

            AddAuthCookie(client.DefaultRequestHeaders, authCookie);

            // Verify manage page
            var manageResponse = await client.GetAsync("http://localhost/Manage");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var manageContent = await manageResponse.Content.ReadAsStringAsync();
            Assert.Contains("Hello testUser@ms.com", manageContent);
            verificationToken = ExtractVerificationToken(manageContent);

            // Verify Logoff
            HttpContent logoffRequestContent = CreateLogOffPost(verificationToken);
            AddCookiesToRequest(manageResponse.Headers, logoffRequestContent.Headers);

            var logoffResponse = await client.PostAsync("http://localhost/Account/LogOff", logoffRequestContent);
            Assert.Equal(HttpStatusCode.Redirect, logoffResponse.StatusCode);
            Assert.Equal("/", GetHeaderValue(logoffResponse.Headers, "Location"));

            // Grab the auth cookie
            authCookie = GetAuthCookie(logoffResponse.Headers);
            Assert.Equal(String.Empty, authCookie);

            var logoffContent = await logoffResponse.Content.ReadAsStringAsync();
            Assert.Equal(String.Empty, logoffContent);

            // Verify relogin
            client = server.CreateClient();

            var loginResponse = await client.GetAsync("http://localhost/Account/Login");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();

            verificationToken = ExtractVerificationToken(responseContent);
            HttpContent loginRequestContent = CreateLoginPost(verificationToken, "testUser@ms.com", "Asd!123$$");
            AddCookiesToRequest(getResponse.Headers, loginRequestContent.Headers);

            var loginPostResponse = await client.PostAsync("http://localhost/Account/Login", loginRequestContent);
            Assert.Equal(HttpStatusCode.Redirect, loginPostResponse.StatusCode);
            Assert.Equal("/", GetHeaderValue(loginPostResponse.Headers, "Location"));

            // Grab the auth cookie
            authCookie = GetAuthCookie(loginPostResponse.Headers);
            Assert.NotEqual(String.Empty, authCookie);
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

        private HttpContent CreateLoginPost(string verificationToken, string email, string password, bool rememberMe = false)
        {
            List<KeyValuePair<string, string>> form = new List<KeyValuePair<string, string>>();

            form.Add(new KeyValuePair<string, string>("__RequestVerificationToken", verificationToken));
            form.Add(new KeyValuePair<string, string>("Email", email));
            form.Add(new KeyValuePair<string, string>("Password", password));
            form.Add(new KeyValuePair<string, string>("RememberMe", rememberMe.ToString()));

            var content = new FormUrlEncodedContent(form);

            return content;
        }

        private HttpContent CreateRegisterPost(string verificationToken, string email, string password, string confirmPassword)
        {
            List<KeyValuePair<string, string>> form = new List<KeyValuePair<string, string>>();

            form.Add(new KeyValuePair<string, string>("__RequestVerificationToken", verificationToken));
            form.Add(new KeyValuePair<string, string>("Email", email));
            form.Add(new KeyValuePair<string, string>("Password", password));
            form.Add(new KeyValuePair<string, string>("ConfirmPassword", confirmPassword));

            var content = new FormUrlEncodedContent(form);

            return content;
        }

        private HttpContent CreateLogOffPost(string verificationToken)
        {
            List<KeyValuePair<string, string>> form = new List<KeyValuePair<string, string>>();

            form.Add(new KeyValuePair<string, string>("__RequestVerificationToken", verificationToken));

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

        private void AddCookiesToRequest(HttpHeaders responseHeaders, HttpHeaders requestHeaders)
        {
            var cookiehHeaders = responseHeaders.GetValues("Set-Cookie");
            foreach (var header in cookiehHeaders)
            {
                var cookieParts = header.Split(';');
                var cookie = cookieParts[0];
                var parts = cookie.Split('=');
                requestHeaders.Add("Cookie", String.Format("{0}={1}", parts[0], parts[1]));
            }
        }

        private string GetAuthCookie(HttpHeaders responseHeaders)
        {
            var cookiehHeaders = responseHeaders.GetValues("Set-Cookie");
            foreach (var header in cookiehHeaders)
            {
                var cookieParts = header.Split(';');
                var cookie = cookieParts[0];
                var parts = cookie.Split('=');
                if (parts[0].Equals(IdentityCookieName, StringComparison.OrdinalIgnoreCase))
                {
                    return parts[1];
                }
            }

            return String.Empty;
        }

        private void AddAuthCookie(HttpHeaders requestHeaders, string cookieValue)
        {
            requestHeaders.Add("Cookie", String.Format("{0}={1}", IdentityCookieName, cookieValue));
        }
    }
}
