using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace Microsoft.Web.Templates.Tests
{
    public class StarterWebIndividualAuthTests : IClassFixture<TemplateTestFixure<StarterWeb.IndividualAuth.Startup>>
    {
        private static readonly string IdentityCookieName = ".AspNetCore.Identity.Application";
        public HttpClient Client { get; private set; }
        private TemplateTestFixure<StarterWeb.IndividualAuth.Startup> _fixture;

        public StarterWebIndividualAuthTests(TemplateTestFixure<StarterWeb.IndividualAuth.Startup> fixture) : base()
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
            Assert.Contains("Home Page - StarterWeb.IndividualAuth", reponseContent);
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

        [Fact]
        public async void Verify_Account_Register_Get()
        {
            // Act
            var getReponse = await Client.GetAsync("http://localhost/Account/Register");
            Assert.Equal(HttpStatusCode.OK, getReponse.StatusCode);

            var reponseContent = await getReponse.Content.ReadAsStringAsync();
            Assert.Contains("Create a new account.", reponseContent);
        }

        [Fact]
        public async void Verify_Account_Register_CreateAccount()
        {
            var testUser = GetUniqueUserId();

            // Act
            var getResponse = await Client.GetAsync("http://localhost/Account/Register");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var responseContent = await getResponse.Content.ReadAsStringAsync();
            Assert.Contains("Create a new account.", responseContent);

            var verificationToken = ExtractVerificationToken(responseContent);

            HttpContent requestContent = CreateRegisterPost(verificationToken, testUser, "Asd!123$$", "Asd!123$$");
            AddCookiesToRequest(getResponse.Headers, requestContent.Headers);

            var postResponse = await Client.PostAsync("http://localhost/Account/Register", requestContent);
            var postResponseContent = await getResponse.Content.ReadAsStringAsync();

            //           Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
            Assert.Equal("/", GetHeaderValue(postResponse.Headers, "Location"));

            // Grab the auth cookie
            string authCookie = GetAuthCookie(postResponse.Headers);
            Assert.NotEqual(String.Empty, authCookie);

            AddAuthCookie(Client.DefaultRequestHeaders, authCookie);

            // Verify manage page
            var manageResponse = await Client.GetAsync("http://localhost/Manage");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var manageContent = await manageResponse.Content.ReadAsStringAsync();
            Assert.Contains("Hello " + testUser, manageContent);
            verificationToken = ExtractVerificationToken(manageContent);

            // Verify Logoff
            HttpContent logoffRequestContent = CreateLogOffPost(verificationToken);
            AddCookiesToRequest(manageResponse.Headers, logoffRequestContent.Headers);

            var logoffResponse = await Client.PostAsync("http://localhost/Account/LogOff", logoffRequestContent);
            Assert.Equal(HttpStatusCode.Redirect, logoffResponse.StatusCode);
            Assert.Equal("/", GetHeaderValue(logoffResponse.Headers, "Location"));

            // Grab the auth cookie
            authCookie = GetAuthCookie(logoffResponse.Headers);
            Assert.Equal(String.Empty, authCookie);

            var logoffContent = await logoffResponse.Content.ReadAsStringAsync();
            Assert.Equal(String.Empty, logoffContent);

            // Verify relogin
            Client = _fixture.CreateClient();

            var loginResponse = await Client.GetAsync("http://localhost/Account/Login");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();

            verificationToken = ExtractVerificationToken(responseContent);
            HttpContent loginRequestContent = CreateLoginPost(verificationToken, testUser, "Asd!123$$");
            AddCookiesToRequest(getResponse.Headers, loginRequestContent.Headers);

            var loginPostResponse = await Client.PostAsync("http://localhost/Account/Login", loginRequestContent);
            Assert.Equal(HttpStatusCode.Redirect, loginPostResponse.StatusCode);
            Assert.Equal("/", GetHeaderValue(loginPostResponse.Headers, "Location"));

            // Grab the auth cookie
            authCookie = GetAuthCookie(loginPostResponse.Headers);
            Assert.NotEqual(String.Empty, authCookie);
        }

        [Fact]
        public async void Verify_Account_Login_Invalid()
        {
            // Act
            var getResponse = await Client.GetAsync("http://localhost/Account/Login");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var responseContent = await getResponse.Content.ReadAsStringAsync();

            var verificationToken = ExtractVerificationToken(responseContent);
            HttpContent requestContent = CreateLoginPost(verificationToken, "NotAUser", "NoPassword");
            AddCookiesToRequest(getResponse.Headers, requestContent.Headers);

            var postResponse = await Client.PostAsync("http://localhost/Account/Login", requestContent);
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
            // <input name="__RequestVerificationToken" type="hidden" value="CfDJ8B_OdGUvnqVKs5KuFSYfNbZTz5af_gv-85B9D1Lf88Ze87ZGq8FG6HLvHxxLmP_UU8SdDNmECMJpFsrDPlzIHBZt_yUajSJLkbVOqlZd59J8eK_90825xgVCf-sGoepijEmxZKu_kggNeONAqxMoIQQ" /></form>
            string tokenPattern = @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""(?<token>.*?)"" />";
            string token = string.Empty;

            Regex ex = new Regex(tokenPattern);
            var m = ex.Match(response);
            if (m.Success)
            {
                token = m.Groups["token"].Captures[0].Value;
            }

            return token;
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

        private string GetUniqueUserId()
        {
            return string.Format("testUser{0}@ms.com", Guid.NewGuid().ToString());
        }
    }
}
