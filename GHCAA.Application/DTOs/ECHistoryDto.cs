using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs
{
    public class ECHistoryDto
    {
        public int Id { get; set; }
        public int PeriodId { get; set; }
        public string PeriodTitle { get; set; } = null!;
        // 95.2: Position is the EcPosition key (the old enum name), kept so clients still match on it.
        public string Position { get; set; } = string.Empty;
        public int PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ChangeReason { get; set; }
        public VacancyReason? EndReason { get; set; }
        public string? EndNote { get; set; }
        public bool IsCurrent { get; set; }
    }
}
