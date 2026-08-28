using System.Security.Claims;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GHCAA.Domain;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/site-content")]
    public class SiteContentController : ControllerBase
    {
        private readonly ISiteContentService _service;

        public SiteContentController(ISiteContentService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetByGroup([FromQuery] string group = "about", CancellationToken cancellationToken = default)
        {
            var blocks = await _service.GetActiveByGroupAsync(group, cancellationToken);
            return Ok(blocks);
        }

        [HttpGet("admin")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var blocks = await _service.GetAllAsync(cancellationToken);
            return Ok(blocks);
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var block = await _service.GetByIdAsync(id, cancellationToken);
            return block == null ? NotFound() : Ok(block);
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> Create([FromBody] UpsertSiteContentDto dto, CancellationToken cancellationToken)
        {
            if (!TryGetAdminId(out var adminId)) return Unauthorized();

            var result = await _service.CreateAsync(dto, adminId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> Update(int id, [FromBody] UpsertSiteContentDto dto, CancellationToken cancellationToken)
        {
            if (!TryGetAdminId(out var adminId)) return Unauthorized();

            try
            {
                var result = await _service.UpdateAsync(id, dto, adminId, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var success = await _service.DeleteAsync(id, cancellationToken);
            return success ? Ok() : NotFound();
        }

        private bool TryGetAdminId(out int adminId)
            => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out adminId);
    }
}
