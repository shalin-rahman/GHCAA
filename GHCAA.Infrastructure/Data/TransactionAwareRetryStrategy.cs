using GHCAA.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace GHCAA.Infrastructure.Data
{
    // 98.10. Npgsql retry on failure, except while the code holds its own transaction.
    // The stock retrying strategy throws on every SaveChanges inside BeginTransaction, and fourteen
    // service paths open one. Retrying a single statement mid-transaction is not safe anyway, so
    // inside a transaction the call runs once and any error goes up as before.
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

        private bool InTransaction => Dependencies.CurrentContext.Context.Database.CurrentTransaction != null;

        public override TResult Execute<TState, TResult>(
            TState state,
            Func<DbContext, TState, TResult> operation,
            Func<DbContext, TState, ExecutionResult<TResult>>? verifySucceeded)
            => InTransaction
                ? operation(Dependencies.CurrentContext.Context, state)
                : base.Execute(state, operation, verifySucceeded);

        public override Task<TResult> ExecuteAsync<TState, TResult>(
            TState state,
            Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
            Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>>? verifySucceeded,
            CancellationToken cancellationToken = default)
            => InTransaction
                ? operation(Dependencies.CurrentContext.Context, state, cancellationToken)
                : base.ExecuteAsync(state, operation, verifySucceeded, cancellationToken);
    }
}
