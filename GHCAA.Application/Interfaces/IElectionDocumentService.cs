using GHCAA.Application.DTOs;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.Interfaces;

public interface IElectionDocumentService
{
    // 37.14s. A null mode uses the org's DefaultPrintMode. Null when the form or the election is unknown.
    Task<byte[]?> GeneratePdfAsync(int electionId, string formCode, ElectionPrintMode? mode = null, CancellationToken cancellationToken = default);

    // 37.14s. The form with ruled lines and no election, for printing ahead of time.
    Task<byte[]?> GenerateBlankPdfAsync(string formCode, CancellationToken cancellationToken = default);

    // 37.14o. The voter's receipt. It carries no choices, and nothing about it is stored.
    Task<byte[]?> GenerateBallotReceiptPdfAsync(int electionId, string trackingCode, DateTime castAtUtc, CancellationToken cancellationToken = default);

    // 37.14n. Backs the verify page the form's QR code opens. Null when the election or the form code is unknown.
    Task<ElectionDocumentVerifyDto?> VerifyAsync(int electionId, string formCode, CancellationToken cancellationToken = default);
}
