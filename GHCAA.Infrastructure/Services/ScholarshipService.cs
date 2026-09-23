using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services;

public sealed class ScholarshipService : IScholarshipService
{
    private readonly ApplicationDbContext _db;
    public ScholarshipService(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<ScholarshipFundDto>> GetPublicFundsAsync(CancellationToken cancellationToken = default)
        => (await _db.ScholarshipFunds.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(cancellationToken)).Select(MapFund);

    public async Task<IEnumerable<ScholarshipCallDto>> GetPublicCallsAsync(CancellationToken cancellationToken = default)
        => (await _db.ScholarshipCalls.Where(x => x.IsActive && x.ScholarshipFund!.IsActive)
            .OrderByDescending(x => x.OpensOn).ToListAsync(cancellationToken)).Select(MapCall);

    public async Task<ScholarshipApplicationDto> SubmitApplicationAsync(int callId, CreateScholarshipApplicationDto dto, CancellationToken cancellationToken = default)
    {
        var call = await _db.ScholarshipCalls.FirstOrDefaultAsync(x => x.Id == callId && x.IsActive && x.ScholarshipFund!.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Scholarship call not found.");
        var now = DateTime.UtcNow;
        if (now < call.OpensOn || now > call.ClosesOn)
            throw new InvalidOperationException("This scholarship call is not accepting applications.");
        var application = new ScholarshipApplication
        {
            ScholarshipCallId = callId,
            ApplicantName = dto.ApplicantName.Trim(),
            ApplicantEmail = dto.ApplicantEmail.Trim(),
            ApplicantPhone = dto.ApplicantPhone.Trim(),
            InstitutionName = dto.InstitutionName.Trim(),
            Class = dto.Class.Trim(),
            GuardianName = dto.GuardianName.Trim(),
            HouseholdIncome = dto.HouseholdIncome,
            NeedStatement = dto.NeedStatement.Trim(),
            MeritStatement = dto.MeritStatement.Trim(),
            Status = Enums.ScholarshipApplicationStatus.Submitted,
            SubmittedAt = now,
            ReferenceCode = await CreateReferenceCodeAsync(cancellationToken)
        };
        _db.ScholarshipApplications.Add(application);
        await _db.SaveChangesAsync(cancellationToken);
        return MapApplication(application);
    }

    public async Task<ScholarshipStatusDto?> GetPublicStatusAsync(string referenceCode, string email, CancellationToken cancellationToken = default)
    {
        var application = await _db.ScholarshipApplications.Include(x => x.ScholarshipCall).Include(x => x.Award)
            .FirstOrDefaultAsync(x => x.ReferenceCode == referenceCode && x.ApplicantEmail == email, cancellationToken);
        if (application == null || application.ScholarshipCall == null) return null;
        return new ScholarshipStatusDto
        {
            ReferenceCode = application.ReferenceCode,
            Status = application.Status,
            AcademicYear = application.ScholarshipCall.AcademicYear,
            SubmittedAt = application.SubmittedAt,
            AwardAmount = application.Award?.Amount,
            DisbursementStatus = application.Award?.DisbursementStatus
        };
    }

    public async Task<IEnumerable<ScholarshipReviewDto>> GetReviewQueueAsync(CancellationToken cancellationToken = default)
    {
        var apps = await _db.ScholarshipApplications.Include(x => x.Reviews)
            .Where(x => x.Status == Enums.ScholarshipApplicationStatus.Submitted || x.Status == Enums.ScholarshipApplicationStatus.UnderReview || x.Status == Enums.ScholarshipApplicationStatus.Shortlisted)
            .OrderBy(x => x.SubmittedAt).ToListAsync(cancellationToken);
        return apps.Select(MapReview);
    }

    public async Task<ScholarshipReviewDto?> GetApplicationForReviewAsync(int applicationId, CancellationToken cancellationToken = default)
    {
        var app = await _db.ScholarshipApplications.Include(x => x.Reviews).FirstOrDefaultAsync(x => x.Id == applicationId, cancellationToken);
        return app == null ? null : MapReview(app);
    }

    public async Task<ScholarshipReviewDto?> SubmitReviewAsync(int applicationId, int reviewerMemberId, SubmitScholarshipReviewDto dto, CancellationToken cancellationToken = default)
    {
        var app = await _db.ScholarshipApplications.Include(x => x.Reviews).FirstOrDefaultAsync(x => x.Id == applicationId, cancellationToken);
        if (app == null) return null;
        var review = app.Reviews.FirstOrDefault(x => x.ReviewerMemberId == reviewerMemberId);
        if (review == null)
        {
            review = new ScholarshipReview { ScholarshipApplicationId = applicationId, ReviewerMemberId = reviewerMemberId };
            _db.ScholarshipReviews.Add(review);
        }
        review.NeedScore = dto.NeedScore; review.MeritScore = dto.MeritScore; review.Comments = dto.Comments; review.ReviewedAt = DateTime.UtcNow;
        if (app.Status == Enums.ScholarshipApplicationStatus.Submitted) app.Status = Enums.ScholarshipApplicationStatus.UnderReview;
        await _db.SaveChangesAsync(cancellationToken);
        return MapReview(app);
    }

    public async Task<ScholarshipFundDto> CreateFundAsync(CreateScholarshipFundDto dto, CancellationToken cancellationToken = default)
    {
        var fund = new ScholarshipFund { Name = dto.Name.Trim(), Description = dto.Description.Trim(), NamedAfter = dto.NamedAfter?.Trim(), TargetAmount = dto.TargetAmount, IsActive = dto.IsActive };
        _db.ScholarshipFunds.Add(fund); await _db.SaveChangesAsync(cancellationToken); return MapFund(fund);
    }

    public async Task<ScholarshipCallDto> CreateCallAsync(CreateScholarshipCallDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.ClosesOn <= dto.OpensOn) throw new ArgumentException("Closing date must be after opening date.");
        if (!await _db.ScholarshipFunds.AnyAsync(x => x.Id == dto.ScholarshipFundId && x.IsActive, cancellationToken)) throw new KeyNotFoundException("Scholarship fund not found.");
        var call = new ScholarshipCall { ScholarshipFundId = dto.ScholarshipFundId, AcademicYear = dto.AcademicYear.Trim(), OpensOn = DateTime.SpecifyKind(dto.OpensOn, DateTimeKind.Utc), ClosesOn = DateTime.SpecifyKind(dto.ClosesOn, DateTimeKind.Utc), SlotCount = dto.SlotCount, AwardAmount = dto.AwardAmount, EligibilityCriteria = dto.EligibilityCriteria.Trim(), IsActive = dto.IsActive };
        _db.ScholarshipCalls.Add(call); await _db.SaveChangesAsync(cancellationToken); return MapCall(call);
    }

    public async Task<IEnumerable<ScholarshipApplicationDto>> GetApplicationsForAdminAsync(CancellationToken cancellationToken = default)
        => (await _db.ScholarshipApplications.OrderByDescending(x => x.SubmittedAt).ToListAsync(cancellationToken)).Select(MapApplication);

    public async Task<ScholarshipAwardDto> CreateAwardAsync(CreateScholarshipAwardDto dto, CancellationToken cancellationToken = default)
    {
        var app = await _db.ScholarshipApplications.FirstOrDefaultAsync(x => x.Id == dto.ScholarshipApplicationId, cancellationToken) ?? throw new KeyNotFoundException("Application not found.");
        var award = await _db.ScholarshipAwards.FirstOrDefaultAsync(x => x.ScholarshipApplicationId == app.Id, cancellationToken);
        if (award == null) { award = new ScholarshipAward { ScholarshipApplicationId = app.Id, Amount = dto.Amount, AwardedOn = DateTime.UtcNow, DisbursementStatus = Enums.DisbursementStatus.Approved }; _db.ScholarshipAwards.Add(award); }
        else if (award.DisbursementStatus == Enums.DisbursementStatus.Pending) award.Amount = dto.Amount;
        app.Status = Enums.ScholarshipApplicationStatus.Awarded;
        await _db.SaveChangesAsync(cancellationToken);
        return MapAward(award, app.ReferenceCode);
    }

    public async Task<bool> DisburseAwardAsync(int awardId, int adminId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var award = await _db.ScholarshipAwards.Include(x => x.ScholarshipApplication).FirstOrDefaultAsync(x => x.Id == awardId, cancellationToken);
        if (award == null || award.ScholarshipApplication == null) return false;
        if (award.FinancialRecordId.HasValue || award.DisbursementStatus == Enums.DisbursementStatus.Paid) { await transaction.CommitAsync(cancellationToken); return true; }
        if (award.DisbursementStatus == Enums.DisbursementStatus.Cancelled) return false;
        var now = DateTime.UtcNow;
        var record = new FinancialRecord
        {
            Year = now.Year,
            RecordType = Enums.FinancialRecordType.Expense,
            FinancialCategory = Enums.FinancialCategory.Grant,
            Date = now,
            Amount = award.Amount,
            Description = $"Scholarship grant - {award.ScholarshipApplication.ReferenceCode}",
            Reference = award.ScholarshipApplication.ReferenceCode,
            CreatedAt = now,
            CreatedByAdminId = adminId
        };
        _db.FinancialRecords.Add(record);
        award.FinancialRecord = record; award.DisbursementStatus = Enums.DisbursementStatus.Paid;
        await _db.SaveChangesAsync(cancellationToken); await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private async Task<string> CreateReferenceCodeAsync(CancellationToken ct)
    {
        for (var i = 0; i < 5; i++)
        {
            var code = $"SCH-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
            if (!await _db.ScholarshipApplications.AnyAsync(x => x.ReferenceCode == code, ct)) return code;
        }
        throw new InvalidOperationException("Could not create a unique application reference.");
    }
    private static ScholarshipFundDto MapFund(ScholarshipFund x) => new() { Id = x.Id, Name = x.Name, Description = x.Description, NamedAfter = x.NamedAfter, TargetAmount = x.TargetAmount, IsActive = x.IsActive };
    private static ScholarshipCallDto MapCall(ScholarshipCall x) => new() { Id = x.Id, ScholarshipFundId = x.ScholarshipFundId, AcademicYear = x.AcademicYear, OpensOn = x.OpensOn, ClosesOn = x.ClosesOn, SlotCount = x.SlotCount, AwardAmount = x.AwardAmount, EligibilityCriteria = x.EligibilityCriteria, IsActive = x.IsActive };
    private static ScholarshipApplicationDto MapApplication(ScholarshipApplication x) => new() { Id = x.Id, ScholarshipCallId = x.ScholarshipCallId, ApplicantName = x.ApplicantName, ApplicantEmail = x.ApplicantEmail, ApplicantPhone = x.ApplicantPhone, InstitutionName = x.InstitutionName, Class = x.Class, GuardianName = x.GuardianName, HouseholdIncome = x.HouseholdIncome, NeedStatement = x.NeedStatement, MeritStatement = x.MeritStatement, Status = x.Status, SubmittedAt = x.SubmittedAt, ReferenceCode = x.ReferenceCode };
    private static ScholarshipReviewDto MapReview(ScholarshipApplication x) { var r = x.Reviews.OrderByDescending(y => y.ReviewedAt).FirstOrDefault(); return new() { Id = r?.Id ?? 0, ScholarshipApplicationId = x.Id, ReferenceCode = x.ReferenceCode, NeedScore = r?.NeedScore ?? 0, MeritScore = r?.MeritScore ?? 0, Comments = r?.Comments, ReviewedAt = r?.ReviewedAt }; }
    private static ScholarshipAwardDto MapAward(ScholarshipAward x, string reference) => new() { Id = x.Id, ScholarshipApplicationId = x.ScholarshipApplicationId, ReferenceCode = reference, Amount = x.Amount, AwardedOn = x.AwardedOn, DisbursementStatus = x.DisbursementStatus, FinancialRecordId = x.FinancialRecordId };
}
