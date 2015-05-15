using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.OpenIdConnect;

namespace $safeprojectname$.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult SignIn()
        {
            return new ChallengeResult(
                OpenIdConnectAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" });

            //Context.Response.Challenge(new AuthenticationProperties { RedirectUri = "/" },
            //    OpenIdConnectAuthenticationDefaults.AuthenticationScheme);

            //return new ContentResult();

            // Send an OpenID Connect sign-in request.
            //if (!Context.User.Identity.IsAuthenticated)
            //{
            //    Context.Response.Challenge(new AuthenticationProperties { RedirectUri = "/" },
            //        CookieAuthenticationDefaults.AuthenticationScheme);
            //        //new string[] { OpenIdConnectAuthenticationDefaults.AuthenticationScheme,
            //        //    CookieAuthenticationDefaults.AuthenticationScheme});
            //}
        }

        public IActionResult SignOut()
        {
            string callbackUrl = Url.Action("SignOutCallback", "Account", values: null, protocol: Request.Scheme);
            Context.Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationScheme);
            Context.Authentication.SignOut(OpenIdConnectAuthenticationDefaults.AuthenticationScheme,
                new AuthenticationProperties { RedirectUri = callbackUrl });
            return new EmptyResult();
        }

        public IActionResult SignOutCallback()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                // Redirect to home page if the user is authenticated.
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}
