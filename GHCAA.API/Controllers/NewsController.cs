using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/news")]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;

        public NewsController(INewsService newsService)
        {
            _newsService = newsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveNews(CancellationToken cancellationToken)
        {
            var news = await _newsService.GetActiveNewsAsync(cancellationToken);
            return Ok(news);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNewsById(int id, CancellationToken cancellationToken)
        {
            var post = await _newsService.GetNewsByIdAsync(id, cancellationToken);
            return post == null ? NotFound() : Ok(post);
        }

        [HttpGet("admin")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAllNewsForAdmin(CancellationToken cancellationToken)
        {
            var news = await _newsService.GetAllNewsForAdminAsync(cancellationToken);
            return Ok(news);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateNews([FromBody] CreateNewsDto dto, CancellationToken cancellationToken)
        {
            var authorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(authorIdClaim, out var authorId))
            {
                return Unauthorized();
            }

            var result = await _newsService.CreateNewsAsync(dto, authorId, cancellationToken);
            return CreatedAtAction(nameof(GetNewsById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateNews(int id, [FromBody] UpdateNewsDto dto, CancellationToken cancellationToken)
        {
            dto.Id = id;
            var result = await _newsService.UpdateNewsAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteNews(int id, CancellationToken cancellationToken)
        {
            var success = await _newsService.DeleteNewsAsync(id, cancellationToken);
            return success ? Ok() : NotFound();
        }
    }
}
