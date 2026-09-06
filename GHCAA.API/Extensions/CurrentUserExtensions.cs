using System.Security.Claims;
using GHCAA.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Extensions
{
    // 82.5: one place for "who is calling", replacing 55 inline User.FindFirst/FindFirstValue
    // reads that were copied controller to controller. Each accessor returns exactly what the
    // inline claim read it replaces returned (the raw claim string, or null if absent) — callers
    // keep their own TryParse/null handling unchanged. This is a refactor only; Work Package 24
    // already settled what happens when a claim is missing, and that behaviour is not touched here.
    public static class CurrentUserExtensions
    {
        public static string? CurrentMemberIdRaw(this ControllerBase controller)
            => controller.User.FindFirst(AppClaimTypes.MemberId)?.Value;

        public static string? CurrentUserIdRaw(this ControllerBase controller)
            => controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public static string? CurrentUsername(this ControllerBase controller)
            => controller.User.FindFirst(ClaimTypes.Name)?.Value;

        public static string? CurrentRole(this ControllerBase controller)
            => controller.User.FindFirst(ClaimTypes.Role)?.Value;
    }
}
