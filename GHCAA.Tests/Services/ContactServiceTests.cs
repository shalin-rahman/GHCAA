using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Options;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Services;

[TestFixture]
public class ContactServiceTests : TestBase
{
    [Test]
    public async Task SubmitMessageAsync_SavesMessageAndLogsWarning_WhenEmailFails()
    {
        var communication = new Mock<ICommunicationService>();
        communication.Setup(c => c.SendEmailByCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP down"));
        var logger = new Mock<ILogger<ContactService>>();
        var options = Options.Create(new ContactUsSettingsOptions { Recipients = ["office@example.org"] });
        var service = new ContactService(_context, communication.Object, options, logger.Object);

        Func<Task> act = () => service.SubmitMessageAsync(new ContactMessageDto { FullName = "A", Email = "a@example.org", Subject = "S", Message = "M" });

        await act.Should().NotThrowAsync();
        _context.ContactMessages.Should().HaveCount(1);
        logger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<InvalidOperationException>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
