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

        private static readonly List<EmailTemplate> DefaultTemplates = new()
        {
            new EmailTemplate 
            { 
                Code = "OTP_EMAIL", 
                Subject = "GHCAA Verification Code: {{OtpCode}}", 
                Description = "Security code for login/registration",
                Body = "<div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'><h2 style='color: #2c3e50;'>Verification Code</h2><p>Hello <strong>{{FullName}}</strong>,</p><p>Your security code is:</p><div style='font-size: 24px; font-weight: bold; background: #f8f9fa; padding: 15px; text-align: center; border-radius: 5px; color: #3498db;'>{{OtpCode}}</div><p>Valid for 10 minutes. Do not share this code.</p></div>",
            },
            new EmailTemplate 
            { 
                Code = "WELCOME_EMAIL", 
                Subject = "Welcome to GHC Alumni Association!", 
                Description = "Official induction message",
                Body = "<div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'><h2 style='color: #2c3e50;'>Welcome to GHCAA</h2><p>Dear <strong>{{FullName}}</strong>,</p><p>Your membership has been approved! We are excited to have you as part of our community.</p><div style='background: #e8f4fd; padding: 15px; border-radius: 5px;'><p><strong>Membership No:</strong> {{MembershipNumber}}</p><p><strong>Default Password:</strong> <code style='background:#fff; padding:2px 5px;'>{{DefaultPassword}}</code></p></div><p>Please log in and change your password immediately.</p></div>",
            },
            new EmailTemplate 
            { 
                Code = "FEE_REMINDER", 
                Subject = "Annual Membership Subscription Due", 
                Description = "Friendly reminder for yearly dues",
                Body = "<div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'><h2 style='color: #2c3e50;'>Subscription Reminder</h2><p>Dear <strong>{{FullName}}</strong>,</p><p>This is a reminder that your annual membership subscription is now due.</p><p>Maintaining an active status ensures you continue to receive all alumni benefits and voting rights.</p><p>Thank you for your continued support!</p></div>",
            },
            new EmailTemplate 
            { 
                Code = "PASSWORD_RESET", 
                Subject = "GHCAA Account Password Reset",
                Description = "Admin-initiated secure password reset link",
                Body = "<div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px; max-width: 600px; margin: auto;'><h2 style='color: #c5a059;'>Password Reset</h2><p>Hello <strong>{{FullName}}</strong>,</p><p>An administrator has initiated a password reset for your GHCAA account. Click below to set a new password — the link is valid for 24 hours.</p><div style='text-align: center; margin: 30px 0;'><a href='{{ResetUrl}}' style='background: #111; color: #c5a059; padding: 12px 30px; text-decoration: none; border-radius: 8px; font-weight: 800; display: inline-block; border: 1px solid #c5a059;'>Reset My Password</a></div><p style='color: #666; font-size: 0.9rem;'>If you did not request this, please ignore this email.</p></div>",
            }
        };

        public CommunicationService(ApplicationDbContext db, IEmailService emailService, ILogger<CommunicationService> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IEnumerable<EmailTemplate>> GetAllTemplatesAsync(CancellationToken cancellationToken = default)
        {
            var dbTemplates = await _db.EmailTemplates.ToListAsync(cancellationToken);
            var results = new List<EmailTemplate>(dbTemplates);

            foreach (var def in DefaultTemplates)
            {
                if (!results.Any(t => t.Code == def.Code))
                {
                    // Add codebase template as virtual entry (Id 0)
                    results.Add(new EmailTemplate 
                    { 
                        Id = 0, 
                        Code = def.Code, 
                        Subject = def.Subject, 
                        Body = def.Body, 
                        Description = def.Description + " (System Default)"
                    });
                }
            }

            return results;
        }

        public async Task<EmailTemplate?> GetTemplateByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            var template = await _db.EmailTemplates.FirstOrDefaultAsync(t => t.Code == code, cancellationToken);
            if (template != null) return template;

            // Fallback to codebase
            _logger.LogInformation("Template {Code} not found in database, falling back to codebase defaults.", code);
            return DefaultTemplates.FirstOrDefault(t => t.Code == code);
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
                { "HSCAdmissionYear", member.HSCAdmissionYear?.ToString() ?? "N/A" },
                { "SubjectGroup", member.GHCLastCertificateSubject },
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
                var fullBody = body + GetEmailFooter();
                await _emailService.SendEmailAsync(to, subject, fullBody, cancellationToken);
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

        private string GetEmailFooter()
        {
            return @"
                <div style='margin-top: 40px; padding-top: 20px; border-top: 2px solid #e5c15e; font-family: sans-serif; color: #666;'>
                    <table width='100%' cellpadding='0' cellspacing='0'>
                        <tr>
                            <td style='vertical-align: middle; width: 60px;'>
                                <img src='https://www.haragangacollege.edu.bd/assets/logo.png' alt='GHCAA Logo' style='width: 50px; height: 50px; border-radius: 50%;' />
                            </td>
                            <td style='vertical-align: middle; padding-left: 15px;'>
                                <div style='font-size: 16px; font-weight: 800; color: #111;'>Govt. Haraganga College Alumni Association</div>
                                <div style='font-size: 12px; color: #c5a059;'>Sharing Heritage, Aligning Lives, Integrating Networks</div>
                            </td>
                        </tr>
                    </table>
                    <div style='margin-top: 15px; font-size: 11px;'>
                        <p>Registered Office: Govt. Haraganga College Campus, Munshiganj, Bangladesh.</p>
                        <p>Enquiries: <a href='mailto:haragangian@gmail.com' style='color: #c5a059; text-decoration: none;'>haragangian@gmail.com</a></p>
                        <p style='color: #999; margin-top: 20px;'>&copy; 2025 HARAGANGIAN. All rights reserved.</p>
                    </div>
                </div>";
        }
    }
}
