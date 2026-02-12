using System;
using System.ComponentModel.DataAnnotations;
using static GHCAA.Domain.Enums;

namespace GHCAA.Domain.Models
{
    public class FinancialRecord
    {
        public int Id { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public FinancialRecordType RecordType { get; set; }

        [Required]
        public FinancialCategory Category { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = null!;

        [MaxLength(100)]
        public string? Reference { get; set; } // e.g., Voucher #, Bank Ref

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CreatedByAdminId { get; set; }
    }
}
