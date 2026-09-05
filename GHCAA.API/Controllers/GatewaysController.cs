using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using System.Linq;
using GHCAA.Domain;
using GHCAA.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GHCAA.Application.Security;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/gateways")]
    public class GatewaysController : ControllerBase
    {
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly IFinancialService _financialService;
        private readonly IMemberService _memberService;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<GatewaysController> _logger;
        private readonly IConfiguration _config;
        private readonly IOrgConfigService _orgConfig;

        public GatewaysController(
            IPaymentGatewayFactory gatewayFactory,
            IFinancialService financialService,
            IMemberService memberService,
            ApplicationDbContext db,
            ILogger<GatewaysController> logger,
            IConfiguration config,
            IOrgConfigService orgConfig)
        {
            _gatewayFactory = gatewayFactory;
            _financialService = financialService;
            _memberService = memberService;
            _db = db;
            _logger = logger;
            _config = config;
            _orgConfig = orgConfig;
        }

        [HttpPost("initiate")]
        [AllowAnonymous]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentRequest request, CancellationToken cancellationToken)
        {
            if (request.Amount <= 0 || request.Amount > 10_000_000m)
                return BadRequest(new { message = "Payment amount is out of the allowed range." });

            if (string.IsNullOrWhiteSpace(request.Reference))
                return BadRequest(new { message = "Reference is required." });

            int? memberId = null;
            var memberIdClaim = User.FindFirst(AppClaimTypes.MemberId)?.Value;
            if (!string.IsNullOrEmpty(memberIdClaim) && int.TryParse(memberIdClaim, out var midClaim))
            {
                memberId = midClaim;
            }

            var isEventOnlinePayment = request.Reference.StartsWith("EVT-REG", StringComparison.OrdinalIgnoreCase);

            if (!isEventOnlinePayment)
            {
                // Require a linked member (MemberId claim from validated JWT). Do not rely on IsAuthenticated here:
                // unit tests and some host integrations build ClaimsPrincipal without the auth middleware pipeline.
                if (!memberId.HasValue)
                {
                    _logger.LogWarning("Blocked payment initiation without MemberId claim for reference {Ref}", request.Reference);
                    return Unauthorized(new { message = "Sign in is required to start this payment." });
                }
            }
            else
            {
                var registration = await _db.EventRegistrations
                    .Include(r => r.Event)
                    .FirstOrDefaultAsync(
                        r => r.PaymentReference == request.Reference && r.Status == Enums.EventRegistrationStatus.Pending,
                        cancellationToken);

                if (registration?.Event == null)
                {
                    _logger.LogWarning("Event payment initiation failed: unknown or non-pending registration {Ref}", request.Reference);
                    return BadRequest(new { message = "Unknown or inactive event registration reference." });
                }

                var regFee = registration.Event.RegistrationFee ?? 0;
                var expected = regFee > 0 ? regFee : (registration.ContributionAmount ?? 0);

                if (expected <= 0 && request.Amount > 0.01m)
                    return BadRequest(new { message = "This registration does not require an online payment." });

                if (expected > 0 && Math.Abs(request.Amount - expected) > 0.01m)
                {
                    _logger.LogWarning(
                        "Event payment amount mismatch for {Ref}. Expected {Expected}, got {Actual}",
                        request.Reference,
                        expected,
                        request.Amount);
                    return BadRequest(new { message = $"Amount must match the event fee ({expected})." });
                }

                if (registration.MemberId.HasValue)
                {
                    if (!memberId.HasValue || memberId.Value != registration.MemberId.Value)
                    {
                        _logger.LogWarning("Event payment member mismatch for {Ref}", request.Reference);
                        return Unauthorized(new { message = "Sign in as the member who registered to complete payment." });
                    }
                }
                else
                {
                    memberId = null;
                }
            }

            var org = await _orgConfig.GetConfigAsync();
            if (!org.EnabledGatewayMethods.Contains(request.Gateway.ToString()))
            {
                _logger.LogWarning("Blocked initiation of disabled gateway: {Gateway}", request.Gateway);
                return BadRequest("This payment method is temporarily unavailable via system configuration.");
            }

            var dbConfig = await _db.PaymentConfigurations.FirstOrDefaultAsync(p => p.Gateway == request.Gateway && p.IsEnabled, cancellationToken);
            if (dbConfig == null)
            {
                _logger.LogWarning("Blocked initiation of disabled gateway (DB): {Gateway}", request.Gateway);
                return BadRequest("This payment method is not active in the registry.");
            }

            var gatewayService = _gatewayFactory.GetGateway(request.Gateway);

            // Create a pending payment history record first
            var prefix = org.Branding.TransactionPrefix;
            var trxId = prefix + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();

            // S4.3: Derive CallbackUrl from server-side config, never from client-supplied BaseUrl.
            var publicApiBase = _config["AppSettings:PublicApiBaseUrl"]
                ?? throw new InvalidOperationException("AppSettings:PublicApiBaseUrl is not configured.");

            var initiateDto = new PaymentGatewayInitiationDto
            {
                MemberId = memberId,
                Amount = request.Amount,
                Currency = org.Currency.Code,
                Reference = request.Reference,
                TransactionId = trxId,
                CallbackUrl = $"{publicApiBase.TrimEnd('/')}/api/gateways/callback/{request.Gateway.ToString().ToLower()}",
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone
            };

            var response = await gatewayService.InitiatePaymentAsync(initiateDto, cancellationToken);

            if (response.Success)
            {
                // Record the intent in history
                await _financialService.RecordPaymentAsync(new CreatePaymentHistoryDto
                {
                    // 82.32: was `memberId ?? 0`, which attributed every guest event payment to a
                    // fabricated Member with Id 0 and crashed on the foreign key (PaymentHistory.MemberId
                    // is nullable now for exactly this case).
                    MemberId = memberId,
                    Amount = request.Amount,
                    TransactionId = trxId,
                    PaidAt = DateTime.UtcNow,
                    // 82.32: was `.Contains("EVT-REG")`, case-sensitive, while `isEventOnlinePayment`
                    // above and the callback's own classification (HandleSuccessfulPayment) both match
                    // case-insensitively. A lowercase reference (client-supplied) fell through to
                    // MembershipFee here while being treated as an event payment everywhere else, so a
                    // member paying an event fee could end up auto-inducted as a full member on
                    // callback — the exact failure 29B.3's category guard exists to block.
                    FinancialCategory = isEventOnlinePayment ? Enums.FinancialCategory.RegistrationFee : Enums.FinancialCategory.MembershipFee,
                    Notes = $"Initiated via {request.Gateway}. Ref: {request.Reference}" + (memberId == null ? " (Guest)" : "")
                }, cancellationToken);

                return Ok(response);
            }

            return BadRequest(response.Message);
        }

        [HttpPost("callback/sslcommerz")]
        [AllowAnonymous]
        public async Task<IActionResult> SSLCommerzCallback([FromForm] IDictionary<string, string> data, CancellationToken cancellationToken)
        {
            var trunkTrxId = data.TryGetValue("tran_id", out var tid) ? tid : "N/A";
            _logger.LogInformation("SSLCommerz Callback Received: {TrxID}", trunkTrxId);

            var gateway = _gatewayFactory.GetGateway(Enums.PaymentGateway.SSLCommerz);
            var isValid = await gateway.VerifyCallbackAsync(data, cancellationToken);

            if (isValid)
            {
                // 29B.2: pass null when the gateway did not include an amount at all (verification is skipped);
                // pass the parsed value — including 0 for a present-but-unparseable amount — when it did (verify).
                decimal? amount = data.TryGetValue("amount", out var a)
                    ? (decimal.TryParse(a, out var amt) ? amt : 0m)
                    : (decimal?)null;
                // 24.13: Pass SSLCommerz's val_id as the idempotency key.
                var valId = data.TryGetValue("val_id", out var vi) ? vi : null;
                await HandleSuccessfulPayment(trunkTrxId, cancellationToken, amount, valId);
                return Redirect($"{GetClientUrl()}/payment/success?trxId={trunkTrxId}");
            }

            _logger.LogWarning("SSLCommerz Callback Verification FAILED for {TrxID}", trunkTrxId);
            return Redirect($"{GetClientUrl()}/payment/failed?trxId={trunkTrxId}");
        }

        [HttpGet("callback/bkashgateway")]
        [AllowAnonymous]
        public async Task<IActionResult> BkashCallbackGet([FromQuery] string paymentID, [FromQuery] string status, CancellationToken cancellationToken)
        {
            var trunkTrxId = paymentID;
            _logger.LogInformation("Bkash Callback Received: {PaymentID}, Status: {Status}", paymentID, status);

            if (string.IsNullOrEmpty(status) || status.ToLower() != "success")
            {
                return Redirect($"{GetClientUrl()}/payment/failed?trxId={paymentID}");
            }

            var gateway = _gatewayFactory.GetGateway(Enums.PaymentGateway.BkashGateway);
            var callbackData = new Dictionary<string, string> { { "paymentID", paymentID }, { "status", status } };

            var isValid = await gateway.VerifyCallbackAsync(callbackData, cancellationToken);

            if (isValid)
            {
                // 24.13: Pass bKash paymentID as the idempotency key.
                await HandleSuccessfulPayment(trunkTrxId, cancellationToken, gatewayPaymentId: paymentID);
                return Redirect($"{GetClientUrl()}/payment/success?trxId={trunkTrxId}");
            }

            _logger.LogWarning("bKash Callback Verification/Execution FAILED for {PaymentID}", trunkTrxId);
            return Redirect($"{GetClientUrl()}/payment/failed?trxId={trunkTrxId}");
        }

        [HttpGet("callback/dgepay")]
        [AllowAnonymous]
        public async Task<IActionResult> DGePayCallback([FromQuery] string data, CancellationToken cancellationToken)
        {
            _logger.LogInformation("DGePay Callback Received.");

            var gateway = _gatewayFactory.GetGateway(Enums.PaymentGateway.DGePay);
            var callbackData = new Dictionary<string, string> { { "data", data } };

            var isValid = await gateway.VerifyCallbackAsync(callbackData, cancellationToken);

            // DGePayGateway.VerifyCallbackAsync populates callbackData with unique_txn_id and amount
            var trunkTrxId = callbackData.TryGetValue("unique_txn_id", out var tid) ? tid : "N/A";

            if (isValid)
            {
                // 29B.2: verify the amount only when the gateway actually reported one.
                decimal? amount = callbackData.TryGetValue("amount", out var a)
                    ? (decimal.TryParse(a, out var amt) ? amt : 0m)
                    : (decimal?)null;

                await HandleSuccessfulPayment(trunkTrxId, cancellationToken, amount, gatewayPaymentId: trunkTrxId);
                return Redirect($"{GetClientUrl()}/payment/success?trxId={trunkTrxId}");
            }

            _logger.LogWarning("DGePay Callback Verification FAILED for {TrxID}", trunkTrxId);
            return Redirect($"{GetClientUrl()}/payment/failed?trxId={trunkTrxId}");
        }

        [HttpPost("webhook/{gateway}")]
        [AllowAnonymous]
        public async Task<IActionResult> GatewayWebhook(string gateway, CancellationToken cancellationToken)
        {
            if (!Enum.TryParse<Enums.PaymentGateway>(gateway, true, out var gatewayType))
                return BadRequest("Invalid gateway type.");

            _logger.LogInformation("Webhook received for {Gateway}", gatewayType);

            // 29G.4: A value that parses as a valid enum name (e.g. Stripe, NagadGateway) may still
            // have no registered implementation — GetGateway throws NotSupportedException in that
            // case. Guard it so an unconfigured gateway returns 404 instead of an unhandled 500.
            IPaymentGatewayService gatewayService;
            try
            {
                gatewayService = _gatewayFactory.GetGateway(gatewayType);
            }
            catch (NotSupportedException ex)
            {
                _logger.LogWarning(ex, "Webhook for unregistered gateway {Gateway}", gatewayType);
                return NotFound(new { status = "unsupported_gateway" });
            }
            var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

            var result = await gatewayService.ProcessWebhookAsync(Request.Body, headers, cancellationToken);

            if (result.IsValid && !string.IsNullOrEmpty(result.TransactionId))
            {
                await HandleSuccessfulPayment(result.TransactionId, cancellationToken, result.ConfirmedAmount, result.GatewayPaymentId);
                return Ok(new { status = "success" });
            }

            return BadRequest(new { status = "failed" });
        }

        private async Task HandleSuccessfulPayment(string transactionId, CancellationToken cancellationToken, decimal? confirmedAmount = null, string? gatewayPaymentId = null)
        {
            // 24.13: Idempotency check — short-circuit if this gateway payment was already processed.
            if (!string.IsNullOrEmpty(gatewayPaymentId))
            {
                var alreadyProcessed = await _db.PaymentHistories.AnyAsync(
                    p => p.GatewayPaymentId == gatewayPaymentId && p.Status == Enums.PaymentStatus.Completed,
                    cancellationToken);
                if (alreadyProcessed)
                {
                    _logger.LogInformation("Duplicate callback ignored for GatewayPaymentId {GwId}", gatewayPaymentId);
                    return;
                }
            }

            var payment = await _db.PaymentHistories.FirstOrDefaultAsync(p => p.TransactionId == transactionId, cancellationToken);
            if (payment == null || payment.Status == Enums.PaymentStatus.Completed) return;

            // 24.13: Persist the gateway payment ID for future idempotency checks.
            if (!string.IsNullOrEmpty(gatewayPaymentId))
                payment.GatewayPaymentId = gatewayPaymentId;

            // 29B.2 Security Check: whenever the gateway REPORTS an amount it must match the recorded
            // amount — including a reported 0. The old `confirmedAmount > 0` guard let a 0 (or absent)
            // amount skip verification entirely and silently complete an unverified payment. A null now
            // means the gateway did not echo an amount at all (e.g. bKash's GET callback, whose amount is
            // authenticated separately inside VerifyCallbackAsync); a value of 0 is treated as reported and
            // will fail against any positive expected amount.
            if (confirmedAmount.HasValue && Math.Abs(payment.Amount - confirmedAmount.Value) > 0.01m)
            {
                _logger.LogWarning("Payment amount mismatch for {TrxID}. Expected {E}, Received {R}. Mark as discrepancy.", transactionId, payment.Amount, confirmedAmount.Value);
                await _financialService.UpdatePaymentStatusAsync(payment.Id, Enums.PaymentStatus.Failed, $"Amount mismatch detected. Paid: {confirmedAmount.Value}, Expected: {payment.Amount}", cancellationToken);
                return;
            }

            // 1. Mark as Completed
            await _financialService.UpdatePaymentStatusAsync(payment.Id, Enums.PaymentStatus.Completed, "Verified via Gateway Automatic Protocol", cancellationToken);

            // 2. If it was an Event Registration, Auto-Approve the registration.
            // 82.32: was a case-sensitive Contains("EVT-REG-") against free text, which missed a
            // lowercase client-supplied reference even though FinancialCategory was already set
            // correctly for exactly this payment (see InitiatePayment). FinancialCategory is now
            // the authoritative check here; the Notes text is only used to recover which
            // registration this payment was for.
            if (payment.FinancialCategory == Enums.FinancialCategory.RegistrationFee)
            {
                var refPrefixIndex = payment.Notes?.IndexOf("Ref: ", StringComparison.OrdinalIgnoreCase) ?? -1;
                // The reference itself never contains a space; anything from the first space
                // onward is trailing text this format appends (" (Guest)", or free text a caller
                // added), same as the original Split("EVT-REG-")[1].Split(" ")[0] this replaces.
                var fullRef = refPrefixIndex >= 0
                    ? payment.Notes!.Substring(refPrefixIndex + "Ref: ".Length).Split(' ')[0].Trim()
                    : null;

                var registration = string.IsNullOrEmpty(fullRef)
                    ? null
                    : await _db.EventRegistrations.Include(r => r.Event).FirstOrDefaultAsync(r => r.PaymentReference == fullRef, cancellationToken);

                if (registration != null && registration.Status == Enums.EventRegistrationStatus.Pending && registration.Event != null)
                {
                    // Verify sufficient amount paid for the event
                    var ev = registration.Event;
                    var expectedAmount = (ev.RegistrationFee ?? 0) > 0 ? (ev.RegistrationFee ?? 0) : (registration.ContributionAmount ?? 0);
                    if (payment.Amount >= expectedAmount)
                    {
                        var adminIdStr = _config["GeneralSettings:SystemAdminId"] ?? "1";
                        int.TryParse(adminIdStr, out var adminId);

                        _logger.LogInformation("Auto-Approving Event Registration {Id} for reference {Ref}", registration.Id, fullRef);
                        registration.Status = Enums.EventRegistrationStatus.Approved;
                        registration.ApprovedAt = DateTime.UtcNow;
                        registration.ApprovedByAdminId = adminId; // System Admin
                        await _db.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        _logger.LogWarning("Event registration {Id} under-paid: Expected {E}, Paid {P}", registration.Id, expectedAmount, payment.Amount);
                    }
                }
            }

            // 3. If it was a MEMBERSHIP fee for an 'Applied' member, Auto-Approve the member.
            //    29B.3: Scope strictly to membership-fee payments. Event-registration payments also carry a
            //    MemberId, so without this FinancialCategory guard a member paying an event fee would be
            //    silently auto-inducted as a full member. Gateway-initiated payments always set the category
            //    at initiation (see InitiatePayment), so this is a reliable discriminator on this code path.
            if (payment.MemberId is int membershipPayerId && membershipPayerId > 0
                && payment.FinancialCategory == Enums.FinancialCategory.MembershipFee)
            {
                var member = await _db.Members.FindAsync(new object[] { membershipPayerId }, cancellationToken);
                if (member != null && member.Status == Enums.MembershipStatus.Applied)
                {
                    // 82.32: was an inline query missing IsActive, the Category filter and the
                    // EffectiveTo upper bound — a disabled or expired fee row, or a differently
                    // categorised row for the same MembershipType (e.g. an EventFee), could win by
                    // being the most recent EffectiveDate. GetApplicableFeeAsync is the one query
                    // every other fee lookup in the codebase uses; this path had drifted from it.
                    var required = await _financialService.GetApplicableFeeAsync(
                        Enums.FinancialCategory.MembershipFee, member.MembershipType, DateTime.UtcNow, cancellationToken);

                    // S4.3: Fail loudly when fee config is missing — do not silently default.
                    if (required <= 0)
                    {
                        _logger.LogError("Auto-approval skipped for Member {Id}: no fee config found for type {Type}", member.Id, member.MembershipType);
                        return;
                    }
                    if (payment.Amount >= required)
                    {
                        // S4.3: Fail loudly when SystemAdminId is not configured.
                        var adminIdStr = _config["GeneralSettings:SystemAdminId"]
                            ?? throw new InvalidOperationException("GeneralSettings:SystemAdminId is not configured.");
                        int.TryParse(adminIdStr, out var adminId);

                        _logger.LogInformation("Auto-Approving Member {MemberId} after successful gateway payment.", member.Id);
                        await _memberService.ApproveMemberAsync(member.Id, adminId, cancellationToken);
                    }
                    else
                    {
                        _logger.LogWarning("Member {Id} under-paid subscription: Required {R}, Paid {P}", member.Id, required, payment.Amount);
                    }
                }
            }
        }

        private string GetClientUrl()
        {
            return _config.GetSection("AppSettings:AllowedOrigins")?.Get<string[]>()?.FirstOrDefault() ?? "http://localhost:4200";
        }

        public class InitiatePaymentRequest
        {
            public decimal Amount { get; set; }
            public Enums.PaymentGateway Gateway { get; set; }
            public string Reference { get; set; } = null!;
            public string? CustomerName { get; set; }
            public string? CustomerEmail { get; set; }
            public string? CustomerPhone { get; set; }
        }
    }
}
