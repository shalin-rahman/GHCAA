using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using GHCAA.Domain;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/lookups")]
    public class LookupsController : ControllerBase
    {
        private readonly ILookupService _lookupService;
        private readonly IMemberService _memberService;

        public LookupsController(ILookupService lookupService, IMemberService memberService)
        {
            _lookupService = lookupService;
            _memberService = memberService;
        }

        [AllowAnonymous]
        [OutputCache(PolicyName = Constants.OutputCachePolicies.PublicReference)]
        [HttpGet("stats")]
        public async Task<IActionResult> GetPublicStats(CancellationToken cancellationToken)
        {
            var stats = await _memberService.GetPublicStatsAsync(cancellationToken);
            return Ok(stats);
        }

        [AllowAnonymous]
        [OutputCache(PolicyName = Constants.OutputCachePolicies.PublicReference)]
        [HttpGet]
        public async Task<IActionResult> GetAllLookups(CancellationToken cancellationToken)
        {
            var lookups = await _lookupService.GetAllLookupsAsync(cancellationToken);
            return Ok(lookups);
        }

        [AllowAnonymous]
        [OutputCache(PolicyName = Constants.OutputCachePolicies.PublicReference)]
        [HttpGet("{group}")]
        public async Task<IActionResult> GetByGroup(string group, CancellationToken cancellationToken)
        {
            var lookups = await _lookupService.GetByGroupAsync(group, cancellationToken);
            return Ok(lookups);
        }

        // Admin Management
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        [HttpPost]
        public async Task<IActionResult> CreateLookup([FromBody] LookupItem item, CancellationToken cancellationToken)
        {
            var result = await _lookupService.AddLookupItemAsync(item, cancellationToken);
            return CreatedAtAction(nameof(GetByGroup), new { group = result.LookupGroup }, result);
        }

        [Authorize(Policy = Constants.Policies.AdminOnly)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLookup(int id, [FromBody] LookupItem item, CancellationToken cancellationToken)
        {
            var success = await _lookupService.UpdateLookupItemAsync(id, item, cancellationToken);
            if (!success) return NotFound();
            return Ok(new { Message = "Lookup updated successfully" });
        }

        [Authorize(Policy = Constants.Policies.AdminOnly)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLookup(int id, CancellationToken cancellationToken)
        {
            var success = await _lookupService.DeleteLookupItemAsync(id, cancellationToken);
            if (!success) return NotFound();
            return Ok(new { Message = "Lookup deleted successfully" });
        }
    }
}
