using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using GHCAA.Application.Security;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All forum actions require authentication
    public class ForumController : ControllerBase
    {
        private readonly IForumService _forumService;

        public ForumController(IForumService forumService)
        {
            _forumService = forumService;
        }

        [HttpGet("categories")]
        public async Task<ActionResult<List<ForumCategoryDto>>> GetCategories()
        {
            return Ok(await _forumService.GetCategoriesAsync());
        }

        [HttpGet("categories/{categoryId}/topics")]
        public async Task<ActionResult<List<ForumTopicDto>>> GetTopics(int categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            return Ok(await _forumService.GetTopicsByCategoryAsync(categoryId, page, pageSize));
        }

        [HttpGet("topics/{topicId}")]
        public async Task<ActionResult<ForumTopicDto>> GetTopic(int topicId)
        {
            var topic = await _forumService.GetTopicByIdAsync(topicId);
            if (topic == null) return NotFound("Topic not found.");

            return Ok(topic);
        }

        [HttpGet("topics/{topicId}/posts")]
        public async Task<ActionResult<List<ForumPostDto>>> GetPosts(int topicId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            return Ok(await _forumService.GetPostsByTopicAsync(topicId, page, pageSize));
        }

        [HttpPost("topics")]
        public async Task<ActionResult<ForumTopicDto>> CreateTopic([FromBody] CreateForumTopicDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();

            var topic = await _forumService.CreateTopicAsync(dto, memberId);
            return CreatedAtAction(nameof(GetTopic), new { topicId = topic.Id }, topic);
        }

        [HttpPost("topics/{topicId}/posts")]
        public async Task<ActionResult<ForumPostDto>> CreatePost(int topicId, [FromBody] CreateForumPostDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (dto.TopicId != topicId) return BadRequest("Topic ID mismatch.");

            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();

            var post = await _forumService.CreatePostAsync(dto, memberId);
            return Ok(post);
        }

        [HttpDelete("topics/{topicId}")]
        public async Task<IActionResult> DeleteTopic(int topicId)
        {
            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();

            var isSuperAdmin = User.IsInRole("SuperAdmin");

            await _forumService.DeleteTopicAsync(topicId, memberId, isSuperAdmin);
            return NoContent();
        }

        [HttpDelete("posts/{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();

            var isSuperAdmin = User.IsInRole("SuperAdmin");

            await _forumService.DeletePostAsync(postId, memberId, isSuperAdmin);
            return NoContent();
        }

        private int GetMemberId()
        {
            var claim = User.FindFirst(AppClaimTypes.MemberId);
            if (claim != null && int.TryParse(claim.Value, out int id))
            {
                return id;
            }
            return 0;
        }
    }
}
