using FluentAssertions;
using GHCAA.API.Services;
using GHCAA.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace GHCAA.Tests.Services
{
    // The sweep keeps no record of its last run, so each boot has to run a pass straight away.
    [TestFixture]
    [Category("NFR-R5")]
    public class DataRetentionSweepTests
    {
        private static DataRetentionSweep NewSweep(Mock<IDataRetentionService> retention)
        {
            var provider = new ServiceCollection().AddSingleton(retention.Object).BuildServiceProvider();
            return new DataRetentionSweep(provider.GetRequiredService<IServiceScopeFactory>(), NullLogger<DataRetentionSweep>.Instance);
        }

        [Test]
        public async Task Start_RunsTheFirstPassWithoutWaiting()
        {
            var ran = new TaskCompletionSource();
            var retention = new Mock<IDataRetentionService>();
            retention.Setup(r => r.PurgeAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Callback(() => ran.TrySetResult())
                .ReturnsAsync(new Dictionary<string, int> { ["Otps"] = 2 });
            var sweep = NewSweep(retention);

            await sweep.StartAsync(CancellationToken.None);
            var finished = await Task.WhenAny(ran.Task, Task.Delay(TimeSpan.FromSeconds(10)));
            await sweep.StopAsync(CancellationToken.None);

            finished.Should().BeSameAs(ran.Task);
        }

        [Test]
        public async Task RunOnce_AFailedPass_DoesNotThrow()
        {
            var retention = new Mock<IDataRetentionService>();
            retention.Setup(r => r.PurgeAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("database down"));

            var act = () => NewSweep(retention).RunOnceAsync(CancellationToken.None);

            await act.Should().NotThrowAsync();
        }
    }
}
