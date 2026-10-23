using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace GHCAA.Infrastructure.Services
{
    public class AssistantService : IAssistantService
    {
        private readonly ApplicationDbContext _db;
        private readonly IOrgConfigService _orgConfigService;

        // Query words mapped to a part of a sector name from PROFESSIONAL_SECTORS on the web.
        // First match wins.
        private static readonly (string[] Words, string Sector)[] SectorWords =
        {
            (new[] { "govt", "government", "public administration", "defense", "army" }, "Public Administration"),
            (new[] { "bank", "finance", "financial" }, "Banking"),
            (new[] { "education", "teacher", "professor", "research" }, "Education"),
            (new[] { "medical", "doctor", "health" }, "Medical"),
            (new[] { "software", "it sector", "information technology" }, "Information Technology"),
            (new[] { "garment", "rmg" }, "Garments"),
            (new[] { "legal", "lawyer", "consult" }, "Legal"),
        };

        public AssistantService(ApplicationDbContext db, IOrgConfigService orgConfigService)
        {
            _db = db;
            _orgConfigService = orgConfigService;
        }

        public async Task<AssistantResponseDto> AskAsync(string query, CancellationToken cancellationToken = default)
        {
            query = query.ToLower();
            var response = new AssistantResponseDto();
            var institutionName =
                (await _orgConfigService.GetConfigAsync()).Branding.InstitutionName;

            // Basic Intent Identification (Simulated NLP)
            bool isSearchAlumni = query.Contains("find") || query.Contains("search") || query.Contains("who") || query.Contains("alumni") || query.Contains("member");

            if (isSearchAlumni)
            {
                // Same base set as the member directory: active, not archived.
                var queryable = _db.Members.AsNoTracking()
                    .Where(m => m.Status == Domain.Enums.MembershipStatus.Active && !m.IsArchived);

                // A year means the passing year at this college, as in the directory filter.
                var matchYear = Regex.Match(query, @"\b(19|20)\d{2}\b");
                if (matchYear.Success && int.TryParse(matchYear.Value, out var year))
                    queryable = queryable.Where(m => m.AcademicHistory.Any(a => a.IsOrgProfile && a.PassingYear == year));

                var sector = SectorWords.FirstOrDefault(s => s.Words.Any(query.Contains)).Sector;
                if (sector != null)
                    queryable = queryable.Where(m => m.ProfessionalHistory.Any(p => p.IsCurrent && p.Sector != null && p.Sector.Contains(sector)));

                var results = await queryable.OrderBy(m => m.FullName).ThenBy(m => m.Id)
                    .Take(10)
                    .Select(m => new
                    {
                        m.Id,
                        m.FullName,
                        m.PhotoPath,
                        Year = m.AcademicHistory.Where(a => a.IsOrgProfile).Select(a => (int?)a.PassingYear).FirstOrDefault(),
                        Job = m.ProfessionalHistory.Where(p => p.IsCurrent).Select(p => new { p.Sector, p.Designation }).FirstOrDefault(),
                    })
                    .ToListAsync(cancellationToken);

                if (results.Any())
                {
                    response.Answer = $"I found {results.Count} members matching your criteria. Here are the top {institutionName} alumni:";
                    response.FoundMembers = results.Select(m => new MemberProfileDto
                    {
                        Id = m.Id,
                        FullName = m.FullName,
                        GHCLastCertificatePassingYear = m.Year,
                        ProfessionalSector = m.Job?.Sector,
                        Designation = m.Job?.Designation,
                        PhotoPath = m.PhotoPath
                    });
                }
                else
                {
                    response.Answer = "I couldn't find any alumni matching those specific details in our registry. Try a broader search like 'alumni from 2015' or 'members in banking'.";
                }
            }
            else if (query.Contains("help") || query.Contains("what can you do"))
            {
                response.Answer = $"I'm your {institutionName} alumni assistant. You can ask me to find specific alumni, e.g., 'Find members from 2010' or 'Search for alumni in banking'. I can also help with association rules or governance if you have questions!";
            }
            else
            {
                response.Answer = "Hello! I am the GHCAA AI Assistant. I can help you search the registry or answer questions about the association. How can I assist you today?";
            }

            return response;
        }
    }
}
