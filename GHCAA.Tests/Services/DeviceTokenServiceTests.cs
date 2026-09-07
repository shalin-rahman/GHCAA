using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Infrastructure.Services;
using NUnit.Framework;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class DeviceTokenServiceTests : TestBase
    {
        private DeviceTokenService _service = null!;

        [SetUp]
        public void Setup()
        {
            _service = new DeviceTokenService(_context);
        }

        [Test]
        public async Task RegisterTokenAsync_ForExistingMember_SavesTokenAndReturnsTrue()
        {
            var member = await CreateAndSaveTestMemberAsync();

            var result = await _service.RegisterTokenAsync(member.Id, new DeviceTokenDto { Token = "abc123", Platform = "android" });

            result.Should().BeTrue();
            var stored = await _service.GetTokenAsync(member.Id);
            stored.Should().Be("abc123");
        }

        [Test]
        public async Task RegisterTokenAsync_ForUnknownMember_ReturnsFalse()
        {
            var result = await _service.RegisterTokenAsync(99999, new DeviceTokenDto { Token = "abc123", Platform = "ios" });

            result.Should().BeFalse();
        }

        [Test]
        public async Task RegisterTokenAsync_CalledTwice_OverwritesThePreviousToken()
        {
            var member = await CreateAndSaveTestMemberAsync();

            await _service.RegisterTokenAsync(member.Id, new DeviceTokenDto { Token = "old-token", Platform = "android" });
            await _service.RegisterTokenAsync(member.Id, new DeviceTokenDto { Token = "new-token", Platform = "android" });

            var stored = await _service.GetTokenAsync(member.Id);
            stored.Should().Be("new-token");
        }

        [Test]
        public async Task GetTokenAsync_ForMemberWithNoToken_ReturnsNull()
        {
            var member = await CreateAndSaveTestMemberAsync();

            var stored = await _service.GetTokenAsync(member.Id);

            stored.Should().BeNull();
        }

        [Test]
        public async Task GetTokenAsync_ForUnknownMember_ReturnsNull()
        {
            var stored = await _service.GetTokenAsync(99999);

            stored.Should().BeNull();
        }
    }
}
