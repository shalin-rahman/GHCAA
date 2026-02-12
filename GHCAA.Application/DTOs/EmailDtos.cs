using System.Collections.Generic;

namespace GHCAA.Application.DTOs
{
    public class BulkEmailDto
    {
        public string TemplateCode { get; set; } = null!;
        public Dictionary<string, string>? CustomVars { get; set; }
        
        // Target filters
        public int? PassingYear { get; set; }
        public string? MembershipType { get; set; }
        public int? MemberId { get; set; }
    }

    public class CustomEmailDto
    {
        public List<string> Emails { get; set; } = new();
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
    }
}
