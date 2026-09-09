using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Tests.Services;

/// <summary>
/// Work Package 82.16 — the audit trail on the two entities that record money.
///
/// Before this, FinancialLedgerService.DeleteRecordAsync called Remove() on a ledger row and
/// FinancialService.DeletePaymentAsync called Remove() on a payment, and neither update path
/// recorded who changed a value. A row recording money received could be altered or destroyed with
/// nothing left behind. These tests pin the three things that fixes: updates stamp an actor and a
/// time, deletes are soft, and a soft-deleted row stops appearing in ordinary reads while still
/// existing in the table.
/// </summary>
[TestFixture]
public class FinancialAuditTrailTests : TestBase
{
    private const int ActingAdminId = 42;

    private FinancialLedgerService NewLedgerService() => new(_context);

    private async Task<FinancialRecord> SeedRecordAsync(decimal amount = 500m)
    {
        var record = new FinancialRecord
        {
            Year = 2026,
            RecordType = Enums.FinancialRecordType.Income,
            FinancialCategory = Enums.FinancialCategory.Donation,
            Date = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            Amount = amount,
            Description = "Original description",
            Reference = "VCH-1",
            CreatedByAdminId = 7
        };
        await _context.FinancialRecords.AddAsync(record);
        await _context.SaveChangesAsync();
        return record;
    }

    [Category("FR-25")]
    [Category("FR-43")]
    [Test]
    public async Task UpdateRecord_RecordsWhoChangedItAndWhen()
    {
        var record = await SeedRecordAsync();
        record.UpdatedAt.Should().BeNull("a freshly created row has never been updated");

        var before = DateTime.UtcNow;
        record.Amount = 750m;
        record.Description = "Corrected amount";
        await NewLedgerService().UpdateRecordAsync(record, ActingAdminId);

        var saved = await _context.FinancialRecords.FindAsync(record.Id);
        saved!.Amount.Should().Be(750m);
        saved.UpdatedByAdminId.Should().Be(ActingAdminId);
        saved.UpdatedAt.Should().NotBeNull().And.BeOnOrAfter(before);
        saved.CreatedByAdminId.Should().Be(7, "the original author is not overwritten by an edit");
    }

    [Category("FR-25")]
    [Category("FR-44")]
    [Test]
    public async Task DeleteRecord_KeepsTheRow_AndRecordsWhoDeletedIt()
    {
        var record = await SeedRecordAsync();

        var deleted = await NewLedgerService().DeleteRecordAsync(record.Id, ActingAdminId);
        deleted.Should().BeTrue();

        // The row survives. This is the whole point: a deleted ledger entry is still evidence that
        // it once existed, and of who removed it.
        var stillThere = await _context.FinancialRecords
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == record.Id);

        stillThere.Should().NotBeNull("a financial record must never be physically removed");
        stillThere!.IsArchived.Should().BeTrue();
        stillThere.DeletedByAdminId.Should().Be(ActingAdminId);
        stillThere.DeletedAt.Should().NotBeNull();
        stillThere.Amount.Should().Be(500m, "the deleted row keeps its values, or it is not evidence");
    }

    [Category("FR-25")]
    [Category("FR-44")]
    [Test]
    public async Task DeletedRecord_DisappearsFromOrdinaryReads()
    {
        var kept = await SeedRecordAsync(100m);
        var removed = await SeedRecordAsync(200m);

        await NewLedgerService().DeleteRecordAsync(removed.Id, ActingAdminId);
        _context.ChangeTracker.Clear();

        // Callers that never asked about deletion must see exactly what they saw before 82.16 —
        // otherwise "soft delete" silently changes the meaning of every existing ledger query.
        var visible = await _context.FinancialRecords.ToListAsync();
        visible.Should().Contain(r => r.Id == kept.Id);
        visible.Should().NotContain(r => r.Id == removed.Id);
    }

    [Test]
    public async Task DeleteRecord_Twice_ReportsNotFoundTheSecondTime()
    {
        var record = await SeedRecordAsync();
        var service = NewLedgerService();

        (await service.DeleteRecordAsync(record.Id, ActingAdminId)).Should().BeTrue();

        // In production a fresh request gets a fresh scoped DbContext, so the second call's
        // FindAsync always runs a real query against the (now-filtered) DbSet and gets null there —
        // record.IsArchived is never actually read on that path. Without clearing the tracker here,
        // this assertion would pass for the wrong reason: through the still-tracked, still-`false`
        // second delete, in a state production never reaches.
        _context.ChangeTracker.Clear();

        (await service.DeleteRecordAsync(record.Id, ActingAdminId)).Should().BeFalse(
            "an already-deleted row must not be re-stamped with a new deleter, which would "
            + "overwrite the record of who actually deleted it");
    }

    [Test]
    public async Task DeleteRecord_UnknownId_ReturnsFalse()
    {
        (await NewLedgerService().DeleteRecordAsync(999999, ActingAdminId)).Should().BeFalse();
    }
}
