using FluentAssertions;
using GHCAA.API.Middleware;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace GHCAA.Tests.Middleware
{
    [TestFixture]
    public class SecurityStampMiddlewareTests
    {
        private SqliteConnection _connection = null!;
        private MemoryCache _cache = null!;
        private ApplicationDbContext _context = null!;
        private SecurityStampMiddleware _middleware = null!;
        private bool _nextCalled;

        [SetUp]
        public void Setup()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _cache = new MemoryCache(new MemoryCacheOptions());
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .AddInterceptors(new SecurityStampCacheInterceptor(_cache))
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();
            _nextCalled = false;
            _middleware = new SecurityStampMiddleware(_ => { _nextCalled = true; return Task.CompletedTask; }, _cache);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
            _cache.Dispose();
            _connection.Close();
        }

        private async Task<User> AddUserAsync()
        {
            var user = new User { Username = "stamp-user", PasswordHash = "ph" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        private static DefaultHttpContext RequestWith(int userId, string stamp) => new()
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.ToString()), new Claim(Constants.SecurityStamps.ClaimType, stamp)], "test")),
        };

        [Test]
        [Category("NFR-S5")]
        public async Task MatchingStamp_PassesThrough()
        {
            var user = await AddUserAsync();

            await _middleware.InvokeAsync(RequestWith(user.Id, user.SecurityStamp), _context);

            _nextCalled.Should().BeTrue();
        }

        [Test]
        [Category("NFR-S5")]
        public async Task StaleStamp_Returns401()
        {
            var user = await AddUserAsync();
            var http = RequestWith(user.Id, "old-stamp");

            await _middleware.InvokeAsync(http, _context);

            _nextCalled.Should().BeFalse();
            http.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        // NFR-P1. The second request is served from the cache. ExecuteUpdate changes the row behind
        // the cache's back, so a passing second request proves no query ran (98.8).
        [Test]
        [Category("NFR-P1")]
        public async Task SecondRequest_IsServedFromTheCache()
        {
            var user = await AddUserAsync();
            var original = user.SecurityStamp;
            await _middleware.InvokeAsync(RequestWith(user.Id, original), _context);

            await _context.Users.Where(u => u.Id == user.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.SecurityStamp, "changed-behind-the-cache"));
            _nextCalled = false;
            await _middleware.InvokeAsync(RequestWith(user.Id, original), _context);

            _nextCalled.Should().BeTrue();
        }

        // QAS-03. Terminating a member must end a live token on the very next request, cache or not.
        [Test]
        [Category("NFR-S5")]
        public async Task RotationThroughATrackedSave_EndsTheSessionOnTheNextRequest()
        {
            var user = await AddUserAsync();
            var original = user.SecurityStamp;
            await _middleware.InvokeAsync(RequestWith(user.Id, original), _context);

            user.RotateSecurityStamp();
            await _context.SaveChangesAsync();
            _nextCalled = false;
            var http = RequestWith(user.Id, original);
            await _middleware.InvokeAsync(http, _context);

            _nextCalled.Should().BeFalse();
            http.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Test]
        public async Task AnonymousRequest_IsNotChecked()
        {
            await _middleware.InvokeAsync(new DefaultHttpContext(), _context);

            _nextCalled.Should().BeTrue();
        }
    }
}
