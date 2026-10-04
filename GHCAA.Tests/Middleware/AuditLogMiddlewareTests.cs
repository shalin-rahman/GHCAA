using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
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

        // TODO 37.13l. Nothing logs request bodies or query strings today. These tests make sure a
        // later logging change cannot start writing the count key or a ballot without notice.
        private const string Secret = "MIIEvQIBADANBgkqhkiG9w0BAQEFAASC-not-a-real-key";
        private const string ScopePrefix = "scope:";

        [Test]
        [Category("FR-39")]
        public async Task CountRequest_KeyReachesNoLogLineOrActivityRow()
        {
            var (lines, rows) = await RunAsync(ActionEndpoint(nameof(ElectionsController.Count)), "/api/elections/5/count",
                $"{{\"privateKey\":\"{Secret}\"}}");

            lines.Should().NotBeEmpty("the count route is logged like any other POST");
            rows.Should().ContainSingle();
            lines.Concat(rows).Should().NotContain(x => x.Contains(Secret));
        }

        [Test]
        [Category("FR-39")]
        public async Task VoteRequest_BallotReachesNoLogLineOrActivityRow()
        {
            var (lines, rows) = await RunAsync(VoteEndpoint(), "/api/elections/5/vote",
                $"{{\"choices\":[{{\"seatId\":1,\"nominationIds\":[42]}}],\"note\":\"{Secret}\"}}");

            lines.Where(x => !x.StartsWith(ScopePrefix)).Should().BeEmpty();
            lines.Should().NotContain(x => x.Contains(Secret));
            rows.Should().ContainSingle();
            rows.Should().NotContain(x => x.Contains(Secret) || x.Contains("nominationIds"));
        }

        // Runs the two logging middlewares the way Program.cs orders them, with the secret in the
        // body and the query string. The inner step reads the body, as model binding would.
        private static async Task<(List<string> Lines, List<string> Rows)> RunAsync(Endpoint endpoint, string path, string body)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            context.Request.Path = path;
            context.Request.QueryString = new QueryString($"?privateKey={Secret}");
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
            context.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "10"), new Claim(ClaimTypes.Name, "officer")], "test"));
            context.SetEndpoint(endpoint);

            var rows = new List<string>();
            var activity = new Mock<IActivityService>();
            activity.Setup(x => x.LogActivityAsync(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(),
                    It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>(),
                    It.IsAny<DateTime?>()))
                .Callback(new InvocationAction(i => rows.Add(string.Join("|", i.Arguments.Select(a => a?.ToString())))));
            var lines = new List<string>();

            var audit = new AuditLogMiddleware(async ctx =>
            {
                using var reader = new StreamReader(ctx.Request.Body);
                await reader.ReadToEndAsync();
                ctx.Response.StatusCode = 200;
            }, new ListLogger<AuditLogMiddleware>(lines));
            var correlation = new CorrelationIdMiddleware(ctx => audit.InvokeAsync(ctx, activity.Object),
                new ListLogger<CorrelationIdMiddleware>(lines));

            await correlation.InvokeAsync(context);
            return (lines, rows);
        }

        private sealed class ListLogger<T>(List<string> lines) : ILogger<T>
        {
            // A scope is not a log line on its own, but its values go out with every line inside it.
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            {
                if (state is IEnumerable<KeyValuePair<string, object>> values)
                    lines.AddRange(values.Select(v => $"{ScopePrefix}{v.Key}={v.Value}"));
                return null;
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                lines.Add(formatter(state, exception));
                if (state is IEnumerable<KeyValuePair<string, object?>> values)
                    lines.AddRange(values.Select(v => $"{v.Key}={v.Value}"));
            }
        }

        private static Endpoint VoteEndpoint() => ActionEndpoint(nameof(ElectionsController.Vote));

        private static Endpoint ActionEndpoint(string actionName) => new(null, new EndpointMetadataCollection(new ControllerActionDescriptor
        {
            ControllerTypeInfo = typeof(ElectionsController).GetTypeInfo(),
            ActionName = actionName,
        }), actionName);
    }
}
