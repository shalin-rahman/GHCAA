using GHCAA.API.Extensions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using static GHCAA.Domain.Enums;

namespace GHCAA.API.Controllers
{
    // 37.14s. Election pages and form layouts. Anyone can read what is published; only an
    // admin edits, and only on the web portal.
    [ApiController]
    [Route("api/election-content")]
    public class ElectionContentController : ControllerBase
    {
        private readonly IElectionContentService _service;

        public ElectionContentController(IElectionContentService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [OutputCache(PolicyName = Constants.OutputCachePolicies.PublicContent)]
        [HttpGet]
        public async Task<IActionResult> GetPublished([FromQuery] ElectionContentKind? kind, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetPublishedAsync(kind, cancellationToken));
        }

        [AllowAnonymous]
        [OutputCache(PolicyName = Constants.OutputCachePolicies.PublicContent)]
        [HttpGet("{kind}/{key}")]
        public async Task<IActionResult> GetPublishedByKey(ElectionContentKind kind, string key, CancellationToken cancellationToken)
        {
            var content = await _service.GetPublishedAsync(kind, key, cancellationToken);
            return content == null ? NotFound() : Ok(content);
        }

        [HttpGet("admin")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            return Ok(await _service.GetAllAsync(cancellationToken));
        }

        [HttpGet("admin/{id:int}")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var content = await _service.GetByIdAsync(id, cancellationToken);
            return content == null ? NotFound() : Ok(content);
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> Create([FromBody] UpsertElectionContentDto dto, CancellationToken cancellationToken)
        {
            if (!TryGetAdminId(out var adminId)) return Unauthorized();

            var (content, error) = await _service.CreateAsync(dto, adminId, cancellationToken);
            if (error != null) return Problem(detail: error, statusCode: StatusCodes.Status400BadRequest);
            return CreatedAtAction(nameof(GetById), new { id = content!.Id }, content);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> Update(int id, [FromBody] UpsertElectionContentDto dto, CancellationToken cancellationToken)
        {
            if (!TryGetAdminId(out var adminId)) return Unauthorized();

            try
            {
                var (content, error) = await _service.UpdateAsync(id, dto, adminId, cancellationToken);
                return error != null ? Problem(detail: error, statusCode: StatusCodes.Status400BadRequest) : Ok(content);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        private bool TryGetAdminId(out int adminId)
            => int.TryParse(this.CurrentUserIdRaw(), out adminId);
    }
}
