using System;

namespace GHCAA.Application.DTOs
{
    public class SavedPaymentMethodDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public bool IsDefault { get; set; }
    }

    public class CreateSavedPaymentMethodDto
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
    }
}
