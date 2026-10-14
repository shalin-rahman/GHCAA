using FluentAssertions;
using GHCAA.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

        // Throws a transient error from the first few commands, then lets them through.
        private sealed class FailingCommands(int failures) : DbCommandInterceptor
        {
            public int Attempts { get; private set; }

            public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
                DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
            {
                if (++Attempts <= failures) throw Transient();
                return ValueTask.FromResult(result);
            }

            public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
                DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
            {
                if (++Attempts <= failures) throw Transient();
                return ValueTask.FromResult(result);
            }
        }

        private TinyContext NewContext(FailingCommands commands)
        {
            var context = NewContext();
            var options = new DbContextOptionsBuilder<TinyContext>()
                .UseSqlite(_connection, o => o.ExecutionStrategy(d => new TransactionAwareRetryStrategy(d, maxRetryCount: 2, maxRetryDelay: TimeSpan.FromMilliseconds(1))))
                .AddInterceptors(commands)
                .Options;
            context.Dispose();
            return new TinyContext(options);
        }

        // A save that failed after the server took it may have written its rows. Running it again
        // would write them twice, so the strategy gives up instead.
        [Test]
        public async Task SaveOutsideATransaction_IsNotRetried()
        {
            var commands = new FailingCommands(failures: 1);
            using var context = NewContext(commands);
            context.Rows.Add(new Row());

            var act = () => context.SaveChangesAsync();

            await act.Should().ThrowAsync<DbUpdateException>();
            commands.Attempts.Should().Be(1);
        }

        // ExecuteUpdate runs once on the server like a save, so it gets the same rule.
        [Test]
        public async Task ExecuteUpdateOutsideATransaction_IsNotRetried()
        {
            var commands = new FailingCommands(failures: 1);
            using var context = NewContext(commands);

            var act = () => context.Rows.ExecuteUpdateAsync(s => s.SetProperty(r => r.Id, r => r.Id));

            await act.Should().ThrowAsync<NpgsqlException>();
            commands.Attempts.Should().Be(1);
        }

        // Stands in for a pooled socket that died while the database slept: the first check fails.
        private sealed class DeadFirstConnection(ExecutionStrategyDependencies dependencies)
            : TransactionAwareRetryStrategy(dependencies, maxRetryCount: 2, maxRetryDelay: TimeSpan.FromMilliseconds(1))
        {
            public int Checks { get; private set; }

            protected override Task CheckConnectionAsync(DbConnection connection, CancellationToken cancellationToken)
            {
                if (++Checks == 1) throw Transient();
                return base.CheckConnectionAsync(connection, cancellationToken);
            }
        }

        // The context opens its own connection here, as it does against Postgres, so the check runs.
        [Test]
        public async Task SaveOutsideATransaction_DeadConnection_IsReplacedBeforeTheSave()
        {
            var shared = $"DataSource=file:retry{Guid.NewGuid():N}?mode=memory&cache=shared";
            using var keepAlive = new SqliteConnection(shared);
            keepAlive.Open();
            DeadFirstConnection? strategy = null;
            var options = new DbContextOptionsBuilder<TinyContext>()
                .UseSqlite(shared, o => o.ExecutionStrategy(d => strategy ??= new DeadFirstConnection(d)))
                .Options;
            using var context = new TinyContext(options);
            context.Database.EnsureCreated();

            context.Rows.Add(new Row());
            await context.SaveChangesAsync();

            strategy!.Checks.Should().Be(2);
            (await context.Rows.CountAsync()).Should().Be(1);
        }

        // Unsaved changes in the context must not cost a query its retry.
        [Test]
        public async Task QueryWithUnsavedChangesTracked_IsStillRetried()
        {
            var commands = new FailingCommands(failures: 1);
            using var context = NewContext(commands);
            context.Rows.Add(new Row());

            var count = await context.Rows.CountAsync();

            count.Should().Be(0);
            commands.Attempts.Should().Be(2);
        }

        [Test]
        public void OutsideATransaction_SyncSaveStillWorks()
        {
            using var context = NewContext();
            context.Rows.Add(new Row());

            context.SaveChanges();

            context.Rows.Count().Should().Be(1);
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
