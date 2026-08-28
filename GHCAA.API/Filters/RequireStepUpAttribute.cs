using GHCAA.Application.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace GHCAA.API.Filters
{
    /// <summary>
    /// 7.13: Requires a recent OTP step-up on top of the caller's normal role authorization.
    /// Applied to destructive/financial/identity admin actions so a stolen or left-open admin
    /// session cannot execute them without access to the account's email inbox.
    /// Returns 403 with Code = STEP_UP_REQUIRED, which the Angular interceptor uses to prompt.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequireStepUpAttribute : Attribute, IAsyncActionFilter
    {
        public const string ErrorCode = "STEP_UP_REQUIRED";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var claim = context.HttpContext.User.FindFirst(StepUpClaim.Type)?.Value;

            if (!long.TryParse(claim, out var verifiedAtEpoch))
            {
                context.Result = Denied("Additional verification is required for this action.");
                return;
            }

            var config = context.HttpContext.RequestServices.GetService<IConfiguration>();
            var ttlMinutes = int.TryParse(config?["AppSettings:StepUpTtlMinutes"], out var v) && v > 0
                ? v
                : StepUpClaim.DefaultTtlMinutes;

            if (!StepUpClaim.IsValid(verifiedAtEpoch, ttlMinutes))
            {
                context.Result = Denied("Your verification has expired. Please verify again.");
                return;
            }

            await next();
        }

        private static ObjectResult Denied(string message) =>
            new(new { Code = ErrorCode, Message = message }) { StatusCode = StatusCodes.Status403Forbidden };
    }
}
