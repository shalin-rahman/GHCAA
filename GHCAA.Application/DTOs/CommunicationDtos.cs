using System;
using System.Collections.Generic;

namespace GHCAA.Application.DTOs;

public sealed class CommunicationLogDto
{
    public int Id { get; init; }
    public string Channel { get; init; } = null!;
    public string Subject { get; init; } = null!;
    public string Body { get; init; } = null!;
    public DateTime SentDate { get; init; }
    public string Status { get; init; } = null!;
    public string DeliveryScope { get; init; } = null!;
    public string? TargetAudience { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed class CommunicationLogPageDto
{
    public IReadOnlyList<CommunicationLogDto> Items { get; init; } = Array.Empty<CommunicationLogDto>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}
