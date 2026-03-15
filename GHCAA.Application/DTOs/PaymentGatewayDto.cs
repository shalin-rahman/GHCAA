namespace GHCAA.Application.DTOs
{
    public class PaymentGatewayInitiationDto
    {
        public int MemberId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "BDT";
        public string Reference { get; set; } = null!; // E.g. "Registration-123"
        public string? TransactionId { get; set; }
        public string CallbackUrl { get; set; } = null!;
    }

    public class PaymentGatewayResponseDto
    {
        public bool Success { get; set; }
        public string? GatewayUrl { get; set; }
        public string? TransactionId { get; set; }
        public string? Message { get; set; }
    }
}
