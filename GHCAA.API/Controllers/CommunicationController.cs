using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/admin/comm")]
    [Authorize(Policy = "AdminOnly")]
    public class CommunicationController : ControllerBase
    {
        private readonly ICommunicationService _commService;

        public CommunicationController(ICommunicationService commService)
        {
            _commService = commService;
        }

        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates(CancellationToken cancellationToken)
        {
            var templates = await _commService.GetAllTemplatesAsync(cancellationToken);
            return Ok(templates);
        }

        [HttpPut("templates/{id}")]
        public async Task<IActionResult> UpdateTemplate(int id, [FromBody] EmailTemplate template, CancellationToken cancellationToken)
        {
            template.Id = id;
            var result = await _commService.UpdateTemplateAsync(template, cancellationToken);
            return Ok(result);
        }

        [HttpPost("send-individual")]
        public async Task<IActionResult> SendIndividual([FromBody] BulkEmailDto dto, CancellationToken cancellationToken)
        {
            if (!dto.MemberId.HasValue) return BadRequest("MemberId is required");
            await _commService.SendIndividualEmailAsync(dto.MemberId.Value, dto.TemplateCode, dto.CustomVars, cancellationToken);
            return Ok(new { Message = "Email queued for delivery" });
        }

        [HttpPost("send-batch")]
        public async Task<IActionResult> SendBatch([FromBody] BulkEmailDto dto, CancellationToken cancellationToken)
        {
            if (!dto.PassingYear.HasValue) return BadRequest("PassingYear is required");
            await _commService.SendBatchEmailAsync(dto.PassingYear.Value, dto.TemplateCode, dto.CustomVars, cancellationToken);
            return Ok(new { Message = $"Emails queued for batch {dto.PassingYear}" });
        }

        [HttpPost("send-type")]
        public async Task<IActionResult> SendType([FromBody] BulkEmailDto dto, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(dto.MembershipType)) return BadRequest("MembershipType is required");
            await _commService.SendTypeEmailAsync(dto.MembershipType, dto.TemplateCode, dto.CustomVars, cancellationToken);
            return Ok(new { Message = $"Emails queued for type {dto.MembershipType}" });
        }

        [HttpPost("send-custom")]
        public async Task<IActionResult> SendCustom([FromBody] CustomEmailDto dto, CancellationToken cancellationToken)
        {
            await _commService.SendCustomEmailAsync(dto.Emails, dto.Subject, dto.Body, cancellationToken);
            return Ok(new { Message = "Custom emails queued for delivery" });
        }

        [HttpPost("send-custom-to-member/{memberId}")]
        public async Task<IActionResult> SendCustomToMember(int memberId, [FromBody] CustomEmailDto dto, CancellationToken cancellationToken)
        {
            await _commService.SendMemberCustomEmailAsync(memberId, dto.Subject, dto.Body, cancellationToken);
            return Ok(new { Message = "Email sent to member" });
        }
    }
}
