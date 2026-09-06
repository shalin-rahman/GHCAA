using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/assistant")]
    [Authorize]
    public class AssistantController : ControllerBase
    {
        private readonly IAssistantService _assistantService;

        public AssistantController(IAssistantService assistantService)
        {
            _assistantService = assistantService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AssistantQueryDto dto, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(dto.Query))
                return Problem(detail: "Query cannot be empty.", statusCode: StatusCodes.Status400BadRequest);

            var response = await _assistantService.AskAsync(dto.Query, cancellationToken);
            return Ok(response);
        }
    }

    public class AssistantQueryDto
    {
        public string Query { get; set; } = null!;
    }
}
