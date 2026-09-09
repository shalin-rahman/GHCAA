using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    /// <summary>
    /// Implements the post-gateway-verification business flow:
    /// idempotency guard → amount check → event-registration auto-approval
    /// → membership auto-induction. Extracted from GatewaysController (82.67 / SRP).
    /// </summary>
    public class PaymentCallbackOrchestrator : IPaymentCallbackOrchestrator
    {
        private readonly IFinancialService _financialService;
        private readonly IEventService _eventService;
        private readonly IMemberService _memberService;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentCallbackOrchestrator> _logger;

        public PaymentCallbackOrchestrator(
            IFinancialService financialService,
            IEventService eventService,
            IMemberService memberService,
            IConfiguration config,
            ILogger<PaymentCallbackOrchestrator> logger)
        {
            _financialService = financialService;
            _eventService = eventService;
            _memberService = memberService;
            _config = config;
            _logger = logger;
        }

        public async Task HandleSuccessfulPaymentAsync(
            string transactionId,
            CancellationToken cancellationToken,
            decimal? confirmedAmount = null,
            string? gatewayPaymentId = null)
        {
            // 24.13: Idempotency check — short-circuit if already processed.
            if (!string.IsNullOrEmpty(gatewayPaymentId))
            {
                var alreadyProcessed = await _financialService.IsGatewayPaymentAlreadyProcessedAsync(gatewayPaymentId, cancellationToken);
                if (alreadyProcessed)
                {
                    _logger.LogInformation("Duplicate callback ignored for GatewayPaymentId {GwId}", gatewayPaymentId);
                    return;
                }
            }

            var payment = await _financialService.GetPaymentSnapshotByTransactionIdAsync(transactionId, cancellationToken);
            if (payment == null || payment.Status == Enums.PaymentStatus.Completed) return;

            // 24.13: Persist the gateway payment ID for future idempotency checks.
            if (!string.IsNullOrEmpty(gatewayPaymentId))
                await _financialService.StampGatewayPaymentIdAsync(payment.Id, gatewayPaymentId, cancellationToken);

            // 29B.2: A null confirmedAmount means the gateway did not echo an amount (skip check).
            // A value of 0 is treated as "reported and wrong" against any positive expected amount.
            if (confirmedAmount.HasValue && Math.Abs(payment.Amount - confirmedAmount.Value) > 0.01m)
            {
                _logger.LogWarning(
                    "Payment amount mismatch for {TrxID}. Expected {E}, Received {R}. Marking as discrepancy.",
                    transactionId, payment.Amount, confirmedAmount.Value);
                await _financialService.UpdatePaymentStatusAsync(
                    payment.Id,
                    Enums.PaymentStatus.Failed,
                    $"Amount mismatch detected. Paid: {confirmedAmount.Value}, Expected: {payment.Amount}",
                    cancellationToken);
                return;
            }

            // 1. Mark as Completed.
            await _financialService.UpdatePaymentStatusAsync(
                payment.Id, Enums.PaymentStatus.Completed, "Verified via Gateway Automatic Protocol", cancellationToken);

            // 2. Event-registration auto-approval.
            // 82.32: FinancialCategory is the authoritative discriminator (not a Notes string search).
            if (payment.FinancialCategory == Enums.FinancialCategory.RegistrationFee)
            {
                var refPrefixIndex = payment.Notes?.IndexOf("Ref: ", StringComparison.OrdinalIgnoreCase) ?? -1;
                var fullRef = refPrefixIndex >= 0
                    ? payment.Notes!.Substring(refPrefixIndex + "Ref: ".Length).Split(' ')[0].Trim()
                    : null;

                var registration = string.IsNullOrEmpty(fullRef)
                    ? null
                    : await _eventService.GetRegistrationByPaymentReferenceAsync(fullRef, cancellationToken);

                if (registration != null
                    && registration.Status == Enums.EventRegistrationStatus.Pending
                    && registration.Event != null)
                {
                    var ev = registration.Event;
                    var expectedAmount = (ev.RegistrationFee ?? 0) > 0
                        ? (ev.RegistrationFee ?? 0)
                        : (registration.ContributionAmount ?? 0);

                    if (payment.Amount >= expectedAmount)
                    {
                        var adminIdStr = _config[Constants.ConfigKeys.SystemAdminId] ?? "1";
                        int.TryParse(adminIdStr, out var adminId);
                        _logger.LogInformation("Auto-Approving Event Registration {Id} for reference {Ref}", registration.Id, fullRef);
                        await _eventService.AutoApproveRegistrationAfterPaymentAsync(registration.Id, adminId, cancellationToken);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Event registration {Id} under-paid: Expected {E}, Paid {P}",
                            registration.Id, expectedAmount, payment.Amount);
                    }
                }
            }

            // 3. Membership auto-induction.
            // 29B.3: Scoped strictly to MembershipFee — prevents event-fee payers from being inducted.
            if (payment.MemberId is int membershipPayerId && membershipPayerId > 0
                && payment.FinancialCategory == Enums.FinancialCategory.MembershipFee)
            {
                var snapshot = await _memberService.GetMembershipSnapshotAsync(membershipPayerId, cancellationToken);
                if (snapshot != null && snapshot.Value.Status == Enums.MembershipStatus.Applied)
                {
                    var membershipType = snapshot.Value.MembershipType;

                    // 82.32: Use the canonical GetApplicableFeeAsync query.
                    var required = await _financialService.GetApplicableFeeAsync(
                        Enums.FinancialCategory.MembershipFee, membershipType, DateTime.UtcNow, cancellationToken);

                    if (required <= 0)
                    {
                        _logger.LogError(
                            "Auto-approval skipped for Member {Id}: no fee config found for type {Type}",
                            membershipPayerId, membershipType);
                        return;
                    }

                    if (payment.Amount >= required)
                    {
                        // S4.3: Fail loudly when SystemAdminId is not configured.
                        var adminIdStr = _config[Constants.ConfigKeys.SystemAdminId]
                            ?? throw new InvalidOperationException($"{Constants.ConfigKeys.SystemAdminId} is not configured.");
                        int.TryParse(adminIdStr, out var adminId);

                        _logger.LogInformation("Auto-Approving Member {MemberId} after successful gateway payment.", membershipPayerId);
                        await _memberService.ApproveMemberAsync(membershipPayerId, adminId, cancellationToken);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Member {Id} under-paid subscription: Required {R}, Paid {P}",
                            membershipPayerId, required, payment.Amount);
                    }
                }
            }
        }
    }
}
