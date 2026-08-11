using System.Collections.Generic;

namespace GHCAA.Application.DTOs
{
    public class BulkEmailDto
    {
        public string TemplateCode { get; set; } = null!;
        public Dictionary<string, string>? CustomVars { get; set; }

        // Target filters
        public int? PassingYear { get; set; }
        public List<int>? PassingYears { get; set; } // Support multiple
        public string? MembershipType { get; set; }
        public List<string>? MembershipTypes { get; set; } // Support multiple
        public int? MemberId { get; set; }
    }

    public class CustomEmailDto
    {
        public List<string> Emails { get; set; } = new();
        public string? TemplateCode { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }

        // Optional filters for manual message
        public string? TargetMethod { get; set; }
        public string? TargetValue { get; set; }
        public List<string>? TargetValues { get; set; } // Support multiple
        public string Channel { get; set; } = "email"; // "email", "push", "sms", "both"
    }
}
