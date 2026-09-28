using System;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.API.Middleware;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Middleware
{
    [TestFixture]
    public class AuditLogMiddlewareTests
    {
        // Spec 023 FR-004: a time of day on the vote audit row would narrow down the ballot batch.
        [Test]
        [Category("FR-39")]
        public async Task VoteRequest_LogsTheDateOnly()
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "10")], "test"));
            context.SetEndpoint(VoteEndpoint());

            var activity = new Mock<IActivityService>();
            var middleware = new AuditLogMiddleware(ctx => { ctx.Response.StatusCode = 200; return Task.CompletedTask; },
                NullLogger<AuditLogMiddleware>.Instance);

            await middleware.InvokeAsync(context, activity.Object);

            activity.Verify(x => x.LogActivityAsync(null, Constants.Elections.VoteAuditType, It.IsAny<string>(), 10,
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>(),
                It.Is<DateTime?>(t => t.HasValue && t.Value.TimeOfDay == TimeSpan.Zero)), Times.Once);
            activity.Verify(x => x.LogActivityAsync(It.IsAny<int?>(), "ApiAction", It.IsAny<string>(), It.IsAny<int?>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>(),
                It.IsAny<DateTime?>()), Times.Never);
        }

        // Spec 023 FR-004: the request log would otherwise carry the member id and exact vote time.
        [Test]
        [Category("FR-39")]
        public async Task VoteRequest_WritesNoRequestLogLine()
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            context.SetEndpoint(VoteEndpoint());
            var logger = new Mock<ILogger<CorrelationIdMiddleware>>();
            var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask, logger.Object);

            await middleware.InvokeAsync(context);

            logger.Verify(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
        }

        private static Endpoint VoteEndpoint() => new(null, new EndpointMetadataCollection(new ControllerActionDescriptor
        {
            ControllerTypeInfo = typeof(ElectionsController).GetTypeInfo(),
            ActionName = nameof(ElectionsController.Vote),
        }), "vote");
    }
}
