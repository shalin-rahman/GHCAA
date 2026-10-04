using GHCAA.Application.DTOs;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs
{
    public class ECPeriodDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public List<ECMemberDto> ECMembers { get; set; } = new();
    }

    public class ECMemberDto
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public ECPosition Position { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public MemberSummaryDto? Member { get; set; }
    }

    // 95.3: one row per committee position in a period. Holder is null when the seat is
    // empty; the vacancy fields then come from the last person who held it, if anyone did.
    public class CommitteeSeatDto
    {
        public ECPosition Position { get; set; }
        public ECMemberDto? Holder { get; set; }
        public VacancyReason? VacancyReason { get; set; }
        public string? VacancyNote { get; set; }
        public DateTime? VacatedOn { get; set; }
        public string? LastHolderName { get; set; }
    }
}
