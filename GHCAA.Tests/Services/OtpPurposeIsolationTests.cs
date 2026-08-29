using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services;

// 7.13: OTP codes are scoped by purpose. Without this, a code mailed out for registration
// would satisfy an admin step-up challenge, which would defeat the point of the step-up gate.
[TestFixture]
public class OtpPurposeIsolationTests : TestBase
{
    private OtpService _service = null!;

    [SetUp]
    public void Setup()
    {
        var mockCommunication = new Mock<ICommunicationService>();
        var mockConfig = new Mock<IConfiguration>();
        var mockLogger = new Mock<ILogger<OtpService>>();
        mockConfig.Setup(x => x["OtpSettings:ExpiryMinutes"]).Returns("10");
        mockConfig.Setup(x => x["Jwt:Key"]).Returns("otp-service-test-dummy-hmac-key-please-32chars");
        _service = new OtpService(_context, mockCommunication.Object, mockConfig.Object, mockLogger.Object);
    }

    [Test]
    public async Task GenerateAndSendOtpAsync_ShouldPersistRequestedPurpose()
    {
        var email = "stepup-persist@example.com";
        await _service.GenerateAndSendOtpAsync(email, OtpPurpose.AdminStepUp);

        var otp = await _context.Otps.FirstAsync(o => o.Email == email);
        otp.Purpose.Should().Be(OtpPurpose.AdminStepUp);
    }

    [Test]
    public async Task VerifyOtpAsync_ShouldAcceptCode_ForMatchingPurpose()
    {
        var email = "stepup-match@example.com";
        var code = await _service.GenerateAndSendOtpAsync(email, OtpPurpose.AdminStepUp);

        var result = await _service.VerifyOtpAsync(email, code, OtpPurpose.AdminStepUp);

        result.Should().BeTrue();
    }

    [Test]
    public async Task VerifyOtpAsync_ShouldRejectRegistrationCode_PresentedAsStepUp()
    {
        var email = "stepup-cross@example.com";
        var registrationCode = await _service.GenerateAndSendOtpAsync(email, OtpPurpose.Registration);

        var result = await _service.VerifyOtpAsync(email, registrationCode, OtpPurpose.AdminStepUp);

        result.Should().BeFalse();
    }

    [Test]
    public async Task VerifyOtpAsync_ShouldRejectStepUpCode_PresentedAsRegistration()
    {
        var email = "stepup-reverse@example.com";
        var stepUpCode = await _service.GenerateAndSendOtpAsync(email, OtpPurpose.AdminStepUp);

        var result = await _service.VerifyOtpAsync(email, stepUpCode, OtpPurpose.Registration);

        result.Should().BeFalse();
    }

    [Test]
    public async Task GenerateAndSendOtpAsync_ShouldNotInvalidate_PendingCodeOfAnotherPurpose()
    {
        var email = "stepup-coexist@example.com";
        var registrationCode = await _service.GenerateAndSendOtpAsync(email, OtpPurpose.Registration);

        // Issuing a step-up code must leave the pending registration code usable.
        await _service.GenerateAndSendOtpAsync(email, OtpPurpose.AdminStepUp);

        var result = await _service.VerifyOtpAsync(email, registrationCode, OtpPurpose.Registration);
        result.Should().BeTrue();
    }

    [Test]
    public async Task GenerateAndSendOtpAsync_ShouldInvalidate_EarlierCodeOfSamePurpose()
    {
        var email = "stepup-supersede@example.com";
        var firstCode = await _service.GenerateAndSendOtpAsync(email, OtpPurpose.AdminStepUp);
        await _service.GenerateAndSendOtpAsync(email, OtpPurpose.AdminStepUp);

        var result = await _service.VerifyOtpAsync(email, firstCode, OtpPurpose.AdminStepUp);
        result.Should().BeFalse();
    }
}
