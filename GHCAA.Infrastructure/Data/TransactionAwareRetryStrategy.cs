using GHCAA.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.Data.Common;

namespace GHCAA.Infrastructure.Data
{
    // 98.10. Npgsql retry on failure, except while the code holds its own transaction.
    // The stock retrying strategy throws on every SaveChanges inside BeginTransaction, and fourteen
    // service paths open one. Retrying a single statement mid-transaction is not safe anyway, so
    // inside a transaction the call runs once and any error goes up as before.
    //
    // A write outside a transaction is not retried either: SaveChanges, ExecuteUpdate or
    // ExecuteDelete. If the connection drops after the commit but before the reply, a retry would
    // write the same rows again. Only the connection open is retried there. Npgsql hands back a
    // pooled socket without checking it, and one that died while Neon slept would fail the write, so
    // a freshly opened connection runs SELECT 1 inside the retried open first. ExecuteSqlRaw never
    // goes through an execution strategy, so it is never retried. Queries keep the full retry.
    public class TransactionAwareRetryStrategy : NpgsqlRetryingExecutionStrategy
    {
        public TransactionAwareRetryStrategy(ExecutionStrategyDependencies dependencies)
            : this(dependencies, Constants.Deploy.DbRetryCount, TimeSpan.FromSeconds(Constants.Deploy.DbRetryMaxDelaySeconds))
        {
        }

        public TransactionAwareRetryStrategy(ExecutionStrategyDependencies dependencies, int maxRetryCount, TimeSpan maxRetryDelay)
            : base(dependencies, maxRetryCount, maxRetryDelay, errorCodesToAdd: null)
        {
        }

        private DbContext Context => Dependencies.CurrentContext.Context;

        // Database.OpenConnection goes back through this strategy and would recurse, so the open
        // uses the connection service directly.
        private IRelationalConnection Connection => Context.GetService<IRelationalConnection>();

        private bool InTransaction => Context.Database.CurrentTransaction != null;

        // SaveChanges hands the strategy its state manager and the accept-all flag. ExecuteUpdate and
        // ExecuteDelete hand it their query context and command. Queries pass other state, so they
        // keep the full retry even while the context holds unsaved changes. Both shapes are EF
        // internal API; SaveOutsideATransaction_IsNotRetried and ExecuteUpdateOutsideATransaction_IsNotRetried
        // fail if either moves.
#pragma warning disable EF1001
        private static bool IsWrite<TState>() =>
            typeof(TState) == typeof(ValueTuple<StateManager, bool>)
            || typeof(TState) == typeof(ValueTuple<RelationalQueryContext, RelationalCommandResolver, CommandSource>);
#pragma warning restore EF1001

        // Runs inside the retried open. Only a connection this call opened is checked, since one the
        // context already held has just been used.
        private void OpenChecked()
        {
            if (!Connection.Open()) return;
            try
            {
                CheckConnection(Connection.DbConnection);
            }
            catch
            {
                Connection.Close();
                throw;
            }
        }

        private async Task OpenCheckedAsync(CancellationToken cancellationToken)
        {
            if (!await Connection.OpenAsync(cancellationToken)) return;
            try
            {
                await CheckConnectionAsync(Connection.DbConnection, cancellationToken);
            }
            catch
            {
                await Connection.CloseAsync();
                throw;
            }
        }

        // Straight on the DbConnection so command interceptors and logging do not see it.
        protected virtual void CheckConnection(DbConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            command.ExecuteScalar();
        }

        protected virtual async Task CheckConnectionAsync(DbConnection connection, CancellationToken cancellationToken)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);
        }

        // A close that throws after a failed write would hide the write's own error. After a write that
        // committed, it would report a failure for rows that are saved, and a caller could write them again.
        private void CloseQuietly()
        {
            try { Connection.Close(); } catch (Exception) { }
        }

        private async Task CloseQuietlyAsync()
        {
            try { await Connection.CloseAsync(); } catch (Exception) { }
        }

        public override TResult Execute<TState, TResult>(
            TState state,
            Func<DbContext, TState, TResult> operation,
            Func<DbContext, TState, ExecutionResult<TResult>>? verifySucceeded)
        {
            if (InTransaction) return operation(Context, state);
            if (!IsWrite<TState>()) return base.Execute(state, operation, verifySucceeded);

            base.Execute(0, (_, _) => { OpenChecked(); return true; }, null);
            TResult result;
            try
            {
                result = operation(Context, state);
            }
            catch
            {
                CloseQuietly();
                throw;
            }
            CloseQuietly();
            return result;
        }

        public override async Task<TResult> ExecuteAsync<TState, TResult>(
            TState state,
            Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
            Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>>? verifySucceeded,
            CancellationToken cancellationToken = default)
        {
            if (InTransaction) return await operation(Context, state, cancellationToken);
            if (!IsWrite<TState>()) return await base.ExecuteAsync(state, operation, verifySucceeded, cancellationToken);

            await base.ExecuteAsync(0, async (_, _, ct) => { await OpenCheckedAsync(ct); return true; }, null, cancellationToken);
            TResult result;
            try
            {
                result = await operation(Context, state, cancellationToken);
            }
            catch
            {
                await CloseQuietlyAsync();
                throw;
            }
            await CloseQuietlyAsync();
            return result;
        }
    }
}
