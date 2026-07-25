using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
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
        private readonly INotificationService _notificationService;

        public CommunicationController(ICommunicationService commService, INotificationService notificationService)
        {
            _commService = commService;
            _notificationService = notificationService;
        }

        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates(CancellationToken cancellationToken)
        {
            var templates = await _commService.GetAllTemplatesAsync(cancellationToken);
            return Ok(templates);
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs([FromQuery] int count = 100, CancellationToken cancellationToken = default)
        {
            var logs = await _commService.GetRecentLogsAsync(count, cancellationToken);
            return Ok(logs);
        }

        [HttpPut("templates/{id}")]
        public async Task<IActionResult> UpdateTemplate(int id, [FromBody] EmailTemplate template, CancellationToken cancellationToken)
        {
            template.Id = id;
            var result = await _commService.UpdateTemplateAsync(template, cancellationToken);
            return Ok(result);
        }

        [HttpPost("templates")]
        public async Task<IActionResult> CreateTemplate([FromBody] EmailTemplate template, CancellationToken cancellationToken)
        {
            var result = await _commService.CreateTemplateAsync(template, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("templates/{id}")]
        public async Task<IActionResult> DeleteTemplate(int id, CancellationToken cancellationToken)
        {
            await _commService.DeleteTemplateAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpPost("send-batch")]
        public async Task<IActionResult> SendBatch([FromBody] BulkEmailDto dto, CancellationToken cancellationToken)
        {
            var years = dto.PassingYears ?? (dto.PassingYear.HasValue ? new List<int> { dto.PassingYear.Value } : null);
            if (years == null || !years.Any()) return BadRequest("At least one PassingYear is required");

            await _commService.SendBatchEmailAsync(years, dto.TemplateCode, dto.CustomVars, cancellationToken);
            return Ok(new { Message = $"Emails queued for batches: {string.Join(", ", years)}" });
        }

        [HttpPost("send-type")]
        public async Task<IActionResult> SendType([FromBody] BulkEmailDto dto, CancellationToken cancellationToken)
        {
            var types = dto.MembershipTypes ?? (!string.IsNullOrEmpty(dto.MembershipType) ? new List<string> { dto.MembershipType } : null);
            if (types == null || !types.Any()) return BadRequest("At least one MembershipType is required");

            await _commService.SendTypeEmailAsync(types, dto.TemplateCode, dto.CustomVars, cancellationToken);
            return Ok(new { Message = $"Emails queued for types: {string.Join(", ", types)}" });
        }

        [HttpPost("send-custom")]
        public async Task<IActionResult> SendCustom([FromBody] CustomEmailDto dto, CancellationToken cancellationToken)
        {
            bool sendEmail = dto.Channel == "email" || dto.Channel == "both";
            bool sendPush = dto.Channel == "push" || dto.Channel == "both";

            if (dto.TargetMethod == "batch")
            {
                var years = dto.TargetValues?.Select(int.Parse).ToList() ?? (string.IsNullOrEmpty(dto.TargetValue) ? null : new List<int> { int.Parse(dto.TargetValue) });
                if (years == null) return BadRequest("Target batch values required");

                if (sendEmail) await _commService.SendBatchCustomEmailAsync(years, dto.Subject ?? "Broadcast Update", dto.Body ?? "", cancellationToken);
                if (sendPush) await _notificationService.BroadcastNotificationAsync(dto.Subject ?? "Broadcast Update", dto.Body ?? "", Enums.NotificationType.GeneralSystem, "/portal/notifications", cancellationToken);
            }
            else if (dto.TargetMethod == "type")
            {
                var types = dto.TargetValues ?? (string.IsNullOrEmpty(dto.TargetValue) ? null : new List<string> { dto.TargetValue });
                if (types == null) return BadRequest("Target membership type values required");

                if (sendEmail) await _commService.SendTypeCustomEmailAsync(types, dto.Subject ?? "Broadcast Update", dto.Body ?? "", cancellationToken);
                if (sendPush) await _notificationService.BroadcastNotificationAsync(dto.Subject ?? "Broadcast Update", dto.Body ?? "", Enums.NotificationType.GeneralSystem, "/portal/notifications", cancellationToken);
            }
            else
            {
                if (sendEmail) await _commService.SendCustomEmailAsync(dto.Emails, dto.TemplateCode, dto.Subject, dto.Body, null, cancellationToken);
            }

            return Ok(new { Message = "Communications queued for delivery via " + dto.Channel });
        }
    }
}
