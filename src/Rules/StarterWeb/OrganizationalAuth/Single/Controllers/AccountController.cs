using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace $safeprojectname$.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult SignIn()
        {
            return Challenge(
                OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" });
        }

        public async Task<IActionResult> SignOut()
        {
            var callbackUrl = Url.Action("SignOutCallback", "Account", values: null, protocol: Request.Scheme);
            await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.Authentication.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
                new AuthenticationProperties { RedirectUri = callbackUrl });
            return new EmptyResult();
        }

        public IActionResult SignOutCallback()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                // Redirect to home page if the user is authenticated.
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            return View();
        }
    }
}
