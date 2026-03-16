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

        public GatewaysController(
            IPaymentGatewayFactory gatewayFactory, 
            IFinancialService financialService,
            IMemberService memberService,
            ApplicationDbContext db,
            ILogger<GatewaysController> logger)
        {
            _gatewayFactory = gatewayFactory;
            _financialService = financialService;
            _memberService = memberService;
            _db = db;
            _logger = logger;
        }

        [HttpPost("initiate")]
        [Authorize]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentRequest request, CancellationToken cancellationToken)
        {
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId))
            {
                return BadRequest("Member association not found.");
            }

            var gatewayService = _gatewayFactory.GetGateway(request.Gateway);
            
            // Create a pending payment history record first
            var trxId = "GHCAA-" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
            
            var initiateDto = new PaymentGatewayInitiationDto
            {
                MemberId = memberId,
                Amount = request.Amount,
                Reference = request.Reference, // e.g. "Registration" or "Due-2024"
                TransactionId = trxId,
                CallbackUrl = $"{request.BaseUrl}/api/gateways/callback/{request.Gateway.ToString().ToLower()}"
            };

            var response = await gatewayService.InitiatePaymentAsync(initiateDto, cancellationToken);
            
            if (response.Success)
            {
                // Record the intent in history
                await _financialService.RecordPaymentAsync(new CreatePaymentHistoryDto
                {
                    MemberId = memberId,
                    Amount = request.Amount,
                    TransactionId = trxId,
                    PaidAt = DateTime.UtcNow,
                    Category = request.Reference.Contains("Registration") ? Enums.FinancialCategory.RegistrationFee : Enums.FinancialCategory.MembershipFee,
                    Notes = $"Initiated via {request.Gateway}. Ref: {request.Reference}"
                }, cancellationToken);

                return Ok(response);
            }

            return BadRequest(response.Message);
        }

        [HttpPost("callback/sslcommerz")]
        [AllowAnonymous]
        public async Task<IActionResult> SSLCommerzCallback([FromForm] IDictionary<string, string> data, CancellationToken cancellationToken)
        {
            _logger.LogInformation("SSLCommerz Callback Received: {TrxID}", data.TryGetValue("tran_id", out var tid) ? tid : "N/A");

            var status = data.TryGetValue("status", out var s) ? s : "";
            var trxId = data.TryGetValue("tran_id", out var t) ? t : "";

            if (status == "VALID" || status == "AUTHENTICATED")
            {
                await HandleSuccessfulPayment(trxId, cancellationToken);
                return Redirect($"{GetClientUrl()}/payment/success?trxId={trxId}");
            }

            return Redirect($"{GetClientUrl()}/payment/failed?trxId={trxId}");
        }

        [HttpPost("callback/bkash")]
        [AllowAnonymous]
        public async Task<IActionResult> BkashCallback([FromBody] IDictionary<string, string> data, CancellationToken cancellationToken)
        {
            // bKash usually returns paymentID and status.
            return Ok();
        }

        private async Task HandleSuccessfulPayment(string transactionId, CancellationToken cancellationToken)
        {
            var payment = await _db.PaymentHistories.FirstOrDefaultAsync(p => p.TransactionId == transactionId, cancellationToken);
            if (payment == null || payment.Status == Enums.PaymentStatus.Completed) return;

            // 1. Mark as Completed
            await _financialService.UpdatePaymentStatusAsync(payment.Id, Enums.PaymentStatus.Completed, "Verified via Gateway Automatic Protocol", cancellationToken);

            // 2. If it was an Event Registration, Auto-Approve the registration
            if (payment.Notes != null && payment.Notes.Contains("EVT-REG-"))
            {
                var regRef = payment.Notes.Split("EVT-REG-")[1].Split(" ")[0]; // Extract just the reference
                var registration = await _db.EventRegistrations.Include(r => r.Event).FirstOrDefaultAsync(r => r.PaymentReference == "EVT-REG-" + regRef, cancellationToken);
                if (registration != null && registration.Status == Enums.EventRegistrationStatus.Pending)
                {
                    _logger.LogInformation("Auto-Approving Event Registration {Id} for reference {Ref}", registration.Id, registration.PaymentReference);
                    registration.Status = Enums.EventRegistrationStatus.Approved;
                    registration.ApprovedAt = DateTime.UtcNow;
                    // Admin ID 1 for system auto-approval
                    registration.ApprovedByAdminId = 1;
                    await _db.SaveChangesAsync(cancellationToken);
                }
            }

            // 3. If it was Registration/Membership fee for 'Applied' status, Auto-Approve Member
            var member = await _db.Members.FindAsync(new object[] { payment.MemberId }, cancellationToken);
            if (member != null && member.Status == Enums.MembershipStatus.Applied)
            {
                _logger.LogInformation("Auto-Approving Member {MemberId} after successful gateway payment.", member.Id);
                // System Admin ID (Usually 1)
                await _memberService.ApproveMemberAsync(member.Id, 1, cancellationToken);
            }
        }

        private string GetClientUrl() => "http://localhost:4200"; // Should come from config

        public class InitiatePaymentRequest
        {
            public decimal Amount { get; set; }
            public Enums.PaymentGateway Gateway { get; set; }
            public string Reference { get; set; } = null!;
            public string BaseUrl { get; set; } = null!; // The API base URL for callback construction
        }
    }
}
