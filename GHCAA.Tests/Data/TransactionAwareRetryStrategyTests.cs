using FluentAssertions;
using GHCAA.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace GHCAA.Tests.Data
{
    // 98.10. The strategy only decides by whether a transaction is open, so a SQLite context is
    // enough to drive it. A timeout wrapped in NpgsqlException is what Npgsql calls transient.
    [TestFixture]
    [Category("NFR-R5")]
    public class TransactionAwareRetryStrategyTests
    {
        private SqliteConnection _connection = null!;

        private sealed class Row
        {
            public int Id { get; set; }
        }

        private sealed class TinyContext(DbContextOptions<TinyContext> options) : DbContext(options)
        {
            public DbSet<Row> Rows => Set<Row>();
        }

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
        }

        [TearDown]
        public void TearDown() => _connection.Dispose();

        private TinyContext NewContext(Func<ExecutionStrategyDependencies, IExecutionStrategy> strategy)
        {
            var options = new DbContextOptionsBuilder<TinyContext>()
                .UseSqlite(_connection, o => o.ExecutionStrategy(strategy))
                .Options;
            var context = new TinyContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private TinyContext NewContext() =>
            NewContext(d => new TransactionAwareRetryStrategy(d, maxRetryCount: 2, maxRetryDelay: TimeSpan.FromMilliseconds(1)));

        private static NpgsqlException Transient() => new("connection timed out", new TimeoutException());

        [Test]
        public async Task OutsideATransaction_TransientErrorIsRetried()
        {
            using var context = NewContext();
            var calls = 0;

            var result = await context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await Task.Yield();
                if (++calls == 1) throw Transient();
                return 42;
            });

            result.Should().Be(42);
            calls.Should().Be(2);
        }

        [Test]
        public async Task InsideATransaction_SaveChangesWorksAndIsNotRetried()
        {
            using var context = NewContext();
            await using var tx = await context.Database.BeginTransactionAsync();

            context.Rows.Add(new Row());
            await context.SaveChangesAsync();

            var calls = 0;
            var act = () => context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await Task.Yield();
                calls++;
                throw Transient();
            });

            await act.Should().ThrowAsync<NpgsqlException>();
            calls.Should().Be(1);
            await tx.CommitAsync();
            (await context.Rows.CountAsync()).Should().Be(1);
        }

        // Why the stock strategy was not used: it refuses SaveChanges inside BeginTransaction.
        [Test]
        public async Task StockRetryStrategy_RejectsSaveInsideATransaction()
        {
            using var context = NewContext(d => new NpgsqlRetryingExecutionStrategy(d, 2));
            await using var tx = await context.Database.BeginTransactionAsync();

            context.Rows.Add(new Row());
            var act = () => context.SaveChangesAsync();

            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
