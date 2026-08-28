using System.Security.Claims;
using FluentAssertions;
using GHCAA.API.Filters;
using GHCAA.Application.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GHCAA.Tests.Filters;

[TestFixture]
public class RequireStepUpAttributeTests
{
    private static ActionExecutingContext BuildContext(string? stepUpClaimValue, int? ttlMinutes = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
        if (stepUpClaimValue != null)
            claims.Add(new Claim(StepUpClaim.Type, stepUpClaimValue));

        var services = new ServiceCollection();
        var settings = new Dictionary<string, string?>();
        if (ttlMinutes.HasValue)
            settings["AppSettings:StepUpTtlMinutes"] = ttlMinutes.Value.ToString();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(settings).Build());

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")),
            RequestServices = services.BuildServiceProvider()
        };

        return new ActionExecutingContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: null!);
    }

    private static long EpochMinutesAgo(int minutes) =>
        DateTimeOffset.UtcNow.AddMinutes(-minutes).ToUnixTimeSeconds();

    [Test]
    public async Task Denies_WhenStepUpClaimMissing()
    {
        var context = BuildContext(stepUpClaimValue: null);
        var nextCalled = false;

        await new RequireStepUpAttribute().OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        });

        nextCalled.Should().BeFalse();
        var result = context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Test]
    public async Task Denies_WhenStepUpClaimIsNotANumber()
    {
        var context = BuildContext("not-a-timestamp");

        await new RequireStepUpAttribute().OnActionExecutionAsync(context, () =>
            Task.FromResult<ActionExecutedContext>(null!));

        context.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Test]
    public async Task Denies_WhenStepUpClaimIsExpired()
    {
        // Default TTL is now 30 days (per the "long grace period since last login" model), so
        // "expired" has to mean well past that, not the old 15-minute window.
        var context = BuildContext(EpochMinutesAgo(31 * 24 * 60).ToString());

        await new RequireStepUpAttribute().OnActionExecutionAsync(context, () =>
            Task.FromResult<ActionExecutedContext>(null!));

        context.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Test]
    public async Task Allows_WhenStepUpClaimIsRecent()
    {
        var context = BuildContext(EpochMinutesAgo(2).ToString());
        var nextCalled = false;

        await new RequireStepUpAttribute().OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        });

        nextCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    [Test]
    public async Task RespectsConfiguredTtl_WhenLongerThanDefault()
    {
        // 45 minutes old would fail the 15-minute default, but the configured TTL is 60.
        var context = BuildContext(EpochMinutesAgo(45).ToString(), ttlMinutes: 60);
        var nextCalled = false;

        await new RequireStepUpAttribute().OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        });

        nextCalled.Should().BeTrue();
    }

    [Test]
    public async Task DeniedResponse_CarriesStepUpRequiredCode()
    {
        var context = BuildContext(stepUpClaimValue: null);

        await new RequireStepUpAttribute().OnActionExecutionAsync(context, () =>
            Task.FromResult<ActionExecutedContext>(null!));

        var payload = context.Result.Should().BeOfType<ObjectResult>().Subject.Value;
        payload.Should().NotBeNull();
        payload!.GetType().GetProperty("Code")!.GetValue(payload)
            .Should().Be(RequireStepUpAttribute.ErrorCode);
    }
}
