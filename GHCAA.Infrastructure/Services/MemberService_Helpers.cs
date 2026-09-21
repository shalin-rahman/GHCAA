using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    public partial class MemberService
    {
        private async Task<string> GetConfiguredInstitutionNameAsync(CancellationToken cancellationToken)
        {
            var name = (await _orgConfigService.GetConfigAsync()).Branding.InstitutionName?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("The institution name is not configured.");

            return name;
        }

        private static void EnforceInstitutionalAcademicRecord(
            IList<AcademicRecordDto> academicHistory,
            string institutionName)
        {
            if (academicHistory.Count == 0)
                throw new InvalidOperationException("At least one academic record is required.");

            var primary = academicHistory[0];
            if (!string.Equals(primary.InstitutionName?.Trim(), institutionName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"The first academic record must be for {institutionName}.");
            }

            if (academicHistory.Skip(1).Any(record =>
                record.IsOrgProfile ||
                string.Equals(record.InstitutionName?.Trim(), institutionName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    "Only the first academic record may be the institutional record.");
            }

            primary.InstitutionName = institutionName;
            primary.IsOrgProfile = true;
            foreach (var record in academicHistory.Skip(1))
                record.IsOrgProfile = false;
        }

        private string? MaskPii(string? value, int visibleStart = 4, int visibleEnd = 2)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (value.Length <= (visibleStart + visibleEnd)) return new string('*', Math.Max(value.Length, 6));

            var start = value.Substring(0, visibleStart);
            var end = value.Substring(value.Length - visibleEnd);
            var middle = new string('*', value.Length - (visibleStart + visibleEnd));
            return $"{start}{middle}{end}";
        }

        // --- Gamification & Profile Health Helpers ---

        private decimal CalculateProfileCompletion(Member member)
        {
            int totalFields = 13;
            int completedFields = 0;

            if (!string.IsNullOrEmpty(member.FullName)) completedFields++;
            if (!string.IsNullOrEmpty(member.Email)) completedFields++;
            if (!string.IsNullOrEmpty(member.MobileNo) && member.MobileNo != "TBD") completedFields++;
            if (member.DateOfBirth != default && member.DateOfBirth.Year > 1900) completedFields++;
            if (member.Gender != Enums.Gender.None) completedFields++;
            if (!string.IsNullOrEmpty(member.NID) && member.NID != "TBD") completedFields++;
            if (!string.IsNullOrEmpty(member.FatherName) && member.FatherName != "TBD") completedFields++;
            if (!string.IsNullOrEmpty(member.MotherName) && member.MotherName != "TBD") completedFields++;
            if (!string.IsNullOrEmpty(member.PermanentAddress) && member.PermanentAddress != "TBD") completedFields++;
            if (member.BloodGroup != Enums.BloodGroup.Unknown) completedFields++;
            if (!string.IsNullOrEmpty(member.PhotoPath)) completedFields++;

            var hasGhc = member.AcademicHistory?.Any(a => a.IsOrgProfile && a.PassingYear > 0) ?? false;
            if (hasGhc) completedFields++;

            var hasProfessional = member.ProfessionalHistory?.Any() ?? false;
            if (hasProfessional) completedFields++;

            return Math.Round((decimal)completedFields / totalFields * 100, 2);
        }

        // 30.28: mirrors the member dashboard's "Complete Your Profile" checklist exactly -
        // Identity & Photo / GHC History / Professional Info / Registration Payment, each
        // worth an equal 25% - so the profile page's percentage stat matches the checklist
        // step indicators the member actually sees. This is intentionally a separate, coarser
        // calculation from CalculateProfileCompletion, which drives the stricter 100%-complete
        // gate used before admin approval and must not be changed by this UI-facing metric.
        private decimal CalculateChecklistProfileCompletion(Member member, bool registrationPaymentCompleted)
        {
            const int totalSteps = 4;
            int completedSteps = 0;

            if (!string.IsNullOrEmpty(member.FullName) && !string.IsNullOrEmpty(member.PhotoPath)) completedSteps++;
            if (member.AcademicHistory != null && member.AcademicHistory.Any()) completedSteps++;
            if (member.ProfessionalHistory != null && member.ProfessionalHistory.Any()) completedSteps++;
            if (registrationPaymentCompleted) completedSteps++;

            return Math.Round((decimal)completedSteps / totalSteps * 100, 2);
        }

        private string GetCategoryBadge(int points)
        {
            if (points >= 1000) return "Legend";
            if (points >= 500) return "Elite";
            if (points >= 200) return "Active";
            return "Member";
        }

        private async Task<(int rank, string badge)> GetMemberGainsAsync(int memberId, int points, CancellationToken cancellationToken)
        {
            var rank = await _db.Members
                .Where(m => m.ContributionPoints > points && !m.IsArchived)
                .CountAsync(cancellationToken) + 1;

            return (rank, GetCategoryBadge(points));
        }

        public async Task<(Enums.MembershipStatus Status, Enums.MembershipType MembershipType)?> GetMembershipSnapshotAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            return member == null ? null : (member.Status, member.MembershipType);
        }
    }
}
