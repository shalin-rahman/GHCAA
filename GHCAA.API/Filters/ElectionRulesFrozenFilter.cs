using GHCAA.API.Extensions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GHCAA.API.Filters
{
    // Spec 023 (37.13q). The freeze check runs inside several services, so one global filter turns
    // its exception into a 409 rather than a try/catch in every action that can hit it.
    public sealed class ElectionRulesFrozenFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is not ElectionRulesFrozenException ex) return;
            var problem = ProblemExtensions.BuildProblemDetails(Constants.ErrorCodes.ElectionRulesFrozen, ex.Message, StatusCodes.Status409Conflict);
            context.Result = new ObjectResult(problem) { StatusCode = StatusCodes.Status409Conflict };
            context.ExceptionHandled = true;
        }
    }
}
