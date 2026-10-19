using FluentAssertions;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GHCAA.Tests.Services;

// TODO 94.1 to 94.4: what stays visible once a member is archived.
[TestFixture]
public class ArchivedMemberRelationshipTests : TestBase
{
    private async Task ArchiveAsync(Member member)
    {
        member.IsArchived = true;
        await _context.SaveChangesAsync();
    }

    private async Task<ScholarshipApplication> SeedApplicationAsync()
    {
        var fund = new ScholarshipFund { Name = "Fund", Description = "d", TargetAmount = 1000m };
        _context.ScholarshipFunds.Add(fund);
        await _context.SaveChangesAsync();
        var call = new ScholarshipCall
        {
            ScholarshipFundId = fund.Id,
            AcademicYear = "2026",
            OpensOn = DateTime.UtcNow.AddDays(-1),
            ClosesOn = DateTime.UtcNow.AddDays(1),
            SlotCount = 1,
            AwardAmount = 100m,
            EligibilityCriteria = "x"
        };
        _context.ScholarshipCalls.Add(call);
        await _context.SaveChangesAsync();
        var app = new ScholarshipApplication
        {
            ScholarshipCallId = call.Id,
            ApplicantName = "A",
            ApplicantEmail = "a@example.com",
            ApplicantPhone = "017",
            InstitutionName = "I",
            Class = "10",
            GuardianName = "G",
            NeedStatement = "n",
            MeritStatement = "m",
            ReferenceCode = "REF-1"
        };
        _context.ScholarshipApplications.Add(app);
        await _context.SaveChangesAsync();
        return app;
    }

    [Test]
    public async Task VoterRoll_KeepsTheRowOfAnArchivedMember()
    {
        var member = await CreateAndSaveTestMemberAsync();
        var period = new ECPeriod { Title = "2026", StartDate = DateTime.UtcNow.AddDays(-1) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        var election = new Election { Title = "E", ECPeriodId = period.Id, NominationOpensOn = DateTime.UtcNow, NominationClosesOn = DateTime.UtcNow.AddDays(1), PollingOpensOn = DateTime.UtcNow.AddDays(2), PollingClosesOn = DateTime.UtcNow.AddDays(3) };
        _context.Elections.Add(election);
        await _context.SaveChangesAsync();
        _context.VoterRolls.Add(new VoterRoll { ElectionId = election.Id, MemberId = member.Id, IsEligible = true, VotedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        await ArchiveAsync(member);

        (await _context.VoterRolls.CountAsync(x => x.ElectionId == election.Id && x.IsEligible)).Should().Be(1);
        (await _context.VoterRolls.CountAsync(x => x.ElectionId == election.Id && x.VotedAt != null)).Should().Be(1);
    }

    [Test]
    public async Task Application_KeepsTheReviewOfAnArchivedReviewer()
    {
        var reviewer = await CreateAndSaveTestMemberAsync();
        var app = await SeedApplicationAsync();
        _context.ScholarshipReviews.Add(new ScholarshipReview { ScholarshipApplicationId = app.Id, ReviewerMemberId = reviewer.Id, NeedScore = 4, MeritScore = 5 });
        await _context.SaveChangesAsync();

        await ArchiveAsync(reviewer);

        var loaded = await _context.ScholarshipApplications.Include(x => x.Reviews).SingleAsync(x => x.Id == app.Id);
        loaded.Reviews.Should().ContainSingle();
    }

    [Test]
    public async Task Application_KeepsADocumentWhoseUploaderIsArchived()
    {
        var uploader = await CreateAndSaveTestMemberAsync();
        var app = await SeedApplicationAsync();
        var upload = new FileUpload { MemberId = uploader.Id, FileName = "id.pdf", FilePath = "uploads/id.pdf" };
        _context.FileUploads.Add(upload);
        await _context.SaveChangesAsync();
        _context.ScholarshipDocuments.Add(new ScholarshipDocument { ScholarshipApplicationId = app.Id, FileUploadId = upload.Id, DocumentType = "ID" });
        await _context.SaveChangesAsync();

        await ArchiveAsync(uploader);

        var loaded = await _context.ScholarshipApplications.Include(x => x.Documents).SingleAsync(x => x.Id == app.Id);
        loaded.Documents.Should().ContainSingle();
    }

    [Test]
    public async Task VerifyCredential_AnArchivedMembersCredentialNoLongerVerifies()
    {
        var member = await CreateAndSaveTestMemberAsync();
        _context.IssuedCredentials.Add(new IssuedCredential { MemberId = member.Id, ShortCode = "ABC123" });
        await _context.SaveChangesAsync();
        var service = new IDCardService(_context, new Mock<GHCAA.Application.Interfaces.IOrgConfigService>().Object,
            new Mock<GHCAA.Application.Interfaces.IFileStorageService>().Object,
            new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>().Object);
        (await service.VerifyCredentialAsync("ABC123")).Should().NotBeNull();

        await ArchiveAsync(member);

        (await service.VerifyCredentialAsync("ABC123")).Should().BeNull();
    }
}
