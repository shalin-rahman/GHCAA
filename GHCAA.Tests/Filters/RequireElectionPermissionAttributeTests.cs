using System.Security.Claims;
using FluentAssertions;
using GHCAA.API.Filters;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Filters;

[TestFixture]
[Category("FR-39")]
public class RequireElectionPermissionAttributeTests
{
    private Mock<IElectionAccessService> _access = null!;

    [SetUp]
    public void SetUp() => _access = new Mock<IElectionAccessService>();

    private ActionExecutingContext BuildContext(RouteValueDictionary route, string? userId = "5")
    {
        var claims = new List<Claim> { new(ClaimTypes.Role, Constants.Roles.ElectionOfficial) };
        if (userId != null)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));

        var services = new ServiceCollection();
        services.AddSingleton(_access.Object);
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")),
            RequestServices = services.BuildServiceProvider()
        };

        return new ActionExecutingContext(
            new ActionContext(httpContext, new RouteData(route), new ActionDescriptor()),
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: null!);
    }

    private static async Task<bool> RunAsync(RequireElectionPermissionAttribute filter, ActionExecutingContext context)
    {
        var nextCalled = false;
        await filter.OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        });
        return nextCalled;
    }

    [Test]
    public async Task Denied_Returns403_WithTheCodeAndPermissionName()
    {
        _access.Setup(a => a.ElectionIdForAsync(ElectionIdLookup.Election, 3, It.IsAny<CancellationToken>())).ReturnsAsync(3);
        _access.Setup(a => a.HasAsync(3, 5, It.IsAny<IReadOnlyCollection<string>>(), ElectionPermission.Count, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var context = BuildContext(new RouteValueDictionary { ["id"] = "3" });

        var ran = await RunAsync(new RequireElectionPermissionAttribute(ElectionPermission.Count), context);

        ran.Should().BeFalse();
        var result = context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        var problem = result.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Extensions["code"].Should().Be(Constants.ErrorCodes.ElectionPermission);
        problem.Detail.Should().Contain(nameof(ElectionPermission.Count));
    }

    [Test]
    public async Task NominationRoute_ResolvesTheElection_AndPassesTheRoles()
    {
        _access.Setup(a => a.ElectionIdForAsync(ElectionIdLookup.Nomination, 12, It.IsAny<CancellationToken>())).ReturnsAsync(3);
        _access.Setup(a => a.HasAsync(3, 5, It.Is<IReadOnlyCollection<string>>(r => r.Contains(Constants.Roles.ElectionOfficial)), ElectionPermission.DecideNominations, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var context = BuildContext(new RouteValueDictionary { ["nominationId"] = "12" });

        var ran = await RunAsync(new RequireElectionPermissionAttribute(ElectionPermission.DecideNominations, ElectionIdLookup.Nomination), context);

        ran.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    // Revoke's route is elections/appointments/{id}/revoke, so "id" is the appointment, not the election.
    [Test]
    public async Task AppointmentLookup_ReadsIdAsTheAppointment()
    {
        _access.Setup(a => a.ElectionIdForAsync(ElectionIdLookup.Appointment, 40, It.IsAny<CancellationToken>())).ReturnsAsync(3);
        _access.Setup(a => a.HasAsync(3, 5, It.IsAny<IReadOnlyCollection<string>>(), ElectionPermission.AppointOfficials, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var context = BuildContext(new RouteValueDictionary { ["id"] = "40" });

        var ran = await RunAsync(new RequireElectionPermissionAttribute(ElectionPermission.AppointOfficials, ElectionIdLookup.Appointment), context);

        ran.Should().BeTrue();
        _access.Verify(a => a.HasAsync(40, It.IsAny<int>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<ElectionPermission>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task UnknownNomination_Returns404()
    {
        _access.Setup(a => a.ElectionIdForAsync(ElectionIdLookup.Nomination, 12, It.IsAny<CancellationToken>())).ReturnsAsync((int?)null);
        var context = BuildContext(new RouteValueDictionary { ["nominationId"] = "12" });

        var ran = await RunAsync(new RequireElectionPermissionAttribute(ElectionPermission.DecideNominations, ElectionIdLookup.Nomination), context);

        ran.Should().BeFalse();
        context.Result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task NoUserId_Returns401()
    {
        var context = BuildContext(new RouteValueDictionary { ["id"] = "3" }, userId: null);

        var ran = await RunAsync(new RequireElectionPermissionAttribute(ElectionPermission.Count), context);

        ran.Should().BeFalse();
        context.Result.Should().BeOfType<UnauthorizedResult>();
    }
}
