using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GHCAA.API.Middleware;
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
        private static async Task<(HttpContext Context, string Body, string? LoggedMessage)> InvokeAsync(
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

            var middleware = new ExceptionMiddleware(
                _ => throw exceptionToThrow,
                loggerMock.Object,
                envMock.Object);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/api/test";
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exceptionToThrow,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            return (context, body, loggedMessage);
        }

        [Test]
        public async Task InvokeAsync_UnhandledException_Returns500()
        {
            var (context, _, _) = await InvokeAsync("Production", new InvalidOperationException("boom"));

            Assert.That(context.Response.StatusCode, Is.EqualTo(500));
            Assert.That(context.Response.ContentType, Is.EqualTo("application/json"));
        }

        [Test]
        public async Task InvokeAsync_Production_HidesExceptionDetailsFromResponseBody()
        {
            var (_, body, _) = await InvokeAsync("Production", new InvalidOperationException("sensitive db connection string leaked here"));

            var payload = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.That(payload.GetProperty("message").GetString(), Is.EqualTo("Internal Server Error"));
            Assert.That(payload.TryGetProperty("details", out var details) && details.ValueKind != JsonValueKind.Null, Is.False);
        }

        [Test]
        public async Task InvokeAsync_Development_IncludesExceptionMessageAndStackTrace()
        {
            var (_, body, _) = await InvokeAsync("Development", new InvalidOperationException("boom"));

            var payload = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.That(payload.GetProperty("message").GetString(), Is.EqualTo("boom"));
        }

        [Test]
        public async Task InvokeAsync_LogsWithFixedTemplate_NotRawExceptionMessageAsTemplate()
        {
            var (_, _, loggedMessage) = await InvokeAsync("Production", new InvalidOperationException("boom"));

            Assert.That(loggedMessage, Does.Contain("Unhandled exception processing"));
            Assert.That(loggedMessage, Does.Contain("GET"));
            Assert.That(loggedMessage, Does.Contain("/api/test"));
        }
    }
}
