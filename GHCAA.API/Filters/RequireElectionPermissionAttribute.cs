using System.Security.Claims;
using GHCAA.API.Extensions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static GHCAA.Domain.Enums;

namespace GHCAA.API.Filters
{
    /// <summary>
    /// Spec 023 (37.12e): lets the action run only when the caller holds <see cref="Needed"/> on the
    /// election the route points at. The role policy on the action still runs first; this narrows it
    /// to one election. Denials are 403 ProblemDetails with Extensions["code"] = ELECTION_PERMISSION.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RequireElectionPermissionAttribute(ElectionPermission needed, ElectionIdLookup lookup = ElectionIdLookup.Election)
        : Attribute, IAsyncActionFilter
    {
        public const string ErrorCode = Constants.ErrorCodes.ElectionPermission;

        public ElectionPermission Needed { get; } = needed;
        public ElectionIdLookup Lookup { get; } = lookup;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;
            if (!int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (!TryReadRouteId(context, out var routeId))
            {
                context.Result = new NotFoundResult();
                return;
            }

            var access = context.HttpContext.RequestServices.GetRequiredService<IElectionAccessService>();
            var ct = context.HttpContext.RequestAborted;
            var electionId = await access.ElectionIdForAsync(Lookup, routeId, ct);
            if (electionId is null)
            {
                context.Result = new NotFoundResult();
                return;
            }

            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            if (!await access.HasAsync(electionId.Value, userId, roles, Needed, ct))
            {
                context.Result = new ObjectResult(ProblemExtensions.BuildProblemDetails(
                    ErrorCode, $"You need the {Needed} permission on this election.", StatusCodes.Status403Forbidden))
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            await next();
        }

        // A route names its id after what it points at, or just "id".
        private bool TryReadRouteId(ActionExecutingContext context, out int id)
        {
            var named = Lookup switch
            {
                ElectionIdLookup.Nomination => "nominationId",
                ElectionIdLookup.Appointment => "appointmentId",
                _ => "electionId",
            };
            var values = context.RouteData.Values;
            var raw = values.TryGetValue(named, out var v) ? v : values.GetValueOrDefault("id");
            return int.TryParse(raw?.ToString(), out id);
        }
    }
}
