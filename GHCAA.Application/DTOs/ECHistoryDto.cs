using System;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs
{
    public class ECHistoryDto
    {
        public int Id { get; set; }
        public int PeriodId { get; set; }
        public string PeriodTitle { get; set; } = null!;
        public ECPosition Position { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ChangeReason { get; set; }
        public bool IsCurrent { get; set; }
    }
}
