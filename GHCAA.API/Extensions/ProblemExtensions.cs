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
