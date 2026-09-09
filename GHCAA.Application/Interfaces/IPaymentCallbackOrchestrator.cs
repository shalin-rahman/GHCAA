namespace GHCAA.Application.Interfaces
{
    /// <summary>
    /// Encapsulates the post-verification business logic that runs after a gateway
    /// confirms a successful payment (idempotency check, amount verification,
    /// event-registration auto-approval, member auto-induction).
    /// Extracted from GatewaysController to satisfy SRP (82.67).
    /// </summary>
    public interface IPaymentCallbackOrchestrator
    {
        /// <param name="transactionId">Internal transaction ID stamped at initiation.</param>
        /// <param name="cancellationToken"/>
        /// <param name="confirmedAmount">
        ///   Amount reported by the gateway. <c>null</c> means the gateway did not echo
        ///   an amount (amount-check is skipped). A value of 0 is treated as "reported and
        ///   wrong" against any positive expected amount (29B.2).
        /// </param>
        /// <param name="gatewayPaymentId">
        ///   Gateway-side payment identifier used for idempotency (24.13).
        /// </param>
        Task HandleSuccessfulPaymentAsync(
            string transactionId,
            CancellationToken cancellationToken,
            decimal? confirmedAmount = null,
            string? gatewayPaymentId = null);
    }
}
