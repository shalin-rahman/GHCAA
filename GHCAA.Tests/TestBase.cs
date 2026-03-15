using GHCAA.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GHCAA.Tests
{
    public abstract class TestBase
    {
        protected ApplicationDbContext _context = null!;
        private SqliteConnection _connection = null!;

        [SetUp]
        public void BaseSetup()
        {
            ApplicationDbContext.IsSeedDisabled = true;
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();
        }

        [TearDown]
        public void BaseTearDown()
        {
            _context.Dispose();
            _connection.Close();
        }

        protected DbContextOptions<ApplicationDbContext> GetOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;
        }
    }
}
