using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
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
            },
            new EmailTemplate
            {
                Code = "EVENT_PARTICIPATION_RECEIVED",
                Subject = "Participation Received: {{EventTitle}}",
                Description = "Initial acknowledgement for event registration",
                Body = "<div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px; max-width: 600px; margin: auto;'><h2 style='color: #2c3e50;'>Registration Received</h2><p>Dear <strong>{{FullName}}</strong>,</p><p>We have received your registration for <strong>{{EventTitle}}</strong>.</p><div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0; border: 1px dashed #c5a059;'><p><strong>Status:</strong> Pending Approval</p><p><strong>Reference:</strong> {{PassId}}</p></div><p>Our team will review your details/payment and send a final confirmation soon.</p></div>",
            },
            new EmailTemplate
            {
                Code = "EVENT_PARTICIPATION_APPROVED",
                Subject = "Participation Approved: {{EventTitle}}",
                Description = "Final confirmation with entry pass details",
                Body = "<div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px; max-width: 600px; margin: auto; background: #fff;'><h2 style='color: #27ae60;'>Registration Confirmed!</h2><p>Dear <strong>{{FullName}}</strong>,</p><p>Your participation in <strong>{{EventTitle}}</strong> has been officially approved.</p><div style='background: #111; color: #fff; padding: 25px; border-radius: 12px; margin: 25px 0; border: 2px solid #c5a059; text-align: center;'><h3 style='color: #c5a059; margin-top: 0; font-size: 1.2rem;'>ENTRY PASS</h3><div style='font-size: 1.1rem; font-weight: bold;'>{{FullName}}</div><div style='font-size: 0.8rem; margin: 10px 0; opacity: 0.7;'>Pass ID: {{PassId}}</div><div style='border-top: 1px solid rgba(255,255,255,0.1); margin: 15px 0; padding-top: 15px;'><p style='margin: 5px 0;'>📍 {{EventLocation}}</p><p style='margin: 5px 0;'>⏰ {{EventDate}}</p></div></div><p>Please present this email or your pass at the registration desk. We look forward to seeing you!</p></div>",
            },
            new EmailTemplate
            {
                Code = "APPLICATION_REJECTED",
                Subject = "Update on your GHCAA Membership Application",
                Description = "Rejection notice with reason",
                Body = "<div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'><h2 style='color: #e74c3c;'>Application Status Update</h2><p>Dear <strong>{{FullName}}</strong>,</p><p>Thank you for your interest in the GHC Alumni Association. After reviewing your application, we regret to inform you that we cannot approve it at this time.</p><div style='background: #fdf2f2; padding: 15px; border-radius: 5px; border-left: 5px solid #e74c3c;'><p><strong>Reason:</strong> {{Reason}}</p></div><p>If you believe this is an error, please contact the association office.</p></div>"
            },
            new EmailTemplate
            {
                Code = "PAYMENT_RECEIVED",
                Subject = "Payment Received: {{Amount}} BDT",
                Description = "Acknowledgment of payment submission",
                Body = "<div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'><h2 style='color: #2c3e50;'>Payment Received</h2><p>Dear <strong>{{FullName}}</strong>,</p><p>We have successfully received your payment. It is currently under verification.</p><div style='background: #f8f9fa; padding: 15px; border-radius: 5px;'><p><strong>Amount:</strong> {{Amount}} BDT</p><p><strong>Transaction ID:</strong> {{TrxID}}</p></div><p>You will be notified once the payment is verified.</p></div>"
            },
            new EmailTemplate
            {
                Code = "PAYMENT_STATUS_UPDATED",
                Subject = "Payment Status Updated: {{Status}}",
                Description = "Notification when payment is verified/rejected",
                Body = "<div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'><h2 style='color: #2c3e50;'>Payment Status Update</h2><p>Dear <strong>{{FullName}}</strong>,</p><p>The status of your transaction <strong>{{TrxID}}</strong> has been updated to <strong>{{Status}}</strong>.</p><p>Thank you for your contribution.</p></div>"
            },
            new EmailTemplate
            {
                Code = "FAMILY_LINK_REQUEST",
                Subject = "New Family Link Request from {{RequesterName}}",
                Description = "Request from another member to link accounts",
                Body = "<div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'><h2 style='color: #2c3e50;'>Family Link Request</h2><p>Hello <strong>{{FullName}}</strong>,</p><p><strong>{{RequesterName}}</strong> has requested to link their account with yours as a <strong>{{Relationship}}</strong>.</p><div style='text-align: center; margin: 30px 0;'><a href='{{ProfileUrl}}' style='background: #c5a059; color: #fff; padding: 12px 30px; text-decoration: none; border-radius: 8px; font-weight: bold;'>Review Request</a></div><p>Please log in to your profile to accept or decline this request.</p></div>"
            },
             new EmailTemplate
            {
                Code = "FAMILY_LINK_ACCEPTED",
                Subject = "Family Link Request Accepted",
                Description = "Confirmation that a family link was approved",
                Body = "<div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'><h2 style='color: #27ae60;'>Link Request Accepted</h2><p>Hello <strong>{{FullName}}</strong>,</p><p>Your family link request to <strong>{{TargetName}}</strong> has been accepted.</p><p>You are now connected in the GHCAA network.</p></div>"
            },
            new EmailTemplate
            {
                Code = "PORTAL_ENQUIRY",
                Subject = "New Portal Enquiry: {{Subject}}",
                Description = "Admin notification for contact form submissions",
                Body = "<div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px; max-width: 600px;'><h2 style='color: #2c3e50;'>New Portal Enquiry</h2><p><strong>From:</strong> {{RequesterName}} ({{RequesterEmail}})</p><p><strong>Subject:</strong> {{Subject}}</p><div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #c5a059;'><p>{{Message}}</p></div><p style='color: #888; font-size: 0.8rem;'>Submitted via GHCAA Portal</p></div>"
            }
        };

        private readonly IOrgConfigService _orgConfigService;

        public CommunicationService(ApplicationDbContext db, IEmailService emailService, ILogger<CommunicationService> logger, IOrgConfigService orgConfigService)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
            _orgConfigService = orgConfigService;
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
            var member = await _db.Members
                .Include(m => m.AcademicHistory)
                .Include(m => m.ProfessionalHistory)
                .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
            if (member == null) throw new KeyNotFoundException("Member not found");

            await SendEmailByCodeAsync(member.Email, templateCode, customVars, member, cancellationToken);
        }

        public async Task SendEmailByCodeAsync(string to, string templateCode, Dictionary<string, string>? customVars = null, Member? member = null, CancellationToken cancellationToken = default)
        {
            var template = await GetTemplateByCodeAsync(templateCode, cancellationToken);
            if (template == null)
            {
                _logger.LogWarning("Email template {TemplateCode} not found. Skipping email.", templateCode);
                return;
            }

            var vars = new Dictionary<string, string>();
            if (member != null)
            {
                vars["FullName"] = member.FullName;
                vars["MembershipNumber"] = member.MembershipNumber ?? "Pending";
                // ... more common vars could be added here if needed for all templates
            }

            if (customVars != null)
            {
                foreach (var kvp in customVars) vars[kvp.Key] = kvp.Value;
            }

            string subject = ReplacePlaceholders(template.Subject, vars);
            string body = ReplacePlaceholders(template.Body, vars);

            await SendAndLogEmailAsync(to, subject, body, templateCode, "Templated", cancellationToken);
        }

        public async Task SendBatchEmailAsync(IEnumerable<int> passingYears, string templateCode, Dictionary<string, string>? customVars = null, CancellationToken cancellationToken = default)
        {
            var members = await _db.Members
                .Include(m => m.AcademicHistory)
                .Include(m => m.ProfessionalHistory)
                .Where(m => m.AcademicHistory.Any(a => a.IsGHC && passingYears.Contains(a.PassingYear)) && !m.IsArchived)
                .ToListAsync(cancellationToken);

            foreach (var member in members)
            {
                await SendTemplatedEmailAsync(member.Email, member, templateCode, customVars, cancellationToken);
            }
        }

        public async Task SendTypeEmailAsync(IEnumerable<string> membershipTypes, string templateCode, Dictionary<string, string>? customVars = null, CancellationToken cancellationToken = default)
        {
            var members = await _db.Members
                .Include(m => m.AcademicHistory)
                .Include(m => m.ProfessionalHistory)
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
                    var member = await _db.Members
                        .Include(m => m.AcademicHistory)
                        .Include(m => m.ProfessionalHistory)
                        .FirstOrDefaultAsync(m => m.Email == email, cancellationToken);
                    await SendTemplatedEmailAsync(email, member, templateCode, customVars, cancellationToken);
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
                .Include(m => m.AcademicHistory)
                .Include(m => m.ProfessionalHistory)
                .Where(m => m.AcademicHistory.Any(a => a.IsGHC && passingYears.Contains(a.PassingYear)) && !m.IsArchived)
                .ToListAsync(cancellationToken);

            foreach (var member in members)
            {
                await SendAndLogEmailAsync(member.Email, subject, htmlBody, null, $"Batch: {string.Join(", ", passingYears)}", cancellationToken);
            }
        }

        public async Task SendTypeCustomEmailAsync(IEnumerable<string> membershipTypes, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            var members = await _db.Members
                .Include(m => m.AcademicHistory)
                .Include(m => m.ProfessionalHistory)
                .Where(m => membershipTypes.Contains(m.MembershipType.ToString()) && !m.IsArchived)
                .ToListAsync(cancellationToken);

            foreach (var member in members)
            {
                await SendAndLogEmailAsync(member.Email, subject, htmlBody, null, $"Types: {string.Join(", ", membershipTypes)}", cancellationToken);
            }
        }

        public async Task SendMemberCustomEmailAsync(int memberId, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members
                .Include(m => m.AcademicHistory)
                .Include(m => m.ProfessionalHistory)
                .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
            if (member == null) throw new KeyNotFoundException("Member not found");

            await SendAndLogEmailAsync(member.Email, subject, htmlBody, null, "Single Member", cancellationToken);
        }

        private async Task SendTemplatedEmailAsync(string to, Member? member, string templateCode, Dictionary<string, string>? customVars, CancellationToken cancellationToken)
        {
            var template = await GetTemplateByCodeAsync(templateCode, cancellationToken);
            if (template == null)
            {
                _logger.LogWarning("Email template {TemplateCode} not found. Skipping email to {To}.", templateCode, to);
                return;
            }

            var vars = new Dictionary<string, string>();

            if (member != null)
            {
                vars["FullName"] = member.FullName;
                vars["FatherName"] = member.FatherName;
                vars["MotherName"] = member.MotherName;
                vars["DateOfBirth"] = member.DateOfBirth.ToString("dd MMM yyyy");
                vars["Gender"] = member.Gender.ToString();
                vars["BloodGroup"] = member.BloodGroup.ToString();
                vars["NID"] = member.NID;
                vars["MembershipNumber"] = member.MembershipNumber ?? "Pending";
                vars["MembershipType"] = member.MembershipType.ToString();
                vars["Email"] = member.Email;
                vars["MobileNo"] = member.MobileNo;
                vars["PresentAddress"] = member.PresentAddress;
                vars["PermanentAddress"] = member.PermanentAddress;

                var ghc = member.AcademicHistory.FirstOrDefault(a => a.IsGHC);
                var hsc = member.AcademicHistory.FirstOrDefault(a => a.Degree == "HSC");
                var prof = member.ProfessionalHistory.FirstOrDefault(p => p.IsCurrent);

                vars["PassingYear"] = ghc?.PassingYear.ToString() ?? "N/A";
                vars["HSCAdmissionYear"] = hsc?.AdmissionYear.ToString() ?? "N/A";
                vars["SubjectGroup"] = ghc?.Subject ?? "N/A";
                vars["ProfessionalSector"] = prof?.Sector ?? "N/A";
                vars["Designation"] = prof?.Designation ?? "N/A";
            }

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
                var fullBody = body + await GetEmailFooterAsync();
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

        private async Task<string> GetEmailFooterAsync()
        {
            var config = await _orgConfigService.GetConfigAsync();
            var locale = config.Localization.Locales.TryGetValue(config.Localization.DefaultLocale, out var lp) ? lp : null;
            var tagline = locale?.Tagline ?? "Together We Thrive";

            return $@"
                <div style='margin-top: 40px; padding-top: 20px; border-top: 2px solid #e5c15e; font-family: sans-serif; color: #666;'>
                    <table width='100%' cellpadding='0' cellspacing='0'>
                        <tr>
                            <td style='vertical-align: middle; width: 60px;'>
                                <img src='{config.Branding.LogoUrl}' alt='{config.Branding.ShortName} Logo' style='width: 50px; height: 50px; border-radius: 50%;' />
                            </td>
                            <td style='vertical-align: middle; padding-left: 15px;'>
                                <div style='font-size: 16px; font-weight: 800; color: #111;'>{config.Branding.FullName}</div>
                                <div style='font-size: 12px; color: #c5a059;'>{tagline}</div>
                            </td>
                        </tr>
                    </table>
                    <div style='margin-top: 15px; font-size: 11px;'>
                        <p>Registered Office: {config.Contact.RegisteredOffice}</p>
                        <p>Enquiries: <a href='mailto:{config.Contact.SupportEmail}' style='color: #c5a059; text-decoration: none;'>{config.Contact.SupportEmail}</a></p>
                        <p style='color: #999; margin-top: 20px;'>&copy; {DateTime.UtcNow.Year} {config.Branding.MemberNickname.ToUpper()}. All rights reserved.</p>
                    </div>
                </div>";
        }
    }
}
