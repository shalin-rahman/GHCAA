using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GHCAA.Domain;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/admin/members/import")]
    [Authorize(Policy = Constants.Policies.AdminOnly)]
    public class MemberImportController : ControllerBase
    {
        private readonly IMemberImportService _importService;
        private readonly IFileValidationService _fileValidationService;

        public MemberImportController(IMemberImportService importService, IFileValidationService fileValidationService)
        {
            _importService = importService;
            _fileValidationService = fileValidationService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Import([FromForm] MemberImportRequestDto request, CancellationToken cancellationToken)
        {
            var excelValidation = _fileValidationService.ValidateFormFile(request.ExcelFile, FileCategory.Spreadsheet, 10 * 1024 * 1024);
            if (!excelValidation.IsValid) return Problem(detail: excelValidation.ErrorMessage, statusCode: StatusCodes.Status400BadRequest);

            foreach (var photo in request.Photos)
            {
                var photoValidation = _fileValidationService.ValidateFormFile(photo, FileCategory.Image, 5 * 1024 * 1024);
                if (!photoValidation.IsValid) return Problem(detail: photoValidation.ErrorMessage, statusCode: StatusCodes.Status400BadRequest);
            }

            var result = await _importService.ImportMembersAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export(CancellationToken cancellationToken)
        {
            var bytes = await _importService.ExportMembersToExcelAsync(cancellationToken);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Members_Registry_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}
