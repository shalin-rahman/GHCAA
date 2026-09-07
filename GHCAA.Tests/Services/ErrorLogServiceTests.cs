using System;
using System.Linq;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Domain;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class ErrorLogServiceTests : TestBase
    {
        private ErrorLogService CreateService() => new(_context, NullLogger<ErrorLogService>.Instance);

        [Test]
        public async Task LogAsync_PersistsRow_WithGivenFields()
        {
            var service = CreateService();

            await service.LogAsync(
                Constants.ErrorLogs.LevelError,
                "Boom",
                "System.InvalidOperationException",
                "at Foo.Bar()",
                "ExceptionMiddleware",
                "/api/members",
                "GET",
                userId: 7,
                username: "shalin");

            var row = _context.ErrorLogs.Single();
            Assert.That(row.Level, Is.EqualTo(Constants.ErrorLogs.LevelError));
            Assert.That(row.Message, Is.EqualTo("Boom"));
            Assert.That(row.ExceptionType, Is.EqualTo("System.InvalidOperationException"));
            Assert.That(row.Source, Is.EqualTo("ExceptionMiddleware"));
            Assert.That(row.RequestPath, Is.EqualTo("/api/members"));
            Assert.That(row.RequestMethod, Is.EqualTo("GET"));
            Assert.That(row.UserId, Is.EqualTo(7));
            Assert.That(row.Username, Is.EqualTo("shalin"));
        }

        [Test]
        public async Task LogAsync_AcceptsNullOptionalFields_ForPreAuthOrBackgroundErrors()
        {
            var service = CreateService();

            await service.LogAsync(
                Constants.ErrorLogs.LevelWarning,
                "Background job failed",
                exceptionType: null,
                stackTrace: null,
                source: "BackgroundWorker",
                requestPath: null,
                requestMethod: null,
                userId: null,
                username: null);

            var row = _context.ErrorLogs.Single();
            Assert.That(row.UserId, Is.Null);
            Assert.That(row.Username, Is.Null);
            Assert.That(row.RequestPath, Is.Null);
        }

        [Test]
        public async Task GetPagedAsync_FiltersByLevel()
        {
            var service = CreateService();
            await service.LogAsync(Constants.ErrorLogs.LevelError, "err one", null, null, null, null, null, null, null);
            await service.LogAsync(Constants.ErrorLogs.LevelWarning, "warn one", null, null, null, null, null, null, null);

            var result = await service.GetPagedAsync(new ErrorLogFilterDto { Level = Constants.ErrorLogs.LevelWarning });

            Assert.That(result.Items.Count(), Is.EqualTo(1));
            Assert.That(result.Items.Single().Message, Is.EqualTo("warn one"));
        }

        [Test]
        public async Task GetPagedAsync_FiltersByFreeTextAcrossMessageExceptionTypeAndStackTrace()
        {
            var service = CreateService();
            await service.LogAsync(Constants.ErrorLogs.LevelError, "Null reference in profile save", "System.NullReferenceException", null, null, null, null, null, null);
            await service.LogAsync(Constants.ErrorLogs.LevelError, "Unrelated failure", "System.Exception", "at SomewhereElse()", null, null, null, null, null);

            var result = await service.GetPagedAsync(new ErrorLogFilterDto { Query = "nullreference" });

            Assert.That(result.Items.Count(), Is.EqualTo(1));
            Assert.That(result.Items.Single().Message, Does.Contain("Null reference"));
        }

        [Test]
        public async Task GetPagedAsync_FiltersByDateRange()
        {
            var service = CreateService();
            var old = new GHCAA.Domain.Models.ErrorLog
            {
                OccurredAt = DateTime.UtcNow.AddDays(-30),
                Level = Constants.ErrorLogs.LevelError,
                Message = "old error"
            };
            await _context.ErrorLogs.AddAsync(old);
            await _context.SaveChangesAsync();

            await service.LogAsync(Constants.ErrorLogs.LevelError, "recent error", null, null, null, null, null, null, null);

            var result = await service.GetPagedAsync(new ErrorLogFilterDto { FromDate = DateTime.UtcNow.AddDays(-1) });

            Assert.That(result.Items.Count(), Is.EqualTo(1));
            Assert.That(result.Items.Single().Message, Is.EqualTo("recent error"));
        }

        [Test]
        public async Task GetPagedAsync_PaginatesAndOrdersNewestFirst()
        {
            var service = CreateService();
            for (var i = 0; i < 5; i++)
            {
                await service.LogAsync(Constants.ErrorLogs.LevelError, $"error {i}", null, null, null, null, null, null, null);
            }

            var page1 = await service.GetPagedAsync(new ErrorLogFilterDto { Page = 1, PageSize = 2 });

            Assert.That(page1.Items.Count(), Is.EqualTo(2));
            Assert.That(page1.TotalItems, Is.EqualTo(5));
            Assert.That(page1.TotalPages, Is.EqualTo(3));
            Assert.That(page1.Items.First().Message, Is.EqualTo("error 4")); // newest first
        }

        [Test]
        public async Task GetPagedAsync_ClampsPageSizeToMax()
        {
            var service = CreateService();
            await service.LogAsync(Constants.ErrorLogs.LevelError, "one error", null, null, null, null, null, null, null);

            var result = await service.GetPagedAsync(new ErrorLogFilterDto { PageSize = 5000 });

            Assert.That(result.PageSize, Is.EqualTo(Constants.ErrorLogs.MaxPageSize));
        }
    }
}
