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
        public async Task<IActionResult> GetActiveNews([FromQuery] Enums.ArticleCategory? articleCategory, CancellationToken cancellationToken)
        {
            var news = await _newsService.GetActiveNewsAsync(articleCategory, cancellationToken);
            return Ok(news);
        }

        [HttpGet("{id:int}")]
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

        [HttpGet("pending")]
        [HttpGet("admin/pending")]
        [HttpGet("News/Pending")] // Mobile Alias
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetPendingSubmissions(CancellationToken cancellationToken)
        {
            var news = await _newsService.GetPendingSubmissionsAsync(cancellationToken);
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

        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateNews(int id, [FromBody] UpdateNewsDto dto, CancellationToken cancellationToken)
        {
            dto.Id = id;
            var result = await _newsService.UpdateNewsAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteNews(int id, CancellationToken cancellationToken)
        {
            var success = await _newsService.DeleteNewsAsync(id, cancellationToken);
            return success ? Ok() : NotFound();
        }

        [HttpGet("my-submissions")]
        [Authorize]
        public async Task<IActionResult> GetMySubmissions(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            // We need to find the member ID for this user to call the service
            // Or we could update the service to accept userId. 
            // The service already uses user.MemberId if needed, but here it wants memberId.
            // Actually, I'll update the service to take userId directly or handle it there.
            // For now, I'll just pass the userId if it's the authorId.
            
            // Re-evaluating GetMySubmissionsAsync logic in NewsService:
            // It searches for user by memberId. 
            // Let's just bypass and use the authorId directly in a new service method or update it.
            // I'll update the service method to take userId.
            
            var news = await _newsService.GetMySubmissionsAsync(userId, cancellationToken); // I'll fix service next
            return Ok(news);
        }

        [HttpPost("submit")]
        [Authorize]
        public async Task<IActionResult> SubmitArticle([FromBody] CreateNewsDto dto, CancellationToken cancellationToken)
        {
            var authorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(authorIdClaim, out var authorId)) return Unauthorized();

            // Ensure status is Pending if submitted by member, or Draft if requested
            if (!User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                if (dto.Status != Enums.SubmissionStatus.Draft)
                    dto.Status = Enums.SubmissionStatus.Pending;
                
                dto.IsActive = false; // Members cannot set active directly
            }

            var result = await _newsService.CreateNewsAsync(dto, authorId, cancellationToken);
            return CreatedAtAction(nameof(GetNewsById), new { id = result.Id }, result);
        }

        [HttpPost("{id:int}/approve")]
        [HttpPost("admin/{id:int}/approve")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ApproveArticle(int id, CancellationToken cancellationToken)
        {
            var success = await _newsService.ApproveArticleAsync(id, cancellationToken);
            return success ? Ok(new { Message = "Article approved." }) : NotFound();
        }

        [HttpPost("{id:int}/reject")]
        [HttpPost("admin/{id:int}/reject")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> RejectArticle(int id, CancellationToken cancellationToken)
        {
            var success = await _newsService.RejectArticleAsync(id, cancellationToken);
            return success ? Ok(new { Message = "Article rejected." }) : NotFound();
        }

        [HttpPost("{id:int}/collaborators/{userId:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AddCollaborator(int id, int userId, CancellationToken cancellationToken)
        {
            var success = await _newsService.AddCollaboratorAsync(id, userId, cancellationToken);
            return success ? Ok(new { Message = "Collaborator added." }) : BadRequest("Could not add collaborator.");
        }

        [HttpDelete("{id:int}/collaborators/{userId:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> RemoveCollaborator(int id, int userId, CancellationToken cancellationToken)
        {
            var success = await _newsService.RemoveCollaboratorAsync(id, userId, cancellationToken);
            return success ? Ok(new { Message = "Collaborator removed." }) : NotFound();
        }

        [HttpPost("upload-image")]
        [Authorize] // Allow members to upload images for their articles too
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
                authorId, 
                Enums.FileUploadType.NewsImage, 
                cancellationToken
            );

            var fileUrl = "/" + relativePath.TrimStart('/');
            return Ok(new { url = fileUrl, relativePath });
        }
    }
}
