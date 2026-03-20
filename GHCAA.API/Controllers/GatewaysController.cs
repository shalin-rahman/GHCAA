using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        public GatewaysController(
            IPaymentGatewayFactory gatewayFactory, 
            IFinancialService financialService,
            IMemberService memberService,
            ApplicationDbContext db,
            ILogger<GatewaysController> logger,
            IConfiguration config)
        {
            _gatewayFactory = gatewayFactory;
            _financialService = financialService;
            _memberService = memberService;
            _db = db;
            _logger = logger;
            _config = config;
        }

        [HttpPost("initiate")]
        [AllowAnonymous]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentRequest request, CancellationToken cancellationToken)
        {
            int? memberId = null;
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            if (!string.IsNullOrEmpty(memberIdClaim) && int.TryParse(memberIdClaim, out var mid))
            {
                memberId = mid;
            }

            var gatewayService = _gatewayFactory.GetGateway(request.Gateway);
            
            // Create a pending payment history record first
            var prefix = _config["GeneralSettings:AssociationNamePrefix"] ?? "GHCAA-";
            var trxId = prefix + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
            
            var initiateDto = new PaymentGatewayInitiationDto
            {
                MemberId = memberId,
                Amount = request.Amount,
                Currency = _config["GeneralSettings:Currency"] ?? "BDT",
                Reference = request.Reference, // e.g. "Registration" or "Due-2024"
                TransactionId = trxId,
                CallbackUrl = $"{request.BaseUrl}/api/gateways/callback/{request.Gateway.ToString().ToLower()}",
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
                    MemberId = memberId ?? 0,
                    Amount = request.Amount,
                    TransactionId = trxId,
                    PaidAt = DateTime.UtcNow,
                    FinancialCategory = request.Reference.Contains("EVT-REG") ? Enums.FinancialCategory.RegistrationFee : Enums.FinancialCategory.MembershipFee,
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
                var amount = data.TryGetValue("amount", out var a) && decimal.TryParse(a, out var amt) ? amt : 0;
                await HandleSuccessfulPayment(trunkTrxId, cancellationToken, amount);
                return Redirect($"{GetClientUrl()}/payment/success?trxId={trunkTrxId}");
            }

            _logger.LogWarning("SSLCommerz Callback Verification FAILED for {TrxID}", trunkTrxId);
            return Redirect($"{GetClientUrl()}/payment/failed?trxId={trunkTrxId}");
        }

        [HttpPost("callback/bkash")]
        [AllowAnonymous]
        public async Task<IActionResult> BkashCallback([FromBody] IDictionary<string, string> data, CancellationToken cancellationToken)
        {
            // bKash usually returns paymentID and status.
            return Ok();
        }

        private async Task HandleSuccessfulPayment(string transactionId, CancellationToken cancellationToken, decimal confirmedAmount = 0)
        {
            var payment = await _db.PaymentHistories.FirstOrDefaultAsync(p => p.TransactionId == transactionId, cancellationToken);
            if (payment == null || payment.Status == Enums.PaymentStatus.Completed) return;

            // Security Check: Verify amount matches the record (if provided)
            if (confirmedAmount > 0 && Math.Abs(payment.Amount - confirmedAmount) > 0.01m)
            {
                _logger.LogWarning("Payment amount mismatch for {TrxID}. Expected {E}, Received {R}. Mark as discrepancy.", transactionId, payment.Amount, confirmedAmount);
                await _financialService.UpdatePaymentStatusAsync(payment.Id, Enums.PaymentStatus.Failed, $"Amount mismatch detected. Paid: {confirmedAmount}, Expected: {payment.Amount}", cancellationToken);
                return;
            }

            // 1. Mark as Completed
            await _financialService.UpdatePaymentStatusAsync(payment.Id, Enums.PaymentStatus.Completed, "Verified via Gateway Automatic Protocol", cancellationToken);

            // 2. If it was an Event Registration, Auto-Approve the registration
            if (payment.Notes != null && payment.Notes.Contains("EVT-REG-"))
            {
                var regRef = payment.Notes.Split("EVT-REG-")[1].Split(" ")[0]; // Extract just the reference
                var fullRef = "EVT-REG-" + regRef;
                var registration = await _db.EventRegistrations.Include(r => r.Event).FirstOrDefaultAsync(r => r.PaymentReference == fullRef, cancellationToken);
                
                if (registration != null && registration.Status == Enums.EventRegistrationStatus.Pending)
                {
                    // Verify sufficient amount paid for the event
                    var expectedAmount = (registration.Event.RegistrationFee > 0) ? (registration.Event.RegistrationFee ?? 0) : registration.ContributionAmount;
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

            // 3. If it was Registration/Membership fee for 'Applied' status, Auto-Approve Member
            if (payment.MemberId > 0)
            {
                var member = await _db.Members.FindAsync(new object[] { payment.MemberId }, cancellationToken);
                if (member != null && member.Status == Enums.MembershipStatus.Applied)
                {
                    // Check if payment amount matches the membership type fee
                    var currentFee = await _db.MembershipFeeConfigs
                        .Where(f => f.MembershipType == member.MembershipType && f.EffectiveDate <= DateTime.UtcNow)
                        .OrderByDescending(f => f.EffectiveDate)
                        .FirstOrDefaultAsync(cancellationToken);

                    var required = currentFee?.Amount ?? 500; // Default to 500 if not found
                    if (payment.Amount >= required)
                    {
                        var adminIdStr = _config["GeneralSettings:SystemAdminId"] ?? "1";
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
            public string BaseUrl { get; set; } = null!; // The API base URL for callback construction
            public string? CustomerName { get; set; }
            public string? CustomerEmail { get; set; }
            public string? CustomerPhone { get; set; }
        }
    }
}
