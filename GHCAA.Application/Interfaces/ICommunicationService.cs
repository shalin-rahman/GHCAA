using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface ICommunicationService
    {
        // Email Templates CRUD
        Task<IEnumerable<EmailTemplate>> GetAllTemplatesAsync(CancellationToken cancellationToken = default);
        Task<EmailTemplate?> GetTemplateByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<EmailTemplate> UpdateTemplateAsync(EmailTemplate template, CancellationToken cancellationToken = default);
        Task<EmailTemplate> CreateTemplateAsync(EmailTemplate template, CancellationToken cancellationToken = default);
        Task DeleteTemplateAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<EmailLog>> GetRecentLogsAsync(int count = 100, CancellationToken cancellationToken = default);

        // Sending logic
        Task SendIndividualEmailAsync(int memberId, string templateCode, Dictionary<string, string>? customVars = null, CancellationToken cancellationToken = default);
        Task SendBatchEmailAsync(IEnumerable<int> passingYears, string templateCode, Dictionary<string, string>? customVars = null, CancellationToken cancellationToken = default);
        Task SendTypeEmailAsync(IEnumerable<string> membershipTypes, string templateCode, Dictionary<string, string>? customVars = null, CancellationToken cancellationToken = default);

        // Custom one-off email
        Task SendCustomEmailAsync(IEnumerable<string> emails, string? templateCode, string? subject, string? htmlBody, Dictionary<string, string>? customVars = null, CancellationToken cancellationToken = default);
        Task SendBatchCustomEmailAsync(IEnumerable<int> passingYears, string subject, string htmlBody, CancellationToken cancellationToken = default);
        Task SendTypeCustomEmailAsync(IEnumerable<string> membershipTypes, string subject, string htmlBody, CancellationToken cancellationToken = default);
        Task SendMemberCustomEmailAsync(int memberId, string subject, string htmlBody, CancellationToken cancellationToken = default);
        Task SendEmailByCodeAsync(string to, string templateCode, Dictionary<string, string>? customVars = null, Member? member = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolves an EmailTemplate's Subject/Body against the same variable set SendEmailByCodeAsync
        /// uses (member fields + org branding + customVars), without sending anything. Returns null if
        /// no template with that code exists — callers decide their own fallback text.
        /// </summary>
        Task<(string Subject, string Body)?> ResolveTemplateTextAsync(string templateCode, int? memberId = null, Dictionary<string, string>? customVars = null, CancellationToken cancellationToken = default);
    }
}
