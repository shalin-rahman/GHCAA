using System.Net;
using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Options;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace GHCAA.Tests.Services;

[TestFixture]
public class EmailServiceRouterTests
{
    private CountingHandler _brevoHandler = null!;
    private Mock<IOrgConfigService> _orgConfig = null!;
    private ServiceProvider _provider = null!;

    // Gmail settings are left empty, so building GmailEmailService throws. That makes a Gmail pick
    // visible without opening an SMTP connection; a Brevo pick shows up as a request on the stub.
    [SetUp]
    public void Setup()
    {
        _brevoHandler = new CountingHandler();
        _orgConfig = new Mock<IOrgConfigService>();
        var services = new ServiceCollection();
        services.AddLogging();
        services.Configure<BrevoSettingsOptions>(o => { o.ApiKey = "key"; o.SenderEmail = "sender@example.com"; });
        services.Configure<GmailSettingsOptions>(_ => { });
        services.AddScoped(sp => new BrevoEmailService(
            new HttpClient(_brevoHandler),
            sp.GetRequiredService<IOptions<BrevoSettingsOptions>>(),
            sp.GetRequiredService<ILogger<BrevoEmailService>>()));
        services.AddScoped<GmailEmailService>();
        _provider = services.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown()
    {
        _provider.Dispose();
        _brevoHandler.Dispose();
    }

    [TestCase("Brevo", EmailSettingsOptions.Gmail)]
    [TestCase(null, EmailSettingsOptions.Brevo)]
    [TestCase(null, "brevo")]
    public async Task SendEmailAsync_UsesBrevo_WhenAdminOrServerPicksIt(string? adminChoice, string serverDefault)
    {
        var router = CreateRouter(adminChoice, serverDefault);

        await router.SendEmailAsync("member@example.com", "Your code", "<p>1</p>");

        _brevoHandler.Requests.Should().Be(1);
    }

    [TestCase("Gmail", EmailSettingsOptions.Brevo)]
    [TestCase(null, EmailSettingsOptions.Gmail)]
    public async Task SendEmailAsync_UsesGmail_WhenAdminOrServerPicksIt(string? adminChoice, string serverDefault)
    {
        var router = CreateRouter(adminChoice, serverDefault);

        var act = () => router.SendEmailAsync("member@example.com", "Your code", "<p>1</p>");

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("GmailSettings:Email");
        _brevoHandler.Requests.Should().Be(0);
    }

    [Test]
    public async Task GetActiveProviderAsync_FallsBackToServerDefault_WhenConfigReadFails()
    {
        _orgConfig.Setup(x => x.GetConfigAsync()).ThrowsAsync(new InvalidOperationException("db down"));
        var router = new EmailServiceRouter(_orgConfig.Object, _provider,
            Options.Create(new EmailSettingsOptions { Provider = EmailSettingsOptions.Brevo }),
            NullLogger<EmailServiceRouter>.Instance);

        (await router.GetActiveProviderAsync()).Should().Be(EmailSettingsOptions.Brevo);
    }

    [TestCase("Brevo", "Gmail", "Brevo")]
    [TestCase("gmail", "Brevo", "Gmail")]
    [TestCase(null, "Brevo", "Brevo")]
    [TestCase("SendGrid", "Brevo", "Brevo")]
    [TestCase(null, null, "Gmail")]
    [TestCase(" ", "", "Gmail")]
    public void Resolve_PrefersAdminChoice_ThenServerDefault_ThenGmail(string? admin, string? server, string expected)
    {
        EmailSettingsOptions.Resolve(admin, server).Should().Be(expected);
    }

    private EmailServiceRouter CreateRouter(string? adminChoice, string serverDefault)
    {
        _orgConfig.Setup(x => x.GetConfigAsync())
            .ReturnsAsync(new OrgConfigDto { Workflow = new WorkflowDto { EmailProvider = adminChoice } });
        return new EmailServiceRouter(_orgConfig.Object, _provider,
            Options.Create(new EmailSettingsOptions { Provider = serverDefault }),
            NullLogger<EmailServiceRouter>.Instance);
    }

    private sealed class CountingHandler : HttpMessageHandler
    {
        public int Requests { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created) { Content = new StringContent("{}") });
        }
    }
}
