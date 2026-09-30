using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Extensions
{
    // 84.45: adds a machine-readable "code" to a ProblemDetails body, for the failures a client
    // actually branches on (step-up required, rate limited, and so on). Everything else keeps
    // using the plain Problem(detail:, statusCode:) from ControllerBase — a code is only worth
    // adding when something on the other end reads it.
    public static class ProblemExtensions
    {
        public static ObjectResult ProblemWithCode(this ControllerBase controller, string code, string detail, int statusCode)
        {
            var problem = BuildProblemDetails(code, detail, statusCode);
            return new ObjectResult(problem) { StatusCode = statusCode };
        }

        // Spec 023 (37.12f). The reply when a two-person step was stored rather than run, or null
        // when it ran or failed, so the caller maps its own errors.
        public static IActionResult? ApprovalReply(this ControllerBase controller, GHCAA.Application.DTOs.ElectionApprovalRunResult run) =>
            run.Pending is not null ? controller.Accepted(run.Pending)
            : run.Error == "already-pending" ? controller.ProblemWithCode(GHCAA.Domain.Constants.ErrorCodes.ApprovalPending, "This step is already waiting for a second person.", StatusCodes.Status409Conflict)
            : null;

        // Filters and middleware run outside a ControllerBase, so they build the same shape here
        // instead of going through the extension method above.
        public static ProblemDetails BuildProblemDetails(string code, string detail, int statusCode)
        {
            var problem = new ProblemDetails
            {
                Detail = detail,
                Status = statusCode,
            };
            problem.Extensions["code"] = code;
            return problem;
        }
    }
}
