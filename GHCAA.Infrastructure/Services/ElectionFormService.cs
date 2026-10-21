using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static GHCAA.Domain.Constants;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Services;

// 37.14m. Every change here is written to the election's audit chain in the same save.
public sealed class ElectionFormService(ApplicationDbContext db, IElectionAuditWriter? audit = null) : IElectionFormService
{
    private readonly IElectionAuditWriter _audit = audit ?? new ElectionAuditWriter(db);

    // A time typed on a phone can run a little ahead of the server clock.
    private static readonly TimeSpan ClockSkew = TimeSpan.FromMinutes(5);

    public async Task<(string? Error, string? Detail)> UpdateFormDetailsAsync(int electionId, UpdateElectionFormDetailsRequest request, int actorUserId, CancellationToken ct = default)
    {
        var e = await db.Elections.FirstOrDefaultAsync(x => x.Id == electionId, ct);
        if (e == null) return (Elections.Replies.NotFound, null);
        if (e.Phase == ElectionPhase.Archived) return (Elections.Replies.WrongPhase, "An archived election cannot be changed.");

        var finalList = Utc(request.FinalListOn);
        var campaignOpens = Utc(request.CampaignOpensOn);
        var campaignCloses = Utc(request.CampaignClosesOn);
        var silence = Utc(request.SilenceStartsAt);
        var recountEnds = Utc(request.RecountWindowEndsOn);

        if (campaignOpens is { } opens && campaignCloses is { } closes && closes < opens)
            return (Elections.Replies.Invalid, "The campaign cannot close before it opens.");
        // The final list is a day, not a time, so a list dated the day nominations close is allowed.
        if (finalList is { } list && (list.Date < e.NominationClosesOn.Date || list.Date > e.PollingOpensOn.Date))
            return (Elections.Replies.Invalid, "The final list date must fall between the close of nominations and the start of polling.");
        if (silence is { } quiet && quiet >= e.PollingClosesOn)
            return (Elections.Replies.Invalid, "Silence must start before polling closes.");
        if (recountEnds is { } ends && ends < e.PollingClosesOn)
            return (Elections.Replies.Invalid, "The recount window cannot end before polling closes.");

        var from = FormDetails(e);
        e.FinalListOn = finalList;
        e.CampaignOpensOn = campaignOpens;
        e.CampaignClosesOn = campaignCloses;
        e.SilenceStartsAt = silence;
        e.Venue = Blank(request.Venue);
        e.NoticeReference = Blank(request.NoticeReference);
        e.RecountWindowEndsOn = recountEnds;
        await _audit.AppendAsync(electionId, Elections.AuditActions.FormDetailsChanged, actorUserId, new { from, to = FormDetails(e) }, ct);
        await _audit.SaveAsync(ct);
        return (null, null);
    }

    public async Task<string?> RecordConsentAsync(int electionId, int nominationId, DateTime consentAt, int actorUserId, CancellationToken ct = default)
    {
        var nomination = await db.Nominations.Include(x => x.Election)
            .FirstOrDefaultAsync(x => x.Id == nominationId && x.ElectionId == electionId, ct);
        if (nomination == null) return Elections.Replies.NotFound;
        // A consent signed before the list was published can still be entered during the campaign.
        // Once polling opens it would only be a back-dated paper.
        if (nomination.Election!.Phase >= ElectionPhase.Polling) return Elections.Replies.WrongPhase;
        var at = ElectionService.AsUtc(consentAt);
        if (at > DateTime.UtcNow.Add(ClockSkew)) return Elections.Replies.Invalid;

        nomination.ConsentAt = at;
        await _audit.AppendAsync(electionId, Elections.AuditActions.ConsentRecorded, actorUserId, new { nominationId, consentAt = at }, ct);
        await _audit.SaveAsync(ct);
        return null;
    }

    public async Task<IReadOnlyList<ScrutinyCheckDto>?> GetScrutinyChecksAsync(int electionId, int nominationId, CancellationToken ct = default)
    {
        if (!await db.Nominations.AnyAsync(x => x.Id == nominationId && x.ElectionId == electionId, ct)) return null;
        return await db.NominationScrutinyChecks.AsNoTracking().Where(x => x.NominationId == nominationId)
            .OrderBy(x => x.Item)
            .Select(x => new ScrutinyCheckDto(x.Item, x.IsChecked, x.SetByUserId, x.SetAt)).ToListAsync(ct);
    }

