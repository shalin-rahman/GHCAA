using FluentAssertions;
using GHCAA.API.Filters;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;

namespace GHCAA.Tests.Filters;

[TestFixture]
[Category("FR-39")]
public class ElectionRulesFrozenFilterTests
{
    private static ExceptionContext ContextFor(Exception ex) =>
        new(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()), []) { Exception = ex };

    [Test]
    public void FrozenException_BecomesConflictWithCode()
    {
        var context = ContextFor(new ElectionRulesFrozenException(3, "Board 2027"));

        new ElectionRulesFrozenFilter().OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        var result = context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        var problem = result.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Extensions["code"].Should().Be(Constants.ErrorCodes.ElectionRulesFrozen);
        problem.Detail.Should().Contain("Board 2027");
    }

    [Test]
    public void OtherException_IsLeftAlone()
    {
        var context = ContextFor(new InvalidOperationException("other"));

        new ElectionRulesFrozenFilter().OnException(context);

        context.ExceptionHandled.Should().BeFalse();
        context.Result.Should().BeNull();
    }
}
