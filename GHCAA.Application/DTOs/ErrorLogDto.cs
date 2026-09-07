using System;

namespace GHCAA.Application.DTOs
{
    public class ErrorLogDto
    {
        public int Id { get; set; }
        public DateTime OccurredAt { get; set; }
        public string Level { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? ExceptionType { get; set; }
        public string? StackTrace { get; set; }
        public string? Source { get; set; }
        public string? RequestPath { get; set; }
        public string? RequestMethod { get; set; }
        public int? UserId { get; set; }
        public string? Username { get; set; }
    }

    public class ErrorLogFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Level { get; set; }
        public string? Query { get; set; } // free-text: message/exception-type/stack-trace substring
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