    public async Task<string?> SetScrutinyCheckAsync(int electionId, int nominationId, SetScrutinyCheckRequest request, int actorUserId, CancellationToken ct = default)
    {
        if (!Enum.IsDefined(request.Item)) return Elections.Replies.Invalid;
        var phase = await db.Nominations.Where(x => x.Id == nominationId && x.ElectionId == electionId)
            .Select(x => (ElectionPhase?)x.Election!.Phase).FirstOrDefaultAsync(ct);
        if (phase == null) return Elections.Replies.NotFound;
        if (phase != ElectionPhase.Scrutiny) return Elections.Replies.WrongPhase;

        var check = await db.NominationScrutinyChecks.FirstOrDefaultAsync(x => x.NominationId == nominationId && x.Item == request.Item, ct);
        if (check == null)
        {
            check = new NominationScrutinyCheck { NominationId = nominationId, Item = request.Item };
            db.NominationScrutinyChecks.Add(check);
        }
        check.IsChecked = request.IsChecked;
        check.SetByUserId = actorUserId;
        check.SetAt = DateTime.UtcNow;
        await _audit.AppendAsync(electionId, Elections.AuditActions.ScrutinyChecked, actorUserId, new { nominationId, item = request.Item.ToString(), request.IsChecked }, ct);
        // Two officials ticking the same item at once trip the unique index on (nomination, item).
        return await SaveOrBusyAsync(ct);
    }

    public async Task<IReadOnlyList<ElectionRecountDto>> ListRecountsAsync(int electionId, CancellationToken ct = default)
    {
        var rows = await db.ElectionRecountRequests.AsNoTracking().Include(x => x.Figures)
            .Where(x => x.ElectionId == electionId).OrderBy(x => x.ReceivedAt).ThenBy(x => x.Id).ToListAsync(ct);
        var names = await SeatNamesAsync(electionId, ct);
        return rows.Select(x => ToDto(x, names)).ToList();
    }

    public async Task<(ElectionRecountDto? Recount, string? Error, string? Detail)> RecordRecountAsync(int electionId, RecordRecountRequest request, int actorUserId, CancellationToken ct = default)
    {
        var e = await db.Elections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == electionId, ct);
        if (e == null) return (null, Elections.Replies.NotFound, null);
        if (e.Phase != ElectionPhase.Declared) return (null, Elections.Replies.WrongPhase, "A recount can only be asked for after the result is declared.");
        if (request.ReceivedAt is not { } received) return (null, Elections.Replies.Invalid, "Give the time the request was received.");
        var receivedAt = ElectionService.AsUtc(received);
        if (receivedAt > DateTime.UtcNow.Add(ClockSkew)) return (null, Elections.Replies.Invalid, "The received time cannot be in the future.");
        if (e.RecountWindowEndsOn is { } ends && receivedAt > ends)
            return (null, Elections.Replies.Invalid, "The request arrived after the recount window ended.");
        if (!await db.ElectionSeats.AnyAsync(x => x.Id == request.ElectionSeatId && x.ElectionId == electionId, ct))
            return (null, Elections.Replies.Invalid, "That position is not on this election.");
        // Recount figures are checked against the declared result rows, so a seat without any could never be counted.
        if (!await db.ElectionResults.AnyAsync(x => x.ElectionId == electionId && x.ElectionSeatId == request.ElectionSeatId, ct))
            return (null, Elections.Replies.Invalid, "That position has no declared result to recount.");
        if (request.ApplicantMemberId is { } memberId && !await db.Members.AnyAsync(x => x.Id == memberId, ct))
            return (null, Elections.Replies.Invalid, "That member does not exist.");

