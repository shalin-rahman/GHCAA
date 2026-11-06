using GHCAA.Application.DTOs.Discussion;
using GHCAA.Domain;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Extensions
{
    // Shared by every controller that hosts a comment thread (polls, meetings).
    public static class DiscussionResultExtensions
    {
        public static IActionResult ToActionResult<T>(this ControllerBase controller, DiscussionResult<T> result, Func<T?, object?>? body = null) =>
            result.Status switch
            {
                DiscussionStatus.Ok => controller.Ok(body != null ? body(result.Value) : result.Value),
                DiscussionStatus.NotFound => controller.NotFound(),
                DiscussionStatus.Forbidden => controller.Problem(detail: result.Error, statusCode: StatusCodes.Status403Forbidden),
                _ => controller.Problem(detail: result.Error, statusCode: StatusCodes.Status400BadRequest),
            };

        public static bool IsDiscussionAdmin(this ControllerBase controller) =>
            controller.User.IsInRole(Constants.Roles.Admin) || controller.User.IsInRole(Constants.Roles.SuperAdmin);
    }
}
