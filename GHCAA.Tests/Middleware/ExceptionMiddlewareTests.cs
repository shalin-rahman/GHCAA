using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Middleware;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Middleware
{
    [TestFixture]
    public class ExceptionMiddlewareTests
    {
        private static async Task<(HttpContext Context, string Body, string? LoggedMessage, Mock<IErrorLogService> ErrorLogService)> InvokeAsync(
            string environmentName, Exception exceptionToThrow)
        {
            var envMock = new Mock<IHostEnvironment>();
            envMock.Setup(e => e.EnvironmentName).Returns(environmentName);

            string? loggedMessage = null;
            var loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
            loggerMock
                .Setup(l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback(new InvocationAction(invocation =>
                {
                    var state = invocation.Arguments[2];
                    var formatter = invocation.Arguments[4];
                    var invokeMethod = formatter.GetType().GetMethod("Invoke")!;
                    loggedMessage = (string?)invokeMethod.Invoke(formatter, new[] { state, invocation.Arguments[3] });
                }));

            var errorLogServiceMock = new Mock<IErrorLogService>();
            errorLogServiceMock
                .Setup(s => s.LogAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int?>(),
                    It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var middleware = new ExceptionMiddleware(
                _ => throw exceptionToThrow,
                loggerMock.Object,
                envMock.Object);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/api/test";
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context, errorLogServiceMock.Object);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exceptionToThrow,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            return (context, body, loggedMessage, errorLogServiceMock);
        }

        [Test]
        public async Task InvokeAsync_UnhandledException_Returns500()
        {
            var (context, _, _, _) = await InvokeAsync("Production", new InvalidOperationException("boom"));

            Assert.That(context.Response.StatusCode, Is.EqualTo(500));
            // 82.4: unhandled exceptions now carry the same ProblemDetails (RFC 7807) shape as
            // every controller failure path.
            Assert.That(context.Response.ContentType, Is.EqualTo("application/problem+json"));
        }

        [Test]
        public async Task InvokeAsync_Production_HidesExceptionDetailsFromResponseBody()
        {
            var (_, body, _, _) = await InvokeAsync("Production", new InvalidOperationException("sensitive db connection string leaked here"));

            var payload = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.That(payload.GetProperty("detail").GetString(), Is.EqualTo("Internal Server Error"));
            Assert.That(payload.TryGetProperty("stackTrace", out var stackTrace) && stackTrace.ValueKind != JsonValueKind.Null, Is.False);
        }

        [Test]
        public async Task InvokeAsync_Development_IncludesExceptionMessageAndStackTrace()
        {
            var (_, body, _, _) = await InvokeAsync("Development", new InvalidOperationException("boom"));

            var payload = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.That(payload.GetProperty("detail").GetString(), Is.EqualTo("boom"));
            Assert.That(payload.GetProperty("stackTrace").ValueKind, Is.Not.EqualTo(JsonValueKind.Null));
        }

        [Test]
        public async Task InvokeAsync_LogsWithFixedTemplate_NotRawExceptionMessageAsTemplate()
        {
            var (_, _, loggedMessage, _) = await InvokeAsync("Production", new InvalidOperationException("boom"));

            Assert.That(loggedMessage, Does.Contain("Unhandled exception processing"));
            Assert.That(loggedMessage, Does.Contain("GET"));
            Assert.That(loggedMessage, Does.Contain("/api/test"));
        }

        [Test]
        public async Task InvokeAsync_PersistsErrorLog_WithRequestContextAndException()
        {
            // 45.3: ExceptionMiddleware must hand the caught exception to IErrorLogService with
            // enough context (path/method/exception type) for the admin screen to filter on it.
            var (_, _, _, errorLogService) = await InvokeAsync("Production", new InvalidOperationException("boom"));

            errorLogService.Verify(s => s.LogAsync(
                GHCAA.Domain.Constants.ErrorLogs.LevelError,
                "boom",
                typeof(InvalidOperationException).FullName,
                It.IsAny<string?>(),
                "ExceptionMiddleware",
                "/api/test",
                "GET",
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public void InvokeAsync_ResponseStillSucceeds_WhenErrorLogServiceThrows()
        {
            // 45.3: a failing/slow logging write must never become a new source of request
            // failure for the request that's already failing.
            var envMock = new Mock<IHostEnvironment>();
            envMock.Setup(e => e.EnvironmentName).Returns("Production");
            var loggerMock = new Mock<ILogger<ExceptionMiddleware>>();

            var errorLogServiceMock = new Mock<IErrorLogService>();
            errorLogServiceMock
                .Setup(s => s.LogAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int?>(),
                    It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("db unavailable"));

            var middleware = new ExceptionMiddleware(
                _ => throw new InvalidOperationException("original failure"),
                loggerMock.Object,
                envMock.Object);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/api/test";
            context.Response.Body = new MemoryStream();

            // IErrorLogService.LogAsync is documented to never throw, but even if it somehow did,
            // ExceptionMiddleware must still be able to write its response rather than crash the
            // pipeline with a second, unrelated exception.
            Assert.That(async () => await middleware.InvokeAsync(context, errorLogServiceMock.Object), Throws.Nothing);
        }
    }
}
