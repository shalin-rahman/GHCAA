using System.ComponentModel.DataAnnotations;
using static GHCAA.Domain.Enums;

namespace GHCAA.Domain.Models
{
    public class PaymentConfiguration
    {
        public int Id { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        [Required]
        [MaxLength(100)]
        public string DisplayName { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Icon { get; set; } // emoji or icon class

        public bool IsEnabled { get; set; } = true;

        // For mobile wallets: bKash, Nagad, Rocket
        [MaxLength(20)]
        public string? WalletNumber { get; set; }

        [MaxLength(100)]
        public string? AccountHolderName { get; set; }

        // For bank transfers
        [MaxLength(100)]
        public string? BankName { get; set; }

        [MaxLength(50)]
        public string? BranchName { get; set; }

        [MaxLength(50)]
        public string? AccountNumber { get; set; }

        [MaxLength(50)]
        public string? RoutingNumber { get; set; }

        // For card payments (gateway integration)
        public PaymentGateway Gateway { get; set; } = PaymentGateway.None;

        [MaxLength(500)]
        public string? GatewayPublicKey { get; set; }

        [MaxLength(500)]
        public string? GatewaySecretKey { get; set; }

        [MaxLength(500)]
        public string? GatewayCallbackUrl { get; set; }

        // UI ordering
        public int SortOrder { get; set; } = 0;

        // Instructional text shown to users
        [MaxLength(1000)]
        public string? Instructions { get; set; }

        // Whether receipt upload is required for this method
        public bool RequiresReceipt { get; set; } = true;

        // Whether TxnID / reference is required
        public bool RequiresReference { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
