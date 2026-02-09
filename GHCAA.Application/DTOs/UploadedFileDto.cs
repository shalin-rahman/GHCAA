using System.IO;

namespace GHCAA.Application.DTOs
{
    // Lightweight, transport-safe file DTO so Application layer does not depend on ASP.NET types
    public class UploadedFileDto
    {
        public Stream Content { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long Length { get; set; }
    }
}