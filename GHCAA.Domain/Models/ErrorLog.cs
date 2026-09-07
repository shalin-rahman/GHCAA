using System;

namespace GHCAA.Domain.Models
{
    // 45.2: captured from ExceptionMiddleware only (unhandled exceptions), not every ILogger call
    // app-wide — see docs/TODO.md 45.1 for the two options this was weighed against.
    public class ErrorLog
    {
        public int Id { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public string Level { get; set; } = null!; // Error / Warning
        public string Message { get; set; } = null!;
        public string? ExceptionType { get; set; }
        public string? StackTrace { get; set; }
        public string? Source { get; set; } // controller/middleware/class name
        public string? RequestPath { get; set; }
        public string? RequestMethod { get; set; }
        public int? UserId { get; set; }
        public string? Username { get; set; }
    }
}
