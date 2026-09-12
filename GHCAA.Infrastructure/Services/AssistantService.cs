using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    public class AssistantService : IAssistantService
    {
        private readonly ApplicationDbContext _db;
        private readonly IOrgConfigService _orgConfigService;

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
                var queryable = _db.Members.AsQueryable();

                // Extract Potential Year (e.g. "2010")
                var matchYear = Regex.Match(query, @"\b(19|20)\d{2}\b");
                if (matchYear.Success && int.TryParse(matchYear.Value, out var year))
                {
                    //                     queryable = queryable.Where(m => m.GHCLastCertificatePassingYear == year);
                }

                // Extract Potential Sector
                //                 if (query.Contains("corporate")) queryable = queryable.Where(m => m.ProfessionalSector == "Corporate");
                //                 else if (query.Contains("govt") || query.Contains("government")) queryable = queryable.Where(m => m.ProfessionalSector == "Govt. Service");
                //                 else if (query.Contains("business")) queryable = queryable.Where(m => m.ProfessionalSector == "Business");
                //                 else if (query.Contains("education") || query.Contains("teacher")) queryable = queryable.Where(m => m.ProfessionalSector == "Education");
                //                 else if (query.Contains("medical") || query.Contains("doctor")) queryable = queryable.Where(m => m.ProfessionalSector == "Medical");

                var results = await queryable.Take(10).ToListAsync(cancellationToken);

                if (results.Any())
                {
                    response.Answer = $"I found {results.Count} members matching your criteria. Here are the top {institutionName} alumni:";
                    response.FoundMembers = results.Select(m => new MemberProfileDto
                    {
                        Id = m.Id,
                        FullName = m.FullName,
                        //                         GHCLastCertificatePassingYear = m.GHCLastCertificatePassingYear,
                        //                         ProfessionalSector = m.ProfessionalSector,
                        //                         Designation = m.Designation,
                        PhotoPath = m.PhotoPath
                    });
                }
                else
                {
                    response.Answer = "I couldn't find any alumni matching those specific details in our registry. Try a broader search like 'alumni from 2015' or 'members in Corporate'.";
                }
            }
            else if (query.Contains("help") || query.Contains("what can you do"))
            {
                response.Answer = $"I'm your {institutionName} alumni assistant. You can ask me to find specific alumni, e.g., 'Find members from 2010' or 'Search for alumni in Corporate sector'. I can also help with association rules or governance if you have questions!";
            }
            else
            {
                response.Answer = "Hello! I am the GHCAA AI Assistant. I can help you search the registry or answer questions about the association. How can I assist you today?";
            }

            return response;
        }
    }
}
