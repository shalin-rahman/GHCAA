using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Tests.Services;

[TestFixture]
public class ScholarshipServiceTests : TestBase
{
    private ScholarshipService NewService() => new(_context);

    private async Task<ScholarshipCall> SeedOpenCallAsync()
    {
        var fund = new ScholarshipFund
        {
            Name = "Student Aid Fund",
            Description = "Support for students.",
            TargetAmount = 10000m
        };
        _context.ScholarshipFunds.Add(fund);
        await _context.SaveChangesAsync();

        var call = new ScholarshipCall
        {
            ScholarshipFundId = fund.Id,
            AcademicYear = "2026",
            OpensOn = DateTime.UtcNow.AddDays(-1),
            ClosesOn = DateTime.UtcNow.AddDays(1),
            SlotCount = 2,
            AwardAmount = 5000m,
            EligibilityCriteria = "Open to eligible students."
        };
        _context.ScholarshipCalls.Add(call);
        await _context.SaveChangesAsync();
        return call;
    }

    [Test]
    public async Task SubmitApplicationAsync_DoesNotCreateMemberOrUser()
    {
        var call = await SeedOpenCallAsync();
        var membersBefore = await _context.Members.CountAsync();
        var usersBefore = await _context.Users.CountAsync();

        var result = await NewService().SubmitApplicationAsync(call.Id, new CreateScholarshipApplicationDto
        {
            ApplicantName = "Student Applicant",
            ApplicantEmail = "student@example.com",
            ApplicantPhone = "01700000000",
            InstitutionName = "Govt. College",
            Class = "Class 10",
            GuardianName = "Guardian",
            HouseholdIncome = 25000m,
            NeedStatement = "Needs support.",
            MeritStatement = "Strong academic record."
        });

        result.ReferenceCode.Should().StartWith("SCH-");
        result.Status.Should().Be(Enums.ScholarshipApplicationStatus.Submitted);
        (await _context.Members.CountAsync()).Should().Be(membersBefore);
        (await _context.Users.CountAsync()).Should().Be(usersBefore);
    }

    [Test]
    public async Task ReviewQueueAsync_DoesNotExposeApplicantIdentity()
    {
        var call = await SeedOpenCallAsync();
        var application = new ScholarshipApplication
        {
            ScholarshipCallId = call.Id,
            ApplicantName = "Private Student",
            ApplicantEmail = "private@example.com",
            ApplicantPhone = "01700000001",
            InstitutionName = "Private Institution",
            Class = "Class 9",
            GuardianName = "Private Guardian",
            HouseholdIncome = 30000m,
            NeedStatement = "Need statement.",
            MeritStatement = "Merit statement.",
            ReferenceCode = "SCH-TEST-001"
        };
        _context.ScholarshipApplications.Add(application);
        await _context.SaveChangesAsync();

        var review = (await NewService().GetReviewQueueAsync()).Single();

        review.ReferenceCode.Should().Be(application.ReferenceCode);
        review.Should().NotBeNull();
        review.GetType().GetProperty(nameof(ScholarshipApplication.ApplicantName)).Should().BeNull();
    }

    [Test]
    public async Task SubmitApplicationAsync_RejectsApplicationOutsideCallWindow()
    {
        var call = await SeedOpenCallAsync();
        call.OpensOn = DateTime.UtcNow.AddDays(2);
        call.ClosesOn = DateTime.UtcNow.AddDays(3);
        await _context.SaveChangesAsync();

        var action = () => NewService().SubmitApplicationAsync(call.Id, new CreateScholarshipApplicationDto
        {
            ApplicantName = "Late Applicant",
            ApplicantEmail = "late@example.com",
            ApplicantPhone = "01700000003",
            InstitutionName = "Govt. College",
            Class = "Class 8",
            GuardianName = "Guardian",
            HouseholdIncome = 20000m,
            NeedStatement = "Need statement.",
            MeritStatement = "Merit statement."
        });

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("This scholarship call is not accepting applications.");
    }

    [Test]
    public async Task DisburseAwardAsync_IsIdempotent_AndWritesOneGrantRecord()
    {
        var call = await SeedOpenCallAsync();
        var application = new ScholarshipApplication
        {
            ScholarshipCallId = call.Id,
            ApplicantName = "Awarded Student",
            ApplicantEmail = "awarded@example.com",
            ApplicantPhone = "01700000002",
            InstitutionName = "Govt. College",
            Class = "Class 10",
            GuardianName = "Guardian",
            HouseholdIncome = 25000m,
            NeedStatement = "Need statement.",
            MeritStatement = "Merit statement.",
            ReferenceCode = "SCH-TEST-002",
            Status = Enums.ScholarshipApplicationStatus.Awarded
        };
        _context.ScholarshipApplications.Add(application);
        await _context.SaveChangesAsync();
        var award = new ScholarshipAward
        {
            ScholarshipApplicationId = application.Id,
            Amount = 5000m,
            DisbursementStatus = Enums.DisbursementStatus.Approved
        };
        _context.ScholarshipAwards.Add(award);
        await _context.SaveChangesAsync();

        var service = NewService();
        (await service.DisburseAwardAsync(award.Id, 1)).Should().BeTrue();
        (await service.DisburseAwardAsync(award.Id, 1)).Should().BeTrue();

        var grants = await _context.FinancialRecords
            .Where(x => x.FinancialCategory == Enums.FinancialCategory.Grant)
            .ToListAsync();
        grants.Should().ContainSingle();
        grants[0].RecordType.Should().Be(Enums.FinancialRecordType.Expense);
        grants[0].Reference.Should().Be(application.ReferenceCode);
    }
}
