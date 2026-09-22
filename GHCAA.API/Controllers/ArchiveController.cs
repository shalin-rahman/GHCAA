using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Extensions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/archive")]
    public class ArchiveController : ControllerBase
    {
        private readonly IArchiveService _archive;
        public ArchiveController(IArchiveService archive) => _archive = archive;

        [AllowAnonymous]
        [HttpGet("public")]
        public async Task<IActionResult> PublicCollections([FromQuery] string? search, CancellationToken ct)
            => Ok(await _archive.GetPublicCollectionsAsync(search, ct));

        [AllowAnonymous]
        [HttpGet("items/{id:int}")]
        public async Task<IActionResult> PublicItem(int id, CancellationToken ct)
        {
            var item = await _archive.GetPublicItemAsync(id, ct);
            return item == null ? NotFound() : Ok(item);
        }

        [Authorize(Policy = Constants.Policies.AdminOnly)]
        [HttpGet("admin/items")]
        public async Task<IActionResult> AdminItems([FromQuery] string? search, CancellationToken ct)
            => Ok(await _archive.GetAdminItemsAsync(search, ct));

        [Authorize(Policy = Constants.Policies.AdminOnly)]
        [HttpGet("admin/collections")]
        public async Task<IActionResult> AdminCollections(CancellationToken ct)
            => Ok(await _archive.GetAdminCollectionsAsync(ct));

        [Authorize(Policy = Constants.Policies.AdminOnly)]
        [HttpPost("admin/collections")]
        public async Task<IActionResult> CreateCollection([FromBody] ArchiveCollectionDto dto, CancellationToken ct)
        {
            if (!TryMemberId(out var memberId)) return Unauthorized();
            return Ok(await _archive.CreateCollectionAsync(dto, memberId, ct));
        }

        [Authorize(Policy = Constants.Policies.AdminOnly)]
        [HttpPut("admin/collections/{id:int}")]
        public async Task<IActionResult> UpdateCollection(int id, [FromBody] ArchiveCollectionDto dto, CancellationToken ct)
        {
            var result = await _archive.UpdateCollectionAsync(id, dto, ct);
            return result == null ? NotFound() : Ok(result);
        }

        [Authorize(Policy = Constants.Policies.AdminOnly)]
        [HttpDelete("admin/collections/{id:int}")]
        public async Task<IActionResult> DeleteCollection(int id, CancellationToken ct)
            => await _archive.DeleteCollectionAsync(id, ct) ? Ok() : NotFound();

        [Authorize]
        [HttpPost("items")]
        public async Task<IActionResult> CreateItem([FromBody] ArchiveItemDto dto, CancellationToken ct)
        {
            if (!TryMemberId(out var memberId)) return Unauthorized();
            if (!User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                dto.PublicationState = Enums.ArchivePublicationState.Draft;
                dto.ModerationState = Enums.ArchiveModerationState.Pending;
            }
            var result = await _archive.CreateItemAsync(dto, memberId, ct);
            return CreatedAtAction(nameof(PublicItem), new { id = result.Id }, result);
        }

        [Authorize(Policy = Constants.Policies.AdminOnly)]
        [HttpPut("admin/items/{id:int}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] ArchiveItemDto dto, CancellationToken ct)
        {
            var result = await _archive.UpdateItemAsync(id, dto, ct);
            return result == null ? NotFound() : Ok(result);
        }

        [Authorize(Policy = Constants.Policies.AdminOnly)]
        [HttpDelete("admin/items/{id:int}")]
        public async Task<IActionResult> DeleteItem(int id, CancellationToken ct)
            => await _archive.DeleteItemAsync(id, ct) ? Ok() : NotFound();

        [Authorize(Policy = Constants.Policies.AdminOnly)]
        [HttpPost("admin/items/{id:int}/moderate")]
        public async Task<IActionResult> ModerateItem(int id, [FromQuery] Enums.ArchiveModerationState state, [FromQuery] Enums.ArchivePublicationState publication, CancellationToken ct)
            => await _archive.ModerateItemAsync(id, state, publication, ct) ? Ok() : NotFound();

        private bool TryMemberId(out int memberId)
            => int.TryParse(this.CurrentMemberIdRaw(), out memberId);
    }
}
