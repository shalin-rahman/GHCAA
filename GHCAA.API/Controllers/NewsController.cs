using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IO;
using System;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/news")]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;
        private readonly IFileStorageService _fileStorageService;

        public NewsController(INewsService newsService, IFileStorageService fileStorageService)
        {
            _newsService = newsService;
            _fileStorageService = fileStorageService;
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

        [HttpPost("upload-image")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var authorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(authorIdClaim, out var authorId))
            {
                return Unauthorized();
            }

            using var stream = file.OpenReadStream();
            var extension = Path.GetExtension(file.FileName);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var uniqueFileName = $"news_{timestamp}_{Guid.NewGuid().ToString().Substring(0, 8)}{extension}";
            
            var relativePath = await _fileStorageService.SaveFileAsync(
                stream, 
                uniqueFileName, 
                authorId, // using admin's id as a folder categorization since it's an admin upload
                Enums.FileUploadType.NewsImage, 
                cancellationToken
            );

            // Our storage service typically returns local paths, add leading slash for web url
            var fileUrl = "/" + relativePath.TrimStart('/');
            
            return Ok(new { url = fileUrl, relativePath });
        }
    }
}
