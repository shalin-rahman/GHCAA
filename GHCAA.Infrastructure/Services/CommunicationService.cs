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
            existing.Code = template.Code;
            existing.Description = template.Description;
            existing.Variables = template.Variables;
            existing.LastUpdated = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return existing;
        }

        public async Task<EmailTemplate> CreateTemplateAsync(EmailTemplate template, CancellationToken cancellationToken = default)
        {
            template.LastUpdated = DateTime.UtcNow;
            _db.EmailTemplates.Add(template);
            await _db.SaveChangesAsync(cancellationToken);
            return template;
        }

        public async Task DeleteTemplateAsync(int id, CancellationToken cancellationToken = default)
        {
            var template = await _db.EmailTemplates.FindAsync(new object[] { id }, cancellationToken);
            if (template != null)
            {
                _db.EmailTemplates.Remove(template);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<EmailLog>> GetRecentLogsAsync(int count = 100, CancellationToken cancellationToken = default)
        {
            return await _db.EmailLogs
                .OrderByDescending(l => l.SentDate)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task SendIndividualEmailAsync(int memberId, string templateCode, Dictionary<string, string>? customVars = null, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            if (member == null) throw new KeyNotFoundException("Member not found");

            await SendTemplatedEmailAsync(member.Email, member, templateCode, customVars, cancellationToken);
        }

        public async Task SendBatchEmailAsync(IEnumerable<int> passingYears, string templateCode, Dictionary<string, string>? customVars = null, CancellationToken cancellationToken = default)
        {
            var members = await _db.Members
                .Where(m => passingYears.Contains(m.GHCLastCertificatePassingYear) && !m.IsArchived)
                .ToListAsync(cancellationToken);

            foreach (var member in members)
            {
                await SendTemplatedEmailAsync(member.Email, member, templateCode, customVars, cancellationToken);
            }
        }

        public async Task SendTypeEmailAsync(IEnumerable<string> membershipTypes, string templateCode, Dictionary<string, string>? customVars = null, CancellationToken cancellationToken = default)
        {
            var members = await _db.Members
                .Where(m => membershipTypes.Contains(m.MembershipType.ToString()) && !m.IsArchived)
                .ToListAsync(cancellationToken);

            foreach (var member in members)
            {
                await SendTemplatedEmailAsync(member.Email, member, templateCode, customVars, cancellationToken);
            }
        }

        public async Task SendCustomEmailAsync(IEnumerable<string> emails, string? templateCode, string? subject, string? htmlBody, Dictionary<string, string>? customVars = null, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(templateCode))
            {
                foreach (var email in emails)
                {
                    var member = await _db.Members.FirstOrDefaultAsync(m => m.Email == email, cancellationToken);
                    if (member != null)
                    {
                        await SendTemplatedEmailAsync(email, member, templateCode, customVars, cancellationToken);
                    }
                    else
                    {
                        // Fallback for non-members if template permits or just log
                        _logger.LogWarning("Member with email {Email} not found for templated broadcast. Skipping.", email);
                    }
                }
            }
            else
            {
                foreach (var email in emails)
                {
                    await SendAndLogEmailAsync(email, subject ?? "", htmlBody ?? "", null, "Manual List", cancellationToken);
                }
            }
        }

        public async Task SendBatchCustomEmailAsync(IEnumerable<int> passingYears, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            var members = await _db.Members
                .Where(m => passingYears.Contains(m.GHCLastCertificatePassingYear) && !m.IsArchived)
                .ToListAsync(cancellationToken);

            foreach (var member in members)
            {
                await SendAndLogEmailAsync(member.Email, subject, htmlBody, null, $"Batch: {string.Join(", ", passingYears)}", cancellationToken);
            }
        }

        public async Task SendTypeCustomEmailAsync(IEnumerable<string> membershipTypes, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            var members = await _db.Members
                .Where(m => membershipTypes.Contains(m.MembershipType.ToString()) && !m.IsArchived)
                .ToListAsync(cancellationToken);

            foreach (var member in members)
            {
                await SendAndLogEmailAsync(member.Email, subject, htmlBody, null, $"Types: {string.Join(", ", membershipTypes)}", cancellationToken);
            }
        }

        public async Task SendMemberCustomEmailAsync(int memberId, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            if (member == null) throw new KeyNotFoundException("Member not found");

            await SendAndLogEmailAsync(member.Email, subject, htmlBody, null, "Single Member", cancellationToken);
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
                { "FatherName", member.FatherName },
                { "MotherName", member.MotherName },
                { "DateOfBirth", member.DateOfBirth.ToString("dd MMM yyyy") },
                { "Gender", member.Gender.ToString() },
                { "BloodGroup", member.BloodGroup.ToString() },
                { "NID", member.NID },
                { "MembershipNumber", member.MembershipNumber ?? "Pending" },
                { "MembershipType", member.MembershipType.ToString() },
                { "Email", member.Email },
                { "MobileNo", member.MobileNo },
                { "PresentAddress", member.PresentAddress },
                { "PermanentAddress", member.PermanentAddress },
                { "PassingYear", member.GHCLastCertificatePassingYear.ToString() },
                { "HSCAdmissionYear", member.HSCAdmissionYear.ToString() },
                { "SubjectGroup", member.SubjectGroup },
                { "ProfessionalSector", member.ProfessionalSector },
                { "Designation", member.Designation }
            };

            if (customVars != null)
            {
                foreach (var kvp in customVars) vars[kvp.Key] = kvp.Value;
            }

            string subject = ReplacePlaceholders(template.Subject, vars);
            string body = ReplacePlaceholders(template.Body, vars);

            await SendAndLogEmailAsync(to, subject, body, templateCode, "Templated Broadcast", cancellationToken);
        }

        private async Task SendAndLogEmailAsync(string to, string subject, string body, string? templateCode, string targetAudience, CancellationToken cancellationToken)
        {
            var log = new EmailLog
            {
                RecipientEmail = to,
                Subject = subject,
                Body = body,
                TemplateCode = templateCode,
                TargetAudience = targetAudience,
                SentDate = DateTime.UtcNow,
                Status = "Sent"
            };

            try
            {
                await _emailService.SendEmailAsync(to, subject, body, cancellationToken);
            }
            catch (Exception ex)
            {
                log.Status = "Failed";
                log.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to send email to {To}", to);
            }

            _db.EmailLogs.Add(log);
            await _db.SaveChangesAsync(cancellationToken);
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
