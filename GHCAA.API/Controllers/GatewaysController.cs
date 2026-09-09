using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using System.Linq;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GHCAA.Application.Security;
using GHCAA.API.Extensions;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/gateways")]
    public class GatewaysController : ControllerBase
    {
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly IFinancialService _financialService;
        private readonly IEventService _eventService;
        private readonly IPaymentCallbackOrchestrator _callbackOrchestrator;
        private readonly IPaymentConfigService _paymentConfigService;
        private readonly ILogger<GatewaysController> _logger;
        private readonly IConfiguration _config;
        private readonly IOrgConfigService _orgConfig;

        public GatewaysController(
            IPaymentGatewayFactory gatewayFactory,
            IFinancialService financialService,
            IEventService eventService,
            IPaymentCallbackOrchestrator callbackOrchestrator,
            IPaymentConfigService paymentConfigService,
            ILogger<GatewaysController> logger,
            IConfiguration config,
            IOrgConfigService orgConfig)
        {
            _gatewayFactory = gatewayFactory;
            _financialService = financialService;
            _eventService = eventService;
            _callbackOrchestrator = callbackOrchestrator;
            _paymentConfigService = paymentConfigService;
            _logger = logger;
            _config = config;
            _orgConfig = orgConfig;
        }

        [HttpPost("initiate")]
        [AllowAnonymous]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentRequest request, CancellationToken cancellationToken)
        {
            if (request.Amount <= 0 || request.Amount > 10_000_000m)
                return Problem(detail: "Payment amount is out of the allowed range.", statusCode: StatusCodes.Status400BadRequest);

            if (string.IsNullOrWhiteSpace(request.Reference))
                return Problem(detail: "Reference is required.", statusCode: StatusCodes.Status400BadRequest);

            int? memberId = null;
            var memberIdClaim = this.CurrentMemberIdRaw();
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
                    return Problem(detail: "Sign in is required to start this payment.", statusCode: StatusCodes.Status401Unauthorized);
                }
            }
            else
            {
                var registration = await _eventService.GetRegistrationByPaymentReferenceAsync(request.Reference, cancellationToken);
                if (registration != null && registration.Status != Enums.EventRegistrationStatus.Pending)
                    registration = null;

                if (registration?.Event == null)
                {
                    _logger.LogWarning("Event payment initiation failed: unknown or non-pending registration {Ref}", request.Reference);
                    return Problem(detail: "Unknown or inactive event registration reference.", statusCode: StatusCodes.Status400BadRequest);
                }

                var regFee = registration.Event.RegistrationFee ?? 0;
                var expected = regFee > 0 ? regFee : (registration.ContributionAmount ?? 0);

                if (expected <= 0 && request.Amount > 0.01m)
                    return Problem(detail: "This registration does not require an online payment.", statusCode: StatusCodes.Status400BadRequest);

                if (expected > 0 && Math.Abs(request.Amount - expected) > 0.01m)
                {
                    _logger.LogWarning(
                        "Event payment amount mismatch for {Ref}. Expected {Expected}, got {Actual}",
                        request.Reference,
                        expected,
                        request.Amount);
                    return Problem(detail: $"Amount must match the event fee ({expected}).", statusCode: StatusCodes.Status400BadRequest);
                }

                if (registration.MemberId.HasValue)
                {
                    if (!memberId.HasValue || memberId.Value != registration.MemberId.Value)
                    {
                        _logger.LogWarning("Event payment member mismatch for {Ref}", request.Reference);
                        return Problem(detail: "Sign in as the member who registered to complete payment.", statusCode: StatusCodes.Status401Unauthorized);
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
                return Problem(detail: "This payment method is temporarily unavailable via system configuration.", statusCode: StatusCodes.Status400BadRequest);
            }

            var dbConfig = await _paymentConfigService.GetEnabledByGatewayAsync(request.Gateway, cancellationToken);
            if (dbConfig == null)
            {
                _logger.LogWarning("Blocked initiation of disabled gateway (DB): {Gateway}", request.Gateway);
                return Problem(detail: "This payment method is not active in the registry.", statusCode: StatusCodes.Status400BadRequest);
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

            return Problem(detail: response.Message, statusCode: StatusCodes.Status400BadRequest);
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
                await _callbackOrchestrator.HandleSuccessfulPaymentAsync(trunkTrxId, cancellationToken, amount, valId);
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
                await _callbackOrchestrator.HandleSuccessfulPaymentAsync(trunkTrxId, cancellationToken, gatewayPaymentId: paymentID);
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

                await _callbackOrchestrator.HandleSuccessfulPaymentAsync(trunkTrxId, cancellationToken, amount, gatewayPaymentId: trunkTrxId);
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
                return Problem(detail: "Invalid gateway type.", statusCode: StatusCodes.Status400BadRequest);

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
                return Problem(detail: "unsupported_gateway", statusCode: StatusCodes.Status404NotFound);
            }
            var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

            var result = await gatewayService.ProcessWebhookAsync(Request.Body, headers, cancellationToken);

            if (result.IsValid && !string.IsNullOrEmpty(result.TransactionId))
            {
                await _callbackOrchestrator.HandleSuccessfulPaymentAsync(result.TransactionId, cancellationToken, result.ConfirmedAmount, result.GatewayPaymentId);
                return Ok(new { status = "success" });
            }

            return Problem(detail: "failed", statusCode: StatusCodes.Status400BadRequest);
        }

        // Business orchestration moved to IPaymentCallbackOrchestrator (82.67).

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
