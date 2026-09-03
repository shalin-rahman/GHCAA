namespace GHCAA.Application.DTOs
{
    public class PaymentGatewayInitiationDto
    {
        public int? MemberId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "BDT";
        public string Reference { get; set; } = null!; // E.g. "Registration-123"
        public string? TransactionId { get; set; }
        public string CallbackUrl { get; set; } = null!;
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
    }

    public class PaymentGatewayResponseDto
    {
        public bool Success { get; set; }
        public string? GatewayUrl { get; set; }
        public string? TransactionId { get; set; }
        public string? Message { get; set; }
    }

    // What a gateway's ProcessWebhookAsync needs to hand back so the caller can run the same
    // HandleSuccessfulPayment path the redirect callback already uses. TransactionId must match
    // PaymentHistories.TransactionId; ConfirmedAmount follows the same null/0 distinction as the
    // redirect path (null = the gateway didn't report an amount, 0 = it reported zero).
    public class PaymentWebhookResultDto
    {
        public bool IsValid { get; set; }
        public string? TransactionId { get; set; }
        public decimal? ConfirmedAmount { get; set; }
        public string? GatewayPaymentId { get; set; }

        public static PaymentWebhookResultDto Invalid() => new() { IsValid = false };
    }
}