        var row = new ElectionRecountRequest
        {
            ElectionId = electionId,
            ElectionSeatId = request.ElectionSeatId,
            ApplicantName = request.ApplicantName.Trim(),
            ApplicantMemberId = request.ApplicantMemberId,
            Grounds = request.Grounds.Trim(),
            Explanation = Blank(request.Explanation),
            ReceivedAt = receivedAt,
            RecordedByUserId = actorUserId
        };
        db.ElectionRecountRequests.Add(row);
        await _audit.AppendAsync(electionId, Elections.AuditActions.RecountRequested, actorUserId, new { row.ElectionSeatId, row.ApplicantName, row.ReceivedAt }, ct);
        await _audit.SaveAsync(ct);
        return (ToDto(row, await SeatNamesAsync(electionId, ct)), null, null);
    }

    public async Task<string?> DecideRecountAsync(int electionId, int recountId, DecideRecountRequest request, int actorUserId, CancellationToken ct = default)
    {
        var row = await db.ElectionRecountRequests.Include(x => x.Election)
            .FirstOrDefaultAsync(x => x.Id == recountId && x.ElectionId == electionId, ct);
        if (row == null) return Elections.Replies.NotFound;
        if (row.Election!.Phase != ElectionPhase.Declared) return Elections.Replies.WrongPhase;
        if (row.Allowed != null) return Elections.Replies.AlreadyRecorded;
        if (!request.Allowed && string.IsNullOrWhiteSpace(request.Reason)) return Elections.Replies.Invalid;

        row.Allowed = request.Allowed;
        row.DecisionReason = Blank(request.Reason);
        row.DecidedByUserId = actorUserId;
        row.DecidedAt = DateTime.UtcNow;
        await _audit.AppendAsync(electionId, Elections.AuditActions.RecountDecided, actorUserId, new { recountId, request.Allowed, reason = row.DecisionReason }, ct);
        return await SaveOrBusyAsync(ct);
    }

    // The figures are kept beside the declared result for the recount form (ER-28B). They do not
    // change ElectionResults; a changed outcome still goes through an appeal.
    public async Task<(string? Error, string? Detail)> RecordRecountFiguresAsync(int electionId, int recountId, IReadOnlyList<RecountFigureInput> figures, int actorUserId, CancellationToken ct = default)
    {
        var row = await db.ElectionRecountRequests.Include(x => x.Election)
            .FirstOrDefaultAsync(x => x.Id == recountId && x.ElectionId == electionId, ct);
        if (row == null) return (Elections.Replies.NotFound, null);
        if (row.Election!.Phase != ElectionPhase.Declared) return (Elections.Replies.WrongPhase, null);
        if (row.Allowed != true) return (Elections.Replies.WrongPhase, "Only an allowed recount can have figures.");
        if (row.RecountedAt != null) return (Elections.Replies.AlreadyRecorded, null);

        var expected = await db.ElectionResults.Where(x => x.ElectionId == electionId && x.ElectionSeatId == row.ElectionSeatId)
            .Select(x => x.NominationId).ToListAsync(ct);
        var given = figures.Select(x => x.NominationId).ToList();
        if (given.Count != given.Distinct().Count() || !given.ToHashSet().SetEquals(expected))
            return (Elections.Replies.Invalid, "Give one figure for each candidate on the position, and no others.");
        if (figures.Any(x => x.VoteCount < 0)) return (Elections.Replies.Invalid, "A vote count cannot be negative.");

        foreach (var figure in figures)
            row.Figures.Add(new ElectionRecountFigure { NominationId = figure.NominationId, VoteCount = figure.VoteCount });
        row.RecountedAt = DateTime.UtcNow;
        row.RecountedByUserId = actorUserId;
        await _audit.AppendAsync(electionId, Elections.AuditActions.RecountCounted, actorUserId,
            new { recountId, figures = figures.OrderBy(x => x.NominationId).Select(x => new { x.NominationId, x.VoteCount }) }, ct);
        return (await SaveOrBusyAsync(ct), null);
    }

    private async Task<string?> SaveOrBusyAsync(CancellationToken ct)
    {
        try
        {
            await _audit.SaveAsync(ct);
            return null;
        }
        // Another official saved first, so a unique index or a sibling audit row refused this one.
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            return Elections.Replies.Busy;
        }
    }

    private static object FormDetails(Election e) => new
    {
        finalListOn = e.FinalListOn, campaignOpensOn = e.CampaignOpensOn, campaignClosesOn = e.CampaignClosesOn,
        silenceStartsAt = e.SilenceStartsAt, venue = e.Venue, noticeReference = e.NoticeReference, recountWindowEndsOn = e.RecountWindowEndsOn
    };

    private Task<Dictionary<int, string>> SeatNamesAsync(int electionId, CancellationToken ct) =>
        db.ElectionSeats.AsNoTracking().Where(x => x.ElectionId == electionId)
            .ToDictionaryAsync(x => x.Id, x => x.EcPosition != null ? x.EcPosition.Name : string.Empty, ct);

    private static ElectionRecountDto ToDto(ElectionRecountRequest x, IReadOnlyDictionary<int, string> seatNames) =>
        new(x.Id, x.ElectionSeatId, seatNames.GetValueOrDefault(x.ElectionSeatId, string.Empty), x.ApplicantName, x.ApplicantMemberId, x.Grounds, x.Explanation,
            x.ReceivedAt, x.Allowed, x.DecisionReason, x.DecidedAt, x.RecountedAt,
            x.Figures.OrderBy(f => f.NominationId).Select(f => new RecountFigureDto(f.NominationId, f.VoteCount)).ToList());

    private static DateTime? Utc(DateTime? value) => value is { } v ? ElectionService.AsUtc(v) : null;

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
