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

        // Sending logic
        Task SendIndividualEmailAsync(int memberId, string templateCode, Dictionary<string, string>? customVars = null, CancellationToken cancellationToken = default);
        Task SendBatchEmailAsync(int passingYear, string templateCode, Dictionary<string, string>? customVars = null, CancellationToken cancellationToken = default);
        Task SendTypeEmailAsync(string membershipType, string templateCode, Dictionary<string, string>? customVars = null, CancellationToken cancellationToken = default);
        
        // Custom one-off email
        Task SendCustomEmailAsync(IEnumerable<string> emails, string subject, string htmlBody, CancellationToken cancellationToken = default);
    }
}
