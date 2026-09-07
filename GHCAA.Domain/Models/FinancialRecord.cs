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
        public FinancialCategory FinancialCategory { get; set; }

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

        // 82.16. A ledger row records money. Before this, it could be edited or hard-deleted with
        // no trace of the previous value and no trace of who did it — the audit's top finding, and
        // the first thing anyone reviewing an association's accounts asks about. Updates now stamp
        // who and when; deletes set IsArchived instead of removing the row, so a correction never
        // destroys the record it corrects.
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedByAdminId { get; set; }
        public bool IsArchived { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByAdminId { get; set; }
    }
}
