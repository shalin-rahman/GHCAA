using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace GHCAA.Application.DTOs
{
    public class MemberImportRequestDto
    {
        public IFormFile ExcelFile { get; set; } = null!;
        public List<IFormFile> Photos { get; set; } = new();

        // JSON string mapping: { "ExcelColumnName": "SystemPropertyName" }
        public string ColumnMappingJson { get; set; } = "{}";

        // JSON string naming: { "SystemPropertyName": "DefaultValue" }
        public string DefaultValuesJson { get; set; } = "{}";
    }

    public class MemberImportResultDto
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
