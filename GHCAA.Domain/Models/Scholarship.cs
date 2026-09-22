using System;
using System.Collections.Generic;

namespace GHCAA.Domain.Models;

public class ScholarshipFund
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? NamedAfter { get; set; }
    public decimal TargetAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ScholarshipCall> Calls { get; set; } = new List<ScholarshipCall>();
}

public class ScholarshipCall
{
    public int Id { get; set; }
    public int ScholarshipFundId { get; set; }
    public string AcademicYear { get; set; } = null!;
    public DateTime OpensOn { get; set; }
    public DateTime ClosesOn { get; set; }
    public int SlotCount { get; set; }
    public decimal AwardAmount { get; set; }
    public string EligibilityCriteria { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public ScholarshipFund? ScholarshipFund { get; set; }
    public ICollection<ScholarshipApplication> Applications { get; set; } = new List<ScholarshipApplication>();
}

public class ScholarshipApplication
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
    public Enums.ScholarshipApplicationStatus Status { get; set; } = Enums.ScholarshipApplicationStatus.Submitted;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public string ReferenceCode { get; set; } = null!;
    public ScholarshipCall? ScholarshipCall { get; set; }
    public ICollection<ScholarshipDocument> Documents { get; set; } = new List<ScholarshipDocument>();
    public ICollection<ScholarshipReview> Reviews { get; set; } = new List<ScholarshipReview>();
    public ScholarshipAward? Award { get; set; }
}

public class ScholarshipDocument
{
    public int Id { get; set; }
    public int ScholarshipApplicationId { get; set; }
    public int FileUploadId { get; set; }
    public string DocumentType { get; set; } = null!;
    public ScholarshipApplication? ScholarshipApplication { get; set; }
    public FileUpload? FileUpload { get; set; }
}

public class ScholarshipReview
{
    public int Id { get; set; }
    public int ScholarshipApplicationId { get; set; }
    public int ReviewerMemberId { get; set; }
    public decimal NeedScore { get; set; }
    public decimal MeritScore { get; set; }
    public string? Comments { get; set; }
    public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;
    public ScholarshipApplication? ScholarshipApplication { get; set; }
    public Member? ReviewerMember { get; set; }
}

public class ScholarshipAward
{
    public int Id { get; set; }
    public int ScholarshipApplicationId { get; set; }
    public decimal Amount { get; set; }
    public DateTime AwardedOn { get; set; } = DateTime.UtcNow;
    public Enums.DisbursementStatus DisbursementStatus { get; set; } = Enums.DisbursementStatus.Pending;
    public int? FinancialRecordId { get; set; }
    public ScholarshipApplication? ScholarshipApplication { get; set; }
    public FinancialRecord? FinancialRecord { get; set; }
}
