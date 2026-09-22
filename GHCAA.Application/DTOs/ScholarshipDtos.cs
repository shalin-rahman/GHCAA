using System.ComponentModel.DataAnnotations;
using GHCAA.Domain;

namespace GHCAA.Application.DTOs;

public class ScholarshipFundDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? NamedAfter { get; set; }
    public decimal TargetAmount { get; set; }
    public bool IsActive { get; set; }
}

public class CreateScholarshipFundDto
{
    [Required, MaxLength(200)] public string Name { get; set; } = null!;
    [Required, MaxLength(4000)] public string Description { get; set; } = null!;
    [MaxLength(200)] public string? NamedAfter { get; set; }
    [Range(0, double.MaxValue)] public decimal TargetAmount { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ScholarshipCallDto
{
    public int Id { get; set; }
    public int ScholarshipFundId { get; set; }
    public string AcademicYear { get; set; } = null!;
    public DateTime OpensOn { get; set; }
    public DateTime ClosesOn { get; set; }
    public int SlotCount { get; set; }
    public decimal AwardAmount { get; set; }
    public string EligibilityCriteria { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class CreateScholarshipCallDto
{
    [Required] public int ScholarshipFundId { get; set; }
    [Required, MaxLength(30)] public string AcademicYear { get; set; } = null!;
    [Required] public DateTime OpensOn { get; set; }
    [Required] public DateTime ClosesOn { get; set; }
    [Range(1, int.MaxValue)] public int SlotCount { get; set; }
    [Range(0.01, double.MaxValue)] public decimal AwardAmount { get; set; }
    [Required, MaxLength(4000)] public string EligibilityCriteria { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}

public class CreateScholarshipApplicationDto
{
    [Required, MaxLength(200)] public string ApplicantName { get; set; } = null!;
    [Required, EmailAddress, MaxLength(320)] public string ApplicantEmail { get; set; } = null!;
    [Required, MaxLength(50)] public string ApplicantPhone { get; set; } = null!;
    [Required, MaxLength(200)] public string InstitutionName { get; set; } = null!;
    [Required, MaxLength(100)] public string Class { get; set; } = null!;
    [Required, MaxLength(200)] public string GuardianName { get; set; } = null!;
    [Range(0, double.MaxValue)] public decimal HouseholdIncome { get; set; }
    [Required, MaxLength(4000)] public string NeedStatement { get; set; } = null!;
    [Required, MaxLength(4000)] public string MeritStatement { get; set; } = null!;
}

public class ScholarshipApplicationDto
{
    public int Id { get; set; }
    public int ScholarshipCallId { get; set; }
    public string ApplicantName { get; set; } = null!;
    public string ApplicantEmail { get; set; } = null!;
    public string ApplicantPhone { get; set; } = null!;
    public string InstitutionName { get; set; } = null!;
    public string Class { get; set; } = null!;
    public string GuardianName { get; set; } = null!;
    public decimal HouseholdIncome { get; set; }
    public string NeedStatement { get; set; } = null!;
    public string MeritStatement { get; set; } = null!;
    public Enums.ScholarshipApplicationStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string ReferenceCode { get; set; } = null!;
}

public class ScholarshipReviewDto
{
    public int Id { get; set; }
    public int ScholarshipApplicationId { get; set; }
    public string ReferenceCode { get; set; } = null!;
    public decimal NeedScore { get; set; }
    public decimal MeritScore { get; set; }
    public string? Comments { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class SubmitScholarshipReviewDto
{
    [Range(0, 100)] public decimal NeedScore { get; set; }
    [Range(0, 100)] public decimal MeritScore { get; set; }
    [MaxLength(2000)] public string? Comments { get; set; }
}

public class ScholarshipAwardDto
{
    public int Id { get; set; }
    public int ScholarshipApplicationId { get; set; }
    public string ReferenceCode { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime AwardedOn { get; set; }
    public Enums.DisbursementStatus DisbursementStatus { get; set; }
    public int? FinancialRecordId { get; set; }
}

public class CreateScholarshipAwardDto
{
    [Required] public int ScholarshipApplicationId { get; set; }
    [Range(0.01, double.MaxValue)] public decimal Amount { get; set; }
}

public class ScholarshipStatusDto
{
    public string ReferenceCode { get; set; } = null!;
    public Enums.ScholarshipApplicationStatus Status { get; set; }
    public string AcademicYear { get; set; } = null!;
    public DateTime SubmittedAt { get; set; }
    public decimal? AwardAmount { get; set; }
    public Enums.DisbursementStatus? DisbursementStatus { get; set; }
}
