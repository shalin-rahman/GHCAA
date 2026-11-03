using System.ComponentModel.DataAnnotations;

namespace GHCAA.Domain.Models
{
    public class AcademicRecord
    {
        public int Id { get; set; }
        public int MemberId { get; set; }

        [Required]
        [MaxLength(200)]
        public string InstitutionName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Degree { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Subject { get; set; } = null!;

        public int? AdmissionYear { get; set; }
        public int PassingYear { get; set; }

        public bool IsOrgProfile { get; set; } // Flag for Govt. Haraganga College
        public string? Result { get; set; }
        public string? CertificatePath { get; set; }

        // Navigation
        public Member? Member { get; set; }

        // A member can hold more than one record from the organisation, for example HSC and then
        // a degree. Screens that show one year use the earliest.
        public static AcademicRecord? EarliestOrgRecord(IEnumerable<AcademicRecord>? records) =>
            records?.Where(a => a is { IsOrgProfile: true })
                .OrderBy(a => a.PassingYear <= 0 ? int.MaxValue : a.PassingYear)
                .ThenBy(a => a.Id)
                .FirstOrDefault();

        // Builds "HSC (Science) 2001 / BSc (Physics) 2004" from the organisation records,
        // earliest first. Returns null when the member has none.
        public static string? BatchLabel(IEnumerable<AcademicRecord>? records)
        {
            var parts = records?.Where(a => a is { IsOrgProfile: true })
                .OrderBy(a => a.PassingYear <= 0 ? int.MaxValue : a.PassingYear)
                .ThenBy(a => a.Id)
                .Select(FormatOne)
                .Where(p => p.Length > 0)
                .ToList();
            return parts is { Count: > 0 } ? string.Join(" / ", parts) : null;
        }

        private static string FormatOne(AcademicRecord a)
        {
            var degree = Clean(a.Degree);
            var subject = Clean(a.Subject);
            var year = a.PassingYear > 0 ? a.PassingYear.ToString(System.Globalization.CultureInfo.InvariantCulture) : null;
            var label = degree ?? string.Empty;
            if (subject != null) label = label.Length > 0 ? $"{label} ({subject})" : subject;
            if (year != null) label = label.Length > 0 ? $"{label} {year}" : year;
            return label;
        }

        // "None", "N/A" and "Unknown" are stored as placeholders by the import and older forms.
        private static string? Clean(string? value)
        {
            var t = value?.Trim();
            if (string.IsNullOrEmpty(t)) return null;
            return t.Equals("None", StringComparison.OrdinalIgnoreCase)
                || t.Equals("N/A", StringComparison.OrdinalIgnoreCase)
                || t.Equals("Unknown", StringComparison.OrdinalIgnoreCase) ? null : t;
        }
    }
}
