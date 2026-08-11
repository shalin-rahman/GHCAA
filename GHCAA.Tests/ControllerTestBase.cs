using System.Security.Claims;
using GHCAA.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests
{
    public class ControllerTestBase : TestBase
    {
        protected void SetUserContext(ControllerBase controller, int? memberId = 1, string role = "Admin", int? userId = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, role)
            };

            if (userId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
            }
            else if (memberId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, memberId.Value.ToString()));
            }

            if (memberId.HasValue)
            {
                claims.Add(new Claim("MemberId", memberId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        protected void SetSuperAdminContext(ControllerBase controller, int memberId = 1)
        {
            SetUserContext(controller, memberId, "SuperAdmin");
        }

        protected void SetMemberContext(ControllerBase controller, int memberId = 100)
        {
            SetUserContext(controller, memberId, "Member");
        }
    }
}
