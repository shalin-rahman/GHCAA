using System.ComponentModel.DataAnnotations;
using GHCAA.Domain;
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
        // 95.2: Position is the EcPosition key (the old enum name), kept so clients still match on it.
        public string Position { get; set; } = string.Empty;
        public int PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public MemberSummaryDto? Member { get; set; }
    }

    // 95.3: one row per committee position in a period. Holder is null when the seat is
    // empty; the vacancy fields then come from the last person who held it, if anyone did.
    public class CommitteeSeatDto
    {
        // 95.2: Position is the EcPosition key (the old enum name), kept so clients still match on it.
        public string Position { get; set; } = string.Empty;
        public int PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        // 95.2: from EcPosition.ShowOnBoard, so the board page needs no hard-coded list.
        public bool ShowOnBoard { get; set; }
        public ECMemberDto? Holder { get; set; }
        public VacancyReason? VacancyReason { get; set; }
        public string? VacancyNote { get; set; }
        public DateTime? VacatedOn { get; set; }
        public string? LastHolderName { get; set; }
    }

    // 95.2: one committee position from the EcPositions table.
    public class EcPositionDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public int DisplayOrder { get; set; }
        public int SeatCount { get; set; }
        public bool IsActive { get; set; }
        public bool ShowOnBoard { get; set; }
        public bool SignsAsHead { get; set; }
        public bool SignsAsSecretary { get; set; }
    }

    // The key is made from the name on create and never changes after, because clients and
    // filters match on it.
    public class SaveEcPositionRequest
    {
        [Required]
        [MaxLength(Constants.Governance.PositionNameMaxLength)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(Constants.Governance.PositionNameMaxLength)]
        public string? NameBn { get; set; }
        [Range(0, Constants.Governance.PositionMaxDisplayOrder)]
        public int DisplayOrder { get; set; }
        [Range(1, Constants.Governance.PositionMaxSeats)]
        public int SeatCount { get; set; } = 1;
        public bool ShowOnBoard { get; set; }
        public bool SignsAsHead { get; set; }
        public bool SignsAsSecretary { get; set; }
    }

    // 95.2: a committee term or election seat whose position matches no row, such as the old
    // None = 0 value. Kind is one of Constants.Governance.RepairKinds.
    public class InvalidPositionRowDto
    {
        public string Kind { get; set; } = string.Empty;
        public int Id { get; set; }
        public int Position { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    // 37.14m. One ER-34 handover item for a committee term.
    public record HandoverCheckDto(HandoverCheckItem Item, bool IsChecked, int SetByUserId, DateTime SetAt);

    public class SetHandoverCheckRequest
    {
        [EnumDataType(typeof(HandoverCheckItem))]
        public HandoverCheckItem Item { get; set; }

        public bool IsChecked { get; set; }
    }

    public class RepairPositionRequest
    {
        [Required]
        public string Kind { get; set; } = string.Empty;
        public int Id { get; set; }
        [Range(1, int.MaxValue)]
        public int PositionId { get; set; }
    }
}
