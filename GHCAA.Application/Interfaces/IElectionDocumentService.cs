using GHCAA.Application.DTOs;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.Interfaces;

public interface IElectionDocumentService
{
    // 37.14s. A null mode uses the org's DefaultPrintMode. 37.14t. A filled print is refused before the
    // phase that produces the form, and each filled print is stored as a print record.
    Task<ElectionPdfResult> GeneratePdfAsync(int electionId, string formCode, ElectionPrinter printer, ElectionPrintMode? mode = null, CancellationToken cancellationToken = default);

    // 37.14s. The form with ruled lines and no election, for printing ahead of time.
    Task<byte[]?> GenerateBlankPdfAsync(string formCode, CancellationToken cancellationToken = default);

    // 37.14o. The voter's receipt. It carries no choices, and nothing about it is stored.
    Task<byte[]?> GenerateBallotReceiptPdfAsync(int electionId, string trackingCode, DateTime castAtUtc, CancellationToken cancellationToken = default);

    // 37.14n. Backs the verify page the form's QR code opens. Null when the election or the form code is unknown.
    Task<ElectionDocumentVerifyDto?> VerifyAsync(int electionId, string formCode, CancellationToken cancellationToken = default);

    // 37.14t. Null when the id is unknown or belongs to a test election.
    Task<ElectionFormPrintVerifyDto?> VerifyPrintAsync(Guid printId, CancellationToken cancellationToken = default);

    // 37.16i. Newest first, test elections included. Null when the election is unknown.
    Task<IReadOnlyList<ElectionFormPrintRowDto>?> ListPrintsAsync(int electionId, CancellationToken cancellationToken = default);

    // 37.14t. Marks the record revoked and writes the audit entry. NotFound when the record is not on
    // this election, AlreadyRecorded when it was revoked before.
    Task<string?> RevokePrintAsync(int electionId, Guid printId, int actorUserId, string reason, CancellationToken cancellationToken = default);
}
