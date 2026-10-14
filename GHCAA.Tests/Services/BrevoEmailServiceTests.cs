using System.Net;
using System.Text.Json;
using FluentAssertions;
using GHCAA.Infrastructure.Options;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace GHCAA.Tests.Services;

[TestFixture]
public class BrevoEmailServiceTests
{
    private const string ApiKey = "test-key";
    private const string SenderEmail = "sender@example.com";

    private Mock<ILogger<BrevoEmailService>> _mockLogger = null!;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<BrevoEmailService>>();
    }

    [TestCase(null, SenderEmail)]
    [TestCase("", SenderEmail)]
    [TestCase(ApiKey, null)]
    [TestCase(ApiKey, " ")]
    public void Constructor_WithMissingSetting_ShouldThrowException(string? apiKey, string? senderEmail)
    {
        var settings = Options.Create(new BrevoSettingsOptions { ApiKey = apiKey, SenderEmail = senderEmail });

        var act = () => new BrevoEmailService(new HttpClient(new StubHandler(HttpStatusCode.Created)), settings, _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public async Task SendEmailAsync_OnSuccess_PostsMessageWithApiKey()
    {
        var handler = new StubHandler(HttpStatusCode.Created);
        var service = CreateService(handler, senderName: "Alumni Office");

        await service.SendEmailAsync("member@example.com", "Your code", "<p>123456</p>");

        handler.Request!.Method.Should().Be(HttpMethod.Post);
        handler.Request.RequestUri.Should().Be(new Uri(new BrevoSettingsOptions().BaseUrl));
        handler.Request.Headers.GetValues("api-key").Should().ContainSingle().Which.Should().Be(ApiKey);

        using var body = JsonDocument.Parse(handler.Body!);
        var root = body.RootElement;
        root.GetProperty("sender").GetProperty("email").GetString().Should().Be(SenderEmail);
        root.GetProperty("sender").GetProperty("name").GetString().Should().Be("Alumni Office");
        root.GetProperty("to")[0].GetProperty("email").GetString().Should().Be("member@example.com");
        root.GetProperty("subject").GetString().Should().Be("Your code");
        root.GetProperty("htmlContent").GetString().Should().Be("<p>123456</p>");
    }

    [Test]
    public async Task SendEmailAsync_WithoutSenderName_LeavesNameOut()
    {
        var handler = new StubHandler(HttpStatusCode.Created);
        var service = CreateService(handler, senderName: " ");

        await service.SendEmailAsync("member@example.com", "Subject", "<p>Body</p>");

        using var body = JsonDocument.Parse(handler.Body!);
        body.RootElement.GetProperty("sender").TryGetProperty("name", out _).Should().BeFalse();
    }

    [Test]
    public async Task SendEmailAsync_WhenBrevoRefuses_ThrowsWithStatus()
    {
        var handler = new StubHandler(HttpStatusCode.BadRequest, "{\"message\":\"sender not verified\"}");
        var service = CreateService(handler);

        var act = () => service.SendEmailAsync("member@example.com", "Subject", "<p>Body</p>");

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("400").And.Contain("sender not verified");
    }

    [Test]
    public async Task SendEmailAsync_WhenRequestFails_WrapsTheError()
    {
        var handler = new StubHandler(new HttpRequestException("no route to host"));
        var service = CreateService(handler);

        var act = () => service.SendEmailAsync("member@example.com", "Subject", "<p>Body</p>");

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().StartWith("Email service error:").And.Contain("no route to host");
    }

    private BrevoEmailService CreateService(StubHandler handler, string? senderName = null)
    {
        var settings = Options.Create(new BrevoSettingsOptions
        {
            ApiKey = ApiKey,
            SenderEmail = SenderEmail,
            SenderName = senderName
        });
        return new BrevoEmailService(new HttpClient(handler), settings, _mockLogger.Object);
    }

    // Records the last request and answers with a fixed status, or throws to mimic a network failure.
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _responseBody;
        private readonly Exception? _error;

        public HttpRequestMessage? Request { get; private set; }
        public string? Body { get; private set; }

        public StubHandler(HttpStatusCode status, string responseBody = "{}")
        {
            _status = status;
            _responseBody = responseBody;
        }

        public StubHandler(Exception error) : this(HttpStatusCode.OK)
        {
            _error = error;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_error is not null) throw _error;
            Request = request;
            // The service disposes the request after sending, so the body is read now.
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(_status) { Content = new StringContent(_responseBody) };
        }
    }
}
