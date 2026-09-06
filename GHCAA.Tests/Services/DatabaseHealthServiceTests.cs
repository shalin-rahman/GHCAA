using System.Threading.Tasks;
using GHCAA.Infrastructure.Services;
using NUnit.Framework;

namespace GHCAA.Tests.Services
{
    // 80.13: extracted from HealthController so it doesn't touch ApplicationDbContext directly.
    [TestFixture]
    public class DatabaseHealthServiceTests : TestBase
    {
        [Test]
        public async Task CanConnectAsync_ReturnsTrue_ForALiveConnection()
        {
            var service = new DatabaseHealthService(_context);

            var result = await service.CanConnectAsync();

            Assert.That(result, Is.True);
        }
    }
}
