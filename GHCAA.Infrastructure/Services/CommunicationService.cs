using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    public class CommunicationService : ICommunicationService
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;
        private readonly ILogger<CommunicationService> _logger;

        public CommunicationService(ApplicationDbContext db, IEmailService emailService, ILogger<CommunicationService> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IEnumerable<EmailTemplate>> GetAllTemplatesAsync(CancellationToken cancellationToken = default)
        {
            return await _db.EmailTemplates.ToListAsync(cancellationToken);
        }

        public async Task<EmailTemplate?> GetTemplateByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _db.EmailTemplates.FirstOrDefaultAsync(t => t.Code == code, cancellationToken);
        }

        public async Task<EmailTemplate> UpdateTemplateAsync(EmailTemplate template, CancellationToken cancellationToken = default)
        {
            var existing = await _db.EmailTemplates.FindAsync(new object[] { template.Id }, cancellationToken);
            if (existing == null) throw new KeyNotFoundException("Template not found");

            existing.Subject = template.Subject;
            existing.Body = template.Body;
            existing.Variables = template.Variables;
            existing.LastUpdated = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return existing;
        }

        public async Task SendIndividualEmailAsync(int memberId, string templateCode, Dictionary<string, string>? customVars = null, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            if (member == null) throw new KeyNotFoundException("Member not found");

            await SendTemplatedEmailAsync(member.Email, member, templateCode, customVars, cancellationToken);
        }

        public async Task SendBatchEmailAsync(int passingYear, string templateCode, Dictionary<string, string>? customVars = null, CancellationToken cancellationToken = default)
        {
            var members = await _db.Members
                .Where(m => m.GHCLastCertificatePassingYear == passingYear && !m.IsArchived)
                .ToListAsync(cancellationToken);

            foreach (var member in members)
            {
                await SendTemplatedEmailAsync(member.Email, member, templateCode, customVars, cancellationToken);
            }
        }

        public async Task SendTypeEmailAsync(string membershipType, string templateCode, Dictionary<string, string>? customVars = null, CancellationToken cancellationToken = default)
        {
            // Assuming membershipType corresponds to the Enum value as string
            var members = await _db.Members
                .Where(m => m.MembershipType.ToString() == membershipType && !m.IsArchived)
                .ToListAsync(cancellationToken);

            foreach (var member in members)
            {
                await SendTemplatedEmailAsync(member.Email, member, templateCode, customVars, cancellationToken);
            }
        }

        public async Task SendCustomEmailAsync(IEnumerable<string> emails, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            foreach (var email in emails)
            {
                await _emailService.SendEmailAsync(email, subject, htmlBody, cancellationToken);
            }
        }

        public async Task SendMemberCustomEmailAsync(int memberId, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            if (member == null) throw new KeyNotFoundException("Member not found");

            await _emailService.SendEmailAsync(member.Email, subject, htmlBody, cancellationToken);
        }

        private async Task SendTemplatedEmailAsync(string to, Member member, string templateCode, Dictionary<string, string>? customVars, CancellationToken cancellationToken)
        {
            var template = await GetTemplateByCodeAsync(templateCode, cancellationToken);
            if (template == null)
            {
                _logger.LogWarning("Email template {TemplateCode} not found. Skipping email to {To}.", templateCode, to);
                return;
            }

            var vars = new Dictionary<string, string>
            {
                { "FullName", member.FullName },
                { "MembershipNumber", member.MembershipNumber ?? "Pending" },
                { "Email", member.Email },
                { "MobileNo", member.MobileNo },
                { "PassingYear", member.GHCLastCertificatePassingYear.ToString() }
            };

            if (customVars != null)
            {
                foreach (var kvp in customVars) vars[kvp.Key] = kvp.Value;
            }

            string subject = ReplacePlaceholders(template.Subject, vars);
            string body = ReplacePlaceholders(template.Body, vars);

            await _emailService.SendEmailAsync(to, subject, body, cancellationToken);
        }

        private string ReplacePlaceholders(string text, Dictionary<string, string> vars)
        {
            return Regex.Replace(text, @"\{\{(.+?)\}\}", m =>
            {
                string key = m.Groups[1].Value.Trim();
                return vars.TryGetValue(key, out string? value) ? value : m.Value;
            });
        }
    }
}
