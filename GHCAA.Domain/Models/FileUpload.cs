using System;
using static GHCAA.Domain.Enums;

namespace GHCAA.Domain.Models
{
    public class FileUpload
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public FileUploadType UploadType { get; set; }
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!; // web-relative path (uploads/...)
        public long SizeBytes { get; set; }
        public FileUploadStatus Status { get; set; } = FileUploadStatus.Pending;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Member? Member { get; set; }
    }
}