using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Extensions
{
    // 82.97: AuthController and ProfileController each had their own copy of this
    // cookie-setting logic. One helper so the two auth-cookie writers can't drift.
    public static class AuthCookieExtensions
    {
        public static void SetAuthCookie(this ControllerBase controller, IWebHostEnvironment env, string name, string value, TimeSpan maxAge)
        {
            controller.Response.Cookies.Append(name, value, new CookieOptions
            {
                HttpOnly = true,
                Secure = !env.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                MaxAge = maxAge,
                Path = "/"
            });
        }

        // 1a: Readable (non-httpOnly) double-submit-cookie token. Angular's HttpClient
        // reads this and echoes it back as the X-XSRF-TOKEN header; XsrfMiddleware
        // validates the two match on state-changing requests.
        public static void SetXsrfCookie(this ControllerBase controller, IWebHostEnvironment env, TimeSpan maxAge)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            controller.Response.Cookies.Append("XSRF-TOKEN", token, new CookieOptions
            {
                HttpOnly = false,
                Secure = !env.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                MaxAge = maxAge,
                Path = "/"
            });
        }
    }
}
