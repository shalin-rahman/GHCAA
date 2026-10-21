using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces;

public interface IElectionDocumentService
{
    Task<byte[]?> GeneratePdfAsync(int electionId, string formCode, CancellationToken cancellationToken = default);

    // 37.14o. The voter's receipt. It carries no choices, and nothing about it is stored.
    Task<byte[]?> GenerateBallotReceiptPdfAsync(int electionId, string trackingCode, DateTime castAtUtc, CancellationToken cancellationToken = default);

    // 37.14n. Backs the verify page the form's QR code opens. Null when the election or the form code is unknown.
    Task<ElectionDocumentVerifyDto?> VerifyAsync(int electionId, string formCode, CancellationToken cancellationToken = default);
}
